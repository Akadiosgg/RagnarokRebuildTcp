using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.PlayerControl;
using Assets.Scripts.Sprites;
using Assets.Scripts.UI.ConfigWindow;
using RebuildSharedData.Enum;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Hud
{
    public class MinimapController : MonoBehaviour, IScrollHandler, IPointerMoveHandler, IPointerExitHandler
    {
        public RectTransform Viewport;
        public RectTransform ContentContainer;

        public Image MapImage;
        public Material OverworldMaterial;
        public Material DungeonMaterial;

        public CanvasGroup IconLayer;
        //keeps entity icons below the player marker
        public RectTransform EntityIconContainer;

        //also used for party members
        public UiPlayerSprite PlayerIconPrefab;
        public Image PlayerPrefab;
        public Image MonsterPrefab;
        public Image BossPrefab;
        public Image MvpPrefab;
        public Image PortalPrefab;
        public Image WarpNpcPrefab;
        public Image KafraPrefab;
        public Image TalkNpcPrefab;

        public float IconSizeMultiplier = 1f;
        public float MouseWheelZoomStep = 0.2f;

        private static MinimapController instance;
        private static readonly int SecondaryTex = Shader.PropertyToID("_SecondaryTex");

        private RectTransform selfRect;
        private Vector2 maxSize;

        private Coroutine loadCoroutine;
        private MapType mapType;
        private Sprite mapSprite;
        private Sprite walkSprite;
        private int mapPixelWidth;
        private int mapPixelHeight;

        private float minZoom;
        private float maxZoom;
        private float zoom;

        private readonly Dictionary<int, MinimapEntityData> entities = new();

        private UiPlayerSprite playerIcon;
        private Vector2Int lastIconPosition = new(-1, -1);

        private bool pointerInside;
        private Vector2 pointerPosition;
        private Camera pointerCamera;
        private MinimapEntityData hoveredEntity;

        private class MinimapEntityData
        {
            public RectTransform Icon;
            public Vector2Int Position;
            public CharacterDisplayType Type;
            public string Label;
        }

        public static MinimapController Instance
        {
            get
            {
                if (instance != null)
                    return instance;
                //include inactive: the minimap switches itself off
                instance = FindAnyObjectByType<MinimapController>(FindObjectsInactive.Include);
                return instance;
            }
        }

        //entities for a loading map are tracked but not drawn until LoadMinimap
        public void UnloadMap()
        {
            ResetView();
            entities.Clear();
        }

        private void ResetView()
        {
            mapSprite = null;
            walkSprite = null;
            ClearIconViews();
        }

        private void ClearIconViews()
        {
            foreach (var pair in entities)
                DestroyIcon(pair.Value);

            if (playerIcon != null)
            {
                Destroy(playerIcon.gameObject);
                playerIcon = null;
            }

            //makes the next SetPlayerPosition place the new marker
            lastIconPosition = new Vector2Int(-1, -1);
        }

        public void RemoveEntity(int entityId)
        {
            if (entities.Remove(entityId, out var entity))
                DestroyIcon(entity);
        }

        private void DestroyIcon(MinimapEntityData entity)
        {
            if (hoveredEntity == entity)
                SetHovered(null);

            if (entity.Icon != null)
                Destroy(entity.Icon.gameObject);

            entity.Icon = null;
        }

        private void RefreshPartyIcon(int entityId)
        {
            if (entities.TryGetValue(entityId, out var entity)
                && entity.Icon != null
                && entity.Type == CharacterDisplayType.Player)
                CreateIcon(entityId, entity);
        }

        //counter-scales icons against zoom so they keep their on-screen size
        private float IconScale => IconSizeMultiplier / zoom;

        private void ApplyIconScale(Component icon) =>
            icon.transform.localScale = Vector3.one * IconScale;

        public void OnPointerMove(PointerEventData eventData)
        {
            pointerInside = true;
            pointerPosition = eventData.position;
            pointerCamera = eventData.enterEventCamera;

            UpdateHover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerInside = false;
            SetHovered(null);
        }

        //no pointer exit arrives when the minimap is switched off mid-hover
        private void OnDisable() => OnPointerExit(null);

        private void UpdateHover()
        {
            var ui = UiManager.Instance;

            if (!pointerInside || mapSprite == null || ui == null || ui.IsDraggingItem)
            {
                SetHovered(null);
                return;
            }

            SetHovered(EntityUnderPointer());
        }

        private MinimapEntityData EntityUnderPointer()
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(MapImage.rectTransform, pointerPosition, pointerCamera, out var local))
                return null;

            MinimapEntityData nearest = null;
            var nearestDistance = float.MaxValue;

            var iconScale = IconScale;

            foreach (var pair in entities)
            {
                var entity = pair.Value;

                if (entity.Label == null || entity.Icon == null)
                    continue;

                var half = entity.Icon.rect.size * (0.5f * iconScale);
                var delta = local - (Vector2)MapPosition(entity.Position);

                if (Mathf.Abs(delta.x) > half.x || Mathf.Abs(delta.y) > half.y)
                    continue;

                var distance = delta.sqrMagnitude;
                if (distance >= nearestDistance)
                    continue;

                nearestDistance = distance;
                nearest = entity;
            }

            return nearest;
        }

        private void SetHovered(MinimapEntityData entity)
        {
            if (hoveredEntity == entity)
                return;

            var ui = UiManager.Instance;

            if (hoveredEntity != null && hoveredEntity.Icon != null && ui != null)
                ui.HideTooltip(hoveredEntity.Icon.gameObject);

            hoveredEntity = entity;

            if (hoveredEntity != null && hoveredEntity.Icon != null && ui != null)
                ui.ShowTooltip(hoveredEntity.Icon.gameObject, hoveredEntity.Label);
        }

        public void SetEntityPosition(int entityId, CharacterDisplayType type, Vector2Int pos)
        {
            if (!entities.TryGetValue(entityId, out var entity))
            {
                entity = new MinimapEntityData { Type = type };
                entities.Add(entityId, entity);
            }
            else if (entity.Icon != null && pos == entity.Position)
                return;

            //tracked while loading: static entities are only sent on map entry
            entity.Position = pos;

            if (mapSprite == null)
                return;

            if (entity.Icon == null)
                CreateIcon(entityId, entity);
            else
                entity.Icon.localPosition = MapPosition(pos);
        }

        //relative to the map image's top-left pivot
        private Vector3 MapPosition(Vector2Int pos)
        {
            var p = pos.ToMapPosition();
            return new Vector3(p.x, p.y - mapPixelHeight, 0f);
        }

        private void CreateIcon(int entityId, MinimapEntityData entity)
        {
            DestroyIcon(entity);

            if (entity.Type == CharacterDisplayType.Player && PlayerState.Instance.TryGetPartyMember(entityId, out var member))
            {
                var head = Instantiate(PlayerIconPrefab, EntityIconContainer, false);
                ShowHead(head, member.HeadId, member.HairColor, member.IsMale);
                entity.Icon = (RectTransform)head.transform;
                entity.Label = string.IsNullOrEmpty(member.PlayerName) ? "Party Member" : member.PlayerName;
            }
            else
            {
                (Image Prefab, string Label) kind = entity.Type switch
                {
                    CharacterDisplayType.Player => (PlayerPrefab, "Player"),
                    CharacterDisplayType.Monster => (MonsterPrefab, "Monster"),
                    CharacterDisplayType.Boss => (BossPrefab, "Boss"),
                    CharacterDisplayType.Mvp => (MvpPrefab, "Mvp"),
                    CharacterDisplayType.WarpNpc => (WarpNpcPrefab, "Warp"),
                    CharacterDisplayType.Kafra => (KafraPrefab, "Kafra"),
                    CharacterDisplayType.TalkNpc => (TalkNpcPrefab, "NPC"),

                    //a null label makes the icon unhoverable
                    CharacterDisplayType.Portal => (PortalPrefab, null),
                    _ => (PlayerPrefab, null)
                };

                entity.Icon = Instantiate(kind.Prefab, EntityIconContainer, false).rectTransform;
                entity.Label = kind.Label;
            }

            entity.Icon.localPosition = MapPosition(entity.Position);
            ApplyIconScale(entity.Icon);
        }

        public void SetPlayerPosition(Vector2Int pos)
        {
            if (!gameObject.activeInHierarchy || mapSprite == null)
                return;

            if (playerIcon == null)
            {
                playerIcon = Instantiate(PlayerIconPrefab, IconLayer.transform, false);
                ShowPlayerHead();
            }

            if (pos == lastIconPosition)
                return;

            lastIconPosition = pos;
            UpdatePlayerMarker();
        }

        private void ShowPlayerHead()
        {
            var state = PlayerState.Instance;
            ShowHead(playerIcon, state.HairStyleId, state.HairColorId, state.IsMale);
        }

        private static void ShowHead(UiPlayerSprite sprite, int headId, int hairColor, bool isMale) =>
            sprite.DisplaySprite(ClientDataLoader.Instance.GetPlayerHeadSpriteName(headId, hairColor, isMale));

        private void OnAppearanceChanged()
        {
            if (playerIcon != null)
                ShowPlayerHead();
        }

        private void UpdatePlayerMarker()
        {
            var mapPosition = MapPosition(lastIconPosition);

            playerIcon.transform.localPosition = mapPosition;
            ApplyIconScale(playerIcon);

            ScrollViewportTo(mapPosition);

            //the map moved under the pointer
            UpdateHover();
        }

        private void ScrollViewportTo(Vector2 mapPosition)
        {
            var target = mapPosition * zoom;
            var viewSize = Viewport.rect.size;

            var maxScrollX = Mathf.Max(mapPixelWidth * zoom - viewSize.x, 0f);
            var maxScrollY = Mathf.Max(mapPixelHeight * zoom - viewSize.y, 0f);

            var scrollX = Mathf.Clamp(viewSize.x / 2f - target.x, -maxScrollX, 0f);
            var scrollY = Mathf.Clamp(-target.y - viewSize.y / 2f, 0f, maxScrollY);

            ContentContainer.anchoredPosition = new Vector2(scrollX, scrollY);
        }

        public void LoadMinimap(string mapName, MapType type)
        {
            if(loadCoroutine != null)
                StopCoroutine(loadCoroutine);

            //a coroutine can't start on an inactive object
            gameObject.SetActive(true);

            ContentContainer.gameObject.SetActive(false);
            mapType = type;
            ResetView();

            loadCoroutine = StartCoroutine(LoadMinimapCoroutine(mapName));
        }

        public void HideMinimap()
        {
            UnloadMap();
            gameObject.SetActive(false);
        }

        private void SetZoom(float value)
        {
            zoom = Mathf.Clamp(value, minZoom, maxZoom);
            ContentContainer.localScale = new Vector3(zoom, zoom, zoom);

            foreach (var pair in entities)
                if (pair.Value.Icon != null)
                    ApplyIconScale(pair.Value.Icon);

            if (playerIcon != null)
                UpdatePlayerMarker();
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (mapSprite == null || Mathf.Approximately(eventData.scrollDelta.y, 0f))
                return;

            var zoomMultiplier = Mathf.Pow(1f + MouseWheelZoomStep, eventData.scrollDelta.y);
            SetZoom(zoom * zoomMultiplier);
        }

        private void UpdateMapMaterial()
        {
            MapImage.sprite = mapType == MapType.Dungeon ? walkSprite : mapSprite;

            if (mapType == MapType.Dungeon || mapType == MapType.Town)
            {
                MapImage.material = DungeonMaterial;
                return;
            }

            MapImage.material = OverworldMaterial;
            OverworldMaterial.SetTexture(SecondaryTex, walkSprite.texture);
        }

        private IEnumerator LoadMinimapCoroutine(string mapName)
        {
            yield return new WaitForEndOfFrame();

            var loadMap = Addressables.LoadAssetAsync<Sprite>($"Assets/Maps/minimap/{mapName}.png");
            var loadWalk = Addressables.LoadAssetAsync<Sprite>($"Assets/Maps/minimap/{mapName}_walkmask.png");

            yield return loadMap;
            yield return loadWalk;

            if (!loadWalk.IsDone || !loadWalk.IsValid() || !loadMap.IsDone || !loadMap.IsValid())
            {
                Debug.LogWarning("Could not load minimap.");
                yield break;
            }

            mapSprite = loadMap.Result;
            walkSprite = loadWalk.Result;

            var w = mapPixelWidth = mapSprite.texture.width;
            var h = mapPixelHeight = mapSprite.texture.height;

            UpdateMapMaterial();
            MapImage.rectTransform.sizeDelta = new Vector2(w, h);
            ContentContainer.sizeDelta = MapImage.rectTransform.sizeDelta;
            ContentContainer.localPosition = Vector3.zero;

            minZoom = ResizeToMap(w, h);
            maxZoom = Mathf.Max(minZoom, Mathf.Clamp(minZoom * 6f, 2f, 10f));

            SetZoom(minZoom);

            ContentContainer.gameObject.SetActive(true);

            ApplyVisibility();
        }

        void Awake()
        {
            instance = this;
            selfRect = (RectTransform)transform;
            maxSize = selfRect.rect.size;
            OverworldMaterial = new Material(OverworldMaterial);
            DungeonMaterial = new Material(DungeonMaterial);
            PlayerState.Instance.PartyMembershipChanged += RefreshPartyIcon;
            PlayerState.Instance.AppearanceChanged += OnAppearanceChanged;
            ApplyVisibility();
        }

        private void OnDestroy()
        {
            PlayerState.Instance.PartyMembershipChanged -= RefreshPartyIcon;
            PlayerState.Instance.AppearanceChanged -= OnAppearanceChanged;
        }

        private float ResizeToMap(int mapWidth, int mapHeight)
        {
            var fit = Mathf.Min(maxSize.x / mapWidth, maxSize.y / mapHeight);
            selfRect.sizeDelta = new Vector2(mapWidth * fit, mapHeight * fit);
            return fit;
        }

        public void ApplyVisibility()
        {
            GameConfig.InitializeIfNecessary();

            if (mapSprite == null)
                return;

            var config = GameConfig.Data;

            if (!config.EnableMinimap || (config.MinimapOpacity <= 0f && config.MinimapIconOpacity <= 0f))
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            foreach (var pair in entities)
                if (pair.Value.Icon == null)
                    CreateIcon(pair.Key, pair.Value);

            var mapColor = MapImage.color;
            mapColor.a = config.MinimapOpacity;
            MapImage.color = mapColor;

            IconLayer.alpha = config.MinimapIconOpacity;
        }
    }
}

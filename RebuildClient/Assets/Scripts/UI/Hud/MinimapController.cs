using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.PlayerControl;
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

        public Image PlayerIconPrefab;
        public Image PlayerPrefab;
        public Image PartyPlayerPrefab;
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

        private const float TileCenter = 0.5f;

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
        private bool mapTransformDirty;

        private readonly Dictionary<int, MinimapEntityData> entities = new();

        private Image playerIcon;
        private Vector2Int lastIconPosition = new(-1, -1);
        private float lastIconAngle = float.NaN;

        private bool pointerInside;
        private Vector2 pointerPosition;
        private Camera pointerCamera;
        private MinimapEntityData hoveredEntity;

        private class MinimapEntityData
        {
            public Image Icon;
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
            mapSprite = null;
            walkSprite = null;
            ClearIconViews();
            entities.Clear();
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
            lastIconAngle = float.NaN;
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

        //call after PlayerState reflects the party change
        public void RefreshPartyIcon(int entityId)
        {
            if (entities.TryGetValue(entityId, out var entity)
                && entity.Icon != null
                && entity.Type == CharacterDisplayType.Player)
                CreateIcon(entityId, entity);
        }

        //counter-scales icons against zoom so they keep their on-screen size
        private float IconScale => IconSizeMultiplier / zoom;

        private void ApplyIconScale(MinimapEntityData entity) =>
            entity.Icon.rectTransform.localScale = Vector3.one * IconScale;

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

            SetHovered(EntityAt(pointerPosition, pointerCamera));
        }

        private MinimapEntityData EntityAt(Vector2 screenPoint, Camera cam)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(MapImage.rectTransform, screenPoint, cam, out var local))
                return null;

            MinimapEntityData nearest = null;
            var nearestDistance = float.MaxValue;

            var iconScale = IconScale;

            foreach (var pair in entities)
            {
                var entity = pair.Value;

                if (entity.Label == null || entity.Icon == null)
                    continue;

                var half = entity.Icon.rectTransform.rect.size * (0.5f * iconScale);
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

            //tracked while hidden: static entities are only sent on map entry
            entity.Position = pos;

            if (mapSprite == null || !gameObject.activeInHierarchy)
                return;

            if (entity.Icon == null)
                CreateIcon(entityId, entity);
            else
                entity.Icon.rectTransform.localPosition = MapPosition(pos);
        }

        //relative to the map image's top-left pivot
        private Vector3 MapPosition(Vector2Int pos) =>
            new(pos.x + TileCenter, pos.y - mapPixelHeight + TileCenter, 0f);

        private void CreateIcon(int entityId, MinimapEntityData entity)
        {
            DestroyIcon(entity);

            var state = PlayerState.Instance;
            var type = entity.Type;
            var isParty = type == CharacterDisplayType.Player && state.IsInMyParty(entityId);

            (Image Prefab, string Label) kind = type switch
            {
                CharacterDisplayType.Player => isParty ? (PartyPlayerPrefab, "Party Member") : (PlayerPrefab, "Player"),
                CharacterDisplayType.Monster => (MonsterPrefab, "Monster"),
                CharacterDisplayType.Boss => (BossPrefab, "Boss"),
                CharacterDisplayType.Mvp => (MvpPrefab, "MVP"),
                CharacterDisplayType.WarpNpc => (WarpNpcPrefab, "Warp"),
                CharacterDisplayType.Kafra => (KafraPrefab, "Kafra"),
                CharacterDisplayType.TalkNpc => (TalkNpcPrefab, "NPC"),

                //a null label makes the icon unhoverable
                CharacterDisplayType.Portal => (PortalPrefab, null),
                _ => (PlayerPrefab, null)
            };

            entity.Icon = Instantiate(kind.Prefab, EntityIconContainer, false);
            entity.Icon.rectTransform.localPosition = MapPosition(entity.Position);
            ApplyIconScale(entity);

            entity.Label = isParty
                           && state.PartyMemberIdLookup.TryGetValue(entityId, out var memberId)
                           && state.PartyMembers.TryGetValue(memberId, out var member)
                           && !string.IsNullOrEmpty(member.PlayerName)
                ? member.PlayerName
                : kind.Label;
        }

        public void SetPlayerPosition(Vector2Int pos, float angle)
        {
            if (!gameObject.activeInHierarchy || mapSprite == null)
                return;

            if (playerIcon == null)
                playerIcon = Instantiate(PlayerIconPrefab, IconLayer.transform, false);

            if (pos == lastIconPosition && Mathf.Approximately(angle, lastIconAngle) && !mapTransformDirty)
                return;

            lastIconPosition = pos;
            lastIconAngle = angle;

            var r = playerIcon.rectTransform;
            var mapPosition = MapPosition(pos);

            r.localPosition = mapPosition;
            r.localRotation = Quaternion.Euler(0f, 0f, -angle);
            r.localScale = Vector3.one * IconScale;

            ScrollViewportTo(mapPosition);

            //the map moved under the pointer
            if (pointerInside)
                UpdateHover();

            mapTransformDirty = false;
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
            mapSprite = null;
            walkSprite = null;
            mapType = type;

            ClearIconViews();

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

            if (mapSprite == null)
                return;

            ContentContainer.localScale = new Vector3(zoom, zoom, zoom);

            foreach (var pair in entities)
                if (pair.Value.Icon != null)
                    ApplyIconScale(pair.Value);

            mapTransformDirty = true;
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
            MapImage.transform.localPosition = Vector3.zero;
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
            ApplyVisibility();
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
                //icons aren't moved while hidden, so they're rebuilt when shown
                ClearIconViews();
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

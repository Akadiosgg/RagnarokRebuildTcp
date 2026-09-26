using System;
using System.Collections.Generic;
using Assets.Scripts.PlayerControl;
using Assets.Scripts.Sprites;
using Assets.Scripts.UI.Hud;
using RebuildSharedData.Enum.EntityStats;
using UnityEngine;
using UnityEngine.Audio;

namespace Assets.Scripts.UI.ConfigWindow
{
    public enum BoolOption
    {
        ShowAllSkillsInSkillWindow, AutoLockSkillWindow,
        ShowExpGainOnKill, ScalePlayerDisplayWithZoom, ShowMonsterHpBars, ShowLevelsInOverlay, AutoHideFullHPBars, AdjustOverlayWhenSitting,
        EnableSpriteFiltering, UseSpriteBasedDamageNumbers, AllowTabToShowWalkTable, HideShoutChat, EnableXRay, EnableWASDControls,
        ScaleUiWithResolution, KeepWindowsOnScreen, EnableMinimap,
        VSync, LimitForegroundFps, LimitBackgroundFps,
        ShowBaseExpValue, ShowBaseExpPercent, ShowJobExpValue, ShowJobExpPercent, ShowExpGainInChat,
        EnableMaster, EnableBgm, EnableEffects, EnableEnvironment
    }

    public enum RangeOption
    {
        DamageNumberSize, DamageSpacingSize, MasterUIScale, MinimapOpacity, MinimapIconOpacity,
        ForegroundFpsLimit, BackgroundFpsLimit,
        VolumeMaster, VolumeBgm, VolumeEffects, VolumeEnvironment
    }

    // A get/set pair over a GameConfigData field. GameConfig.Set applies the change after writing.
    public readonly struct ConfigBinding<T>
    {
        public readonly Func<T> Get;
        public readonly Action<T> Set;
        public ConfigBinding(Func<T> get, Action<T> set) { Get = get; Set = set; }
    }

    // A RangeOption's span, in the units its binding exposes.
    public readonly struct RangeBounds
    {
        public readonly float Min, Max;
        public RangeBounds(float min, float max) { Min = min; Max = max; }
    }

    public static partial class GameConfig
    {
        private static GameConfigData D => Data;

        public static AudioMixer AudioMixer;

        private const string NoBinding = "has no binding (no same-named field on GameConfigData and no explicit override) — it will not sync.";
        private const string NoBounds = "has no bounds — a slider bound to it would throw instead of taking a range.";

        // Reflected by name (ReflectMap), except the UI-scale conversion and array-backed audio channels (bound by hand).
        private static readonly Dictionary<BoolOption, ConfigBinding<bool>> Bools = BuildBools();
        private static readonly Dictionary<RangeOption, ConfigBinding<float>> Ranges = BuildRanges();
        private static readonly Dictionary<RangeOption, RangeBounds> BoundsMap = BuildBounds();

        public static RangeBounds BoundsFor(RangeOption o) => BoundsMap[o];

        private static Dictionary<RangeOption, RangeBounds> BuildBounds()
        {
            var map = new Dictionary<RangeOption, RangeBounds>
            {
                [RangeOption.DamageNumberSize]  = new(0.2f, 2f),
                [RangeOption.DamageSpacingSize] = new(0.4f, 1f),
                [RangeOption.MasterUIScale]     = new(5f, 11f),  // tenths: 0.5 - 1.1
                [RangeOption.MinimapOpacity]    = new(0f, 20f),  // 5% steps
                [RangeOption.MinimapIconOpacity] = new(0f, 20f), // 5% steps; both at zero hides the minimap
                [RangeOption.ForegroundFpsLimit] = new(60f, 1000f),
                [RangeOption.BackgroundFpsLimit] = new(10f, 1000f),
                [RangeOption.VolumeMaster]      = new(0f, 100f),
                [RangeOption.VolumeBgm]         = new(0f, 100f),
                [RangeOption.VolumeEffects]     = new(0f, 100f),
                [RangeOption.VolumeEnvironment] = new(0f, 100f),
            };
            Validate(map, NoBounds);
            return map;
        }

        private static Dictionary<BoolOption, ConfigBinding<bool>> BuildBools()
        {
            var map = ReflectMap<BoolOption, bool>();
            map[BoolOption.EnableMaster]      = EnableBinding(ConfigAudioChannel.Master);
            map[BoolOption.EnableBgm]         = EnableBinding(ConfigAudioChannel.Music);
            map[BoolOption.EnableEffects]     = EnableBinding(ConfigAudioChannel.Effects);
            map[BoolOption.EnableEnvironment] = EnableBinding(ConfigAudioChannel.Environment);
            Validate(map, NoBinding);
            return map;
        }

        private static Dictionary<RangeOption, ConfigBinding<float>> BuildRanges()
        {
            var map = ReflectMap<RangeOption, float>();
            // Slider works in tenths; config stores the scale factor (slider / 10).
            map[RangeOption.MasterUIScale] = new(() => D.MasterUIScale * 10f, v => D.MasterUIScale = v / 10f);
            // Slider works in 5% steps; config stores the alpha.
            map[RangeOption.MinimapOpacity] = new(() => D.MinimapOpacity * 20f, v => D.MinimapOpacity = v / 20f);
            map[RangeOption.MinimapIconOpacity] = new(() => D.MinimapIconOpacity * 20f, v => D.MinimapIconOpacity = v / 20f);
            map[RangeOption.VolumeMaster]      = VolumeBinding(ConfigAudioChannel.Master);
            map[RangeOption.VolumeBgm]         = VolumeBinding(ConfigAudioChannel.Music);
            map[RangeOption.VolumeEffects]     = VolumeBinding(ConfigAudioChannel.Effects);
            map[RangeOption.VolumeEnvironment] = VolumeBinding(ConfigAudioChannel.Environment);
            Validate(map, NoBinding);
            return map;
        }

        // Bind each enum member to the same-named, same-typed GameConfigData field; skip members with no match.
        private static Dictionary<TEnum, ConfigBinding<TValue>> ReflectMap<TEnum, TValue>() where TEnum : Enum
        {
            var map = new Dictionary<TEnum, ConfigBinding<TValue>>();
            foreach (TEnum o in Enum.GetValues(typeof(TEnum)))
            {
                var f = typeof(GameConfigData).GetField(o.ToString());
                if (f != null && f.FieldType == typeof(TValue))
                    map[o] = new(() => (TValue)f.GetValue(D), v => f.SetValue(D, v));
            }
            return map;
        }

        // Logs any enum member missing from a table.
        private static void Validate<TEnum, TValue>(Dictionary<TEnum, TValue> map, string problem) where TEnum : Enum
        {
            foreach (TEnum o in Enum.GetValues(typeof(TEnum)))
                if (!map.ContainsKey(o))
                    Debug.LogError($"GameConfig: {typeof(TEnum).Name}.{o} {problem}");
        }

        public static bool Get(BoolOption o) { InitializeIfNecessary(); return Bools[o].Get(); }
        public static void Set(BoolOption o, bool v) { Bools[o].Set(v); ApplyBool(o); }

        public static float Get(RangeOption o) { InitializeIfNecessary(); return Ranges[o].Get(); }
        public static void Set(RangeOption o, float v, bool apply = true) { Ranges[o].Set(v); if (apply) ApplyRange(o); }
        public static void Apply(RangeOption o) => ApplyRange(o);

        public static void ApplyAll()
        {
            foreach (BoolOption o in Enum.GetValues(typeof(BoolOption))) ApplyBool(o);
            foreach (RangeOption o in Enum.GetValues(typeof(RangeOption))) ApplyRange(o);
        }

        // Resets only option-backing fields, leaving other state (window positions, hotbars, saved login) intact.
        public static void ResetToDefaults()
        {
            InitializeIfNecessary();

            var defaults = GameConfigData.CreateDefault();

            foreach (BoolOption o in Enum.GetValues(typeof(BoolOption))) ResetOptionField(o.ToString(), defaults);
            foreach (RangeOption o in Enum.GetValues(typeof(RangeOption))) ResetOptionField(o.ToString(), defaults);

            // Array-backed audio options have no same-named field, so reset their arrays explicitly.
            Data.AudioVolumeLevels = defaults.AudioVolumeLevels;
            Data.AudioMuteValues = defaults.AudioMuteValues;

            ApplyAll();
        }

        private static void ResetOptionField(string name, GameConfigData defaults)
        {
            var f = typeof(GameConfigData).GetField(name);
            if (f != null) f.SetValue(Data, f.GetValue(defaults));
        }

        public static void ApplyAudio()
        {
            foreach (var ch in MixerParam.Keys)
                ApplyAudioChannel(ch);
        }

        // Only options that feed cached/derived runtime state need a case; read-on-use options fall through.
        private static void ApplyBool(BoolOption o)
        {
            switch (o)
            {
                case BoolOption.EnableMaster:      ApplyAudioChannel(ConfigAudioChannel.Master); break;
                case BoolOption.EnableBgm:         ApplyAudioChannel(ConfigAudioChannel.Music); break;
                case BoolOption.EnableEffects:     ApplyAudioChannel(ConfigAudioChannel.Effects); break;
                case BoolOption.EnableEnvironment: ApplyAudioChannel(ConfigAudioChannel.Environment); break;

                case BoolOption.EnableSpriteFiltering:
                    CameraFollower.Instance.SetSmoothPixel(!D.EnableSpriteFiltering);
                    break;
                case BoolOption.UseSpriteBasedDamageNumbers:
                    CameraFollower.Instance.UseTTFDamage = !D.UseSpriteBasedDamageNumbers;
                    break;

                case BoolOption.ScaleUiWithResolution:
                    CameraFollower.Instance.UpdateCameraSize(); //also fits the floating windows
                    break;

                case BoolOption.EnableMinimap:
                    MinimapController.Instance.ApplyVisibility();
                    break;

                case BoolOption.VSync:
                case BoolOption.LimitForegroundFps:
                case BoolOption.LimitBackgroundFps:
                    ApplyFrameRate();
                    break;

                case BoolOption.KeepWindowsOnScreen:
                    if (D.KeepWindowsOnScreen)
                        UiManager.Instance.FitFloatingWindowsIntoPlayArea();
                    break;

                case BoolOption.ShowBaseExpValue:
                case BoolOption.ShowBaseExpPercent:
                case BoolOption.ShowJobExpValue:
                case BoolOption.ShowJobExpPercent:
                case BoolOption.ShowExpGainInChat:
                    RefreshExpDisplay();
                    break;
            }
        }

        private static void ApplyRange(RangeOption o)
        {
            switch (o)
            {
                case RangeOption.VolumeMaster:      ApplyAudioChannel(ConfigAudioChannel.Master); break;
                case RangeOption.VolumeBgm:         ApplyAudioChannel(ConfigAudioChannel.Music); break;
                case RangeOption.VolumeEffects:     ApplyAudioChannel(ConfigAudioChannel.Effects); break;
                case RangeOption.VolumeEnvironment: ApplyAudioChannel(ConfigAudioChannel.Environment); break;

                case RangeOption.MasterUIScale:
                    CameraFollower.Instance.UpdateCameraSize(); //also fits the floating windows
                    break;

                case RangeOption.MinimapOpacity:
                case RangeOption.MinimapIconOpacity:
                    MinimapController.Instance.ApplyVisibility();
                    break;

                case RangeOption.ForegroundFpsLimit:
                case RangeOption.BackgroundFpsLimit:
                    ApplyFrameRate();
                    break;
            }
        }

        private static bool followingFocus;

        private static void ApplyFrameRate()
        {
            if (!followingFocus)
            {
                followingFocus = true;
                Application.focusChanged += _ => ApplyFrameRate();
            }

            var (capped, fps) = Application.isFocused
                ? (D.LimitForegroundFps, D.ForegroundFpsLimit)
                : (D.LimitBackgroundFps, D.BackgroundFpsLimit);

            //vsync overrides the cap, so it's off when the cap is below the vsync rate
            var capBelowVSync = capped && fps < Screen.currentResolution.refreshRateRatio.value;

            QualitySettings.vSyncCount = D.VSync && !capBelowVSync ? 1 : 0;
            Application.targetFrameRate = capped ? Mathf.RoundToInt(fps) : -1;
        }

        private static void RefreshExpDisplay()
        {
            var state = PlayerState.Instance;
            if (state == null)
                return;

            var cam = CameraFollower.Instance;
            cam.UpdatePlayerExp(state.Exp, cam.ExpForLevel(state.Level));
            cam.UpdatePlayerJobExp(state.GetData(PlayerStat.JobExp),
                ClientDataLoader.Instance.GetJobExpRequired(state.JobId, state.GetData(PlayerStat.JobLevel)));
        }

        private static ConfigBinding<bool> EnableBinding(ConfigAudioChannel ch)
            => new(() => !D.AudioMuteValues[(int)ch], v => D.AudioMuteValues[(int)ch] = !v);

        private static ConfigBinding<float> VolumeBinding(ConfigAudioChannel ch)
            => new(() => D.AudioVolumeLevels[(int)ch], v => D.AudioVolumeLevels[(int)ch] = Mathf.RoundToInt(v));

        private static readonly Dictionary<ConfigAudioChannel, string> MixerParam = new()
        {
            [ConfigAudioChannel.Master] = "Master",
            [ConfigAudioChannel.Music] = "Music",
            [ConfigAudioChannel.Environment] = "Environment",
            [ConfigAudioChannel.Effects] = "Sounds",
        };

        private static void ApplyAudioChannel(ConfigAudioChannel ch)
        {
            if (AudioMixer == null)
                return;
            var linear = D.AudioMuteValues[(int)ch] ? 0f : D.AudioVolumeLevels[(int)ch] / 100f;
            AudioMixer.SetFloat(MixerParam[ch], LinearToDecibel(linear));
        }

        private static float LinearToDecibel(float linear) => linear > 0f ? 20f * Mathf.Log10(linear) : -144f;
    }
}

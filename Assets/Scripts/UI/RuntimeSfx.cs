using System.Collections.Generic;
using UnityEngine;

public enum RuntimeSfxType
{
    Click,
    Draw,
    Play,
    Error,
    Special,
    Turn,
    Win,
    Uno,
    Pass,
    Bomb,
    Lose,
    RoundComplete
}

public static class RuntimeSfx
{
    private const string EnabledPrefKey = "RuntimeSfx.Enabled";
    private const string VolumePrefKey = "RuntimeSfx.Volume";
    private const float DefaultVolume = 0.82f;

    private static readonly Dictionary<RuntimeSfxType, AudioClip> Clips = new Dictionary<RuntimeSfxType, AudioClip>();
    private static AudioSource source;
    private static bool preferencesLoaded;
    private static bool enabled = true;
    private static float volume = DefaultVolume;

    public static bool IsEnabled
    {
        get
        {
            LoadPreferences();
            return enabled;
        }
    }

    public static float Volume
    {
        get
        {
            LoadPreferences();
            return volume;
        }
    }

    public static void Play(RuntimeSfxType type, float volume = 1f)
    {
        LoadPreferences();
        if (!enabled || RuntimeSfx.volume <= 0.001f)
        {
            return;
        }

        EnsureSource();

        if (source == null)
        {
            return;
        }

        AudioClip clip = GetClip(type);
        if (clip != null)
        {
            source.PlayOneShot(clip, Mathf.Clamp01(volume));
        }
    }

    public static void SetEnabled(bool value)
    {
        LoadPreferences();
        enabled = value;
        PlayerPrefs.SetInt(EnabledPrefKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void SetVolume(float value)
    {
        LoadPreferences();
        volume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(VolumePrefKey, volume);
        PlayerPrefs.Save();

        if (source != null)
        {
            source.volume = volume;
        }
    }

    private static void LoadPreferences()
    {
        if (preferencesLoaded)
        {
            return;
        }

        enabled = PlayerPrefs.GetInt(EnabledPrefKey, 1) == 1;
        volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumePrefKey, DefaultVolume));
        preferencesLoaded = true;
    }

    private static void EnsureSource()
    {
        if (source != null)
        {
            return;
        }

        GameObject audioObject = new GameObject("Runtime_UIAudio");
        Object.DontDestroyOnLoad(audioObject);

        source = audioObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.ignoreListenerPause = true;
        source.volume = volume;
    }

    private static AudioClip GetClip(RuntimeSfxType type)
    {
        if (Clips.TryGetValue(type, out AudioClip clip))
        {
            return clip;
        }

        switch (type)
        {
            case RuntimeSfxType.Click:
                clip = LoadClip("sfx-card-select", "sfx-card-pick");
                break;
            case RuntimeSfxType.Draw:
                clip = LoadClip("sfx-card-draw-comm-2-new", "uno-sfx-card-deal-comm");
                break;
            case RuntimeSfxType.Play:
                clip = LoadClip("sfx-card-pick", "sfx-card-opendeck");
                break;
            case RuntimeSfxType.Error:
                clip = LoadClip("sfx-gamestart-end", "sfx-card-select");
                break;
            case RuntimeSfxType.Special:
                clip = LoadClip("uno-sfx-card-effect-uturn", "uno-sfx-arrowswitch");
                break;
            case RuntimeSfxType.Turn:
                clip = LoadClip("uno-sfx-arrowswitch", "sfx-card-select");
                break;
            case RuntimeSfxType.Win:
                clip = LoadClip("sfx-ui-victory-token", "sfx-gamestart-end");
                break;
            case RuntimeSfxType.Uno:
                clip = LoadClip("uno", "uno-sfx-gamestart");
                break;
            case RuntimeSfxType.Pass:
                clip = LoadClip("uno-sfx-arrowswitch", "sfx-card-select");
                break;
            case RuntimeSfxType.Bomb:
                clip = LoadClip("uno-sfx-card-effect-uturn", "sfx-gamestart-end");
                break;
            case RuntimeSfxType.Lose:
                clip = LoadClip("sfx-gamestart-end", "sfx-card-pick");
                break;
            case RuntimeSfxType.RoundComplete:
                clip = LoadClip("sfx-ui-victory-token", "sfx-gamestart");
                break;
        }

        Clips[type] = clip;
        return clip;
    }

    private static AudioClip LoadClip(params string[] clipNames)
    {
        foreach (string clipName in clipNames)
        {
            AudioClip clip = Resources.Load<AudioClip>("Audio/SFX/" + clipName);
            if (clip != null)
            {
                return clip;
            }
        }

        Debug.LogWarning("Missing SFX clip: " + string.Join(", ", clipNames));
        return null;
    }
}

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
    Uno
}

public static class RuntimeSfx
{
    private static readonly Dictionary<RuntimeSfxType, AudioClip> Clips = new Dictionary<RuntimeSfxType, AudioClip>();
    private static AudioSource source;

    public static void Play(RuntimeSfxType type, float volume = 1f)
    {
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
        source.volume = 0.82f;
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

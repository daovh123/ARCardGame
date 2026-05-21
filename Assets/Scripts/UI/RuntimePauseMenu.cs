using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RuntimePauseMenu
{
    private RectTransform overlay;
    private RectTransform panel;
    private Button sfxButton;
    private RectTransform volumeTrack;
    private RectTransform volumeFill;
    private Image volumeFillImage;
    private RectTransform volumeHandle;
    private TMP_Text volumeValueText;
    private bool isVisible;
    private const float VolumeHandlePadding = 22f;
    private const float VolumeFillPadding = 8f;

    public bool IsVisible
    {
        get { return isVisible; }
    }

    public void Build(Transform parent, string prefix, Action onResume, Action onRestart, Action onMenu)
    {
        BuildInternal(parent, prefix, "PAUSED", "Audio & Match Controls", "Resume", true, onResume, onRestart, onMenu);
    }

    public void BuildSettings(Transform parent, string prefix, Action onClose)
    {
        BuildInternal(parent, prefix, "SETTINGS", "Audio Settings", "Close", false, onClose, null, null);
    }

    private void BuildInternal(
        Transform parent,
        string prefix,
        string titleText,
        string subtitleText,
        string closeLabel,
        bool showMatchControls,
        Action onClose,
        Action onRestart,
        Action onMenu)
    {
        if (overlay != null || parent == null)
        {
            return;
        }

        GameObject overlayObject = new GameObject(prefix + "_PauseOverlay", typeof(RectTransform), typeof(Image));
        overlayObject.transform.SetParent(parent, false);
        overlay = overlayObject.GetComponent<RectTransform>();
        RuntimeUITheme.SetRect(overlay, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        Image overlayImage = overlayObject.GetComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.62f);
        overlayImage.raycastTarget = true;

        panel = RuntimeUITheme.CreatePanel(
            overlay,
            prefix + "_PausePanel",
            new Color(0.01f, 0.035f, 0.045f, 0.98f),
            new Color(1f, 0.76f, 0.24f, 0.86f),
            30,
            5);
        RuntimeUITheme.SetRect(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, showMatchControls ? new Vector2(640f, 620f) : new Vector2(640f, 430f));
        RuntimeUITheme.AddShadow(panel.gameObject, new Color(0f, 0f, 0f, 0.68f), new Vector2(0f, -14f));

        TMP_Text title = RuntimeUITheme.CreateLabel(panel, "Title", titleText, 42, RuntimeUITheme.Gold);
        RuntimeUITheme.SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, showMatchControls ? 250f : 154f), new Vector2(500f, 56f));

        TMP_Text subtitle = RuntimeUITheme.CreateLabel(panel, "Subtitle", subtitleText, 22, Color.white);
        RuntimeUITheme.SetRect(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, showMatchControls ? 204f : 112f), new Vector2(500f, 32f));

        Button closeButton = CreateButton(panel, prefix + "_CloseButton", closeLabel, RuntimeUITheme.Gold, RuntimeUITheme.Ink);
        RuntimeUITheme.SetRect(closeButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, showMatchControls ? 148f : -162f), new Vector2(420f, 60f));
        closeButton.onClick.AddListener(() =>
        {
            RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
            onClose?.Invoke();
        });

        RectTransform audioPanel = RuntimeUITheme.CreatePanel(
            panel,
            prefix + "_AudioPanel",
            new Color(0.01f, 0.04f, 0.05f, 0.66f),
            new Color(0.18f, 0.95f, 0.86f, 0.24f),
            20,
            2);
        RuntimeUITheme.SetRect(audioPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, showMatchControls ? 0f : -22f), new Vector2(500f, 190f));

        TMP_Text audioTitle = RuntimeUITheme.CreateLabel(audioPanel, "AudioTitle", "SOUND", 24, RuntimeUITheme.Cyan);
        RuntimeUITheme.SetRect(audioTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 64f), new Vector2(420f, 30f));

        sfxButton = CreateButton(audioPanel, prefix + "_SfxToggle", "SFX: ON", new Color(0.04f, 0.56f, 0.34f, 1f), Color.white);
        RuntimeUITheme.SetRect(sfxButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 22f), new Vector2(360f, 50f));
        sfxButton.onClick.AddListener(ToggleSfx);

        TMP_Text volumeLabel = RuntimeUITheme.CreateLabel(audioPanel, "VolumeLabel", "Volume", 20, Color.white);
        RuntimeUITheme.SetRect(volumeLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-180f, -54f), new Vector2(104f, 30f));
        volumeLabel.alignment = TextAlignmentOptions.Left;

        CreateVolumeBar(audioPanel, prefix);

        volumeValueText = RuntimeUITheme.CreateLabel(audioPanel, "VolumeValue", "82%", 19, RuntimeUITheme.Gold);
        RuntimeUITheme.SetRect(volumeValueText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(210f, -54f), new Vector2(72f, 30f));
        volumeValueText.alignment = TextAlignmentOptions.Right;

        if (showMatchControls)
        {
            Button restartButton = CreateButton(panel, prefix + "_RestartButton", "Restart", RuntimeUITheme.Blue, Color.white);
            RuntimeUITheme.SetRect(restartButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-138f, -248f), new Vector2(250f, 60f));
            restartButton.onClick.AddListener(() =>
            {
                onRestart?.Invoke();
            });

            Button menuButton = CreateButton(panel, prefix + "_MenuButton", "Menu", new Color(0.08f, 0.16f, 0.18f, 1f), Color.white);
            RuntimeUITheme.SetRect(menuButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(138f, -248f), new Vector2(230f, 60f));
            menuButton.onClick.AddListener(() =>
            {
                onMenu?.Invoke();
            });
        }

        overlay.gameObject.SetActive(false);
        UpdateAudioControls();
    }

    public void Show()
    {
        if (overlay == null)
        {
            return;
        }

        isVisible = true;
        overlay.gameObject.SetActive(true);
        overlay.SetAsLastSibling();
        panel.localScale = Vector3.one;
        UpdateAudioControls();
    }

    public void Hide()
    {
        if (overlay == null)
        {
            return;
        }

        isVisible = false;
        overlay.gameObject.SetActive(false);
    }

    private Button CreateButton(Transform parent, string name, string label, Color fill, Color textColor)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        GameObject labelObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);
        RuntimeUITheme.SetRect(labelObject.transform as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        Button button = buttonObject.GetComponent<Button>();
        RuntimeUITheme.StyleButton(button, fill, textColor, label);
        return button;
    }

    private void CreateVolumeBar(Transform parent, string prefix)
    {
        GameObject trackObject = new GameObject(prefix + "_VolumeBar", typeof(RectTransform), typeof(Image), typeof(EventTrigger));
        trackObject.transform.SetParent(parent, false);
        volumeTrack = trackObject.GetComponent<RectTransform>();
        RuntimeUITheme.SetRect(volumeTrack, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(24f, -54f), new Vector2(284f, 30f));

        Image background = trackObject.GetComponent<Image>();
        background.sprite = RuntimeUITheme.GetRoundedRectSprite(prefix + "_volume_track", 320, 42, 14, new Color(0.04f, 0.08f, 0.10f, 0.96f), new Color(1f, 0.94f, 0.68f, 0.34f), 2);
        background.type = Image.Type.Sliced;
        background.color = Color.white;

        Image fill = RuntimeUITheme.CreateImage(volumeTrack, "Fill", RuntimeUITheme.GetRoundedRectSprite(prefix + "_volume_fill", 300, 30, 12, RuntimeUITheme.Gold, RuntimeUITheme.Gold, 0));
        fill.raycastTarget = false;
        fill.preserveAspect = false;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.fillAmount = RuntimeSfx.Volume;
        volumeFillImage = fill;
        volumeFill = fill.rectTransform;
        volumeFill.anchorMin = Vector2.zero;
        volumeFill.anchorMax = Vector2.one;
        volumeFill.pivot = new Vector2(0.5f, 0.5f);
        volumeFill.offsetMin = new Vector2(VolumeFillPadding, 6f);
        volumeFill.offsetMax = new Vector2(-VolumeHandlePadding, -6f);

        Image handle = RuntimeUITheme.CreateImage(volumeTrack, "Handle", RuntimeUITheme.GetCircleSprite(prefix + "_volume_handle", 44, Color.white, RuntimeUITheme.Gold, 4));
        handle.raycastTarget = false;
        volumeHandle = handle.rectTransform;
        RuntimeUITheme.SetRect(volumeHandle, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(32f, 32f));

        EventTrigger trigger = trackObject.GetComponent<EventTrigger>();
        AddVolumeEvent(trigger, EventTriggerType.PointerDown);
        AddVolumeEvent(trigger, EventTriggerType.Drag);
    }

    private void AddVolumeEvent(EventTrigger trigger, EventTriggerType eventType)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener(SetVolumeFromPointer);
        trigger.triggers.Add(entry);
    }

    private void SetVolumeFromPointer(BaseEventData eventData)
    {
        if (volumeTrack == null || !(eventData is PointerEventData pointerEventData))
        {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            volumeTrack,
            pointerEventData.position,
            pointerEventData.pressEventCamera,
            out Vector2 localPoint);

        float width = volumeTrack.rect.width <= 0f ? 284f : volumeTrack.rect.width;
        float minX = -width * 0.5f + VolumeHandlePadding;
        float maxX = width * 0.5f - VolumeHandlePadding;
        float nextVolume = Mathf.InverseLerp(minX, maxX, localPoint.x);
        RuntimeSfx.SetVolume(nextVolume);
        UpdateAudioControls();
    }

    private void ToggleSfx()
    {
        bool nextValue = !RuntimeSfx.IsEnabled;
        RuntimeSfx.SetEnabled(nextValue);
        if (nextValue)
        {
            RuntimeSfx.Play(RuntimeSfxType.Click, 0.82f);
        }

        UpdateAudioControls();
    }

    private void UpdateAudioControls()
    {
        UpdateVolumeBar();

        if (volumeValueText != null)
        {
            volumeValueText.text = Mathf.RoundToInt(RuntimeSfx.Volume * 100f) + "%";
        }

        if (sfxButton != null)
        {
            bool sfxEnabled = RuntimeSfx.IsEnabled;
            RuntimeUITheme.StyleButton(
                sfxButton,
                sfxEnabled ? new Color(0.04f, 0.56f, 0.34f, 1f) : new Color(0.64f, 0.12f, 0.12f, 1f),
                Color.white,
                sfxEnabled ? "SFX: ON" : "SFX: OFF");
        }
    }

    private void UpdateVolumeBar()
    {
        if (volumeTrack == null || volumeHandle == null)
        {
            return;
        }

        float volume = Mathf.Clamp01(RuntimeSfx.Volume);
        float width = volumeTrack.rect.width <= 0f ? 284f : volumeTrack.rect.width;
        float minX = -width * 0.5f + VolumeHandlePadding;
        float maxX = width * 0.5f - VolumeHandlePadding;

        if (volumeFillImage != null)
        {
            volumeFillImage.fillAmount = volume;
        }

        volumeHandle.anchoredPosition = new Vector2(Mathf.Lerp(minX, maxX, volume), 0f);
    }
}

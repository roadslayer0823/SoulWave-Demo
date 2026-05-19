using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using TMPro;

public class SettingManager : MonoBehaviour
{
    [Header("Script Reference")]
    public RhythmVisualizatorPro visualizatorScript;

    [Header("Option Panel")]
    [SerializeField] private GameObject[] optionPanels;
    [SerializeField] private Button nextPanelButton;
    [SerializeField] private Button prevPanelButton;
    [SerializeField] private TMP_Text panelTitle;

    [Header("Setting Panel")]
    [SerializeField] TMP_Dropdown dropdownList;
    [SerializeField] private GameObject snowEffect;
    [SerializeField] private Image snowButtonImage;
    [SerializeField] private Sprite snowButtonOn;
    [SerializeField] private Sprite snowButtonOff;

    [Header("Profile Panel")]
    [SerializeField] private ToggleSpriteSet[] genderOptions;
    [SerializeField] private ToggleSpriteSet[] accentOptions;

    private bool isOn = true;
    private int currentPanelIndex = 0;
    private int selectedGenderIndex = -1;
    private int selectedAccentIndex = -1;
    private string currentUsername = "";
    private string[] genderLabels = { "Male", "Female" };
    private string[] accentLabels = { "Calm", "Energetic", "Narrator", "AI Assist" };
    private string[] optionPanelsTitle = { "SETTINGS", "PROFILE", "VOICE LIBRARY", "STATISTIC" };

    private void Start()
    {
        currentUsername = PlayerPrefs.GetString("CurrentUsername", "");
        if (string.IsNullOrEmpty(currentUsername))
        {
            Debug.LogWarning("No username found → using default preferences");
        }

        // Basic validation
        if (optionPanels == null || optionPanels.Length == 0 ||
            optionPanelsTitle == null || optionPanelsTitle.Length != optionPanels.Length)
        {
            Debug.LogError("Option panels / titles setup incomplete!");
            return;
        }

        // Setup gender button listeners
        for (int i = 0; i < genderOptions.Length; i++)
        {
            int index = i;
            if (genderOptions[i].button != null)
            {
                genderOptions[i].button.onClick.AddListener(() => SelectGender(index));
            }
        }

        // Setup accent button listeners
        for (int i = 0; i < accentOptions.Length; i++)
        {
            int index = i;
            if (accentOptions[i].button != null)
            {
                accentOptions[i].button.onClick.AddListener(() => SelectAccent(index));
            }
        }

        // Load saved values
        LoadPreferences();

        // Initialize dropdown
        InitializeDropdown();

        // Show initial panel (also handles initial gender/accent visual update if on Profile)
        ShowPanel(currentPanelIndex);
    }

    private void InitializeDropdown()
    {
        if (dropdownList == null)
        {
            Debug.LogWarning("DropdownList is not assigned!");
            return;
        }

        dropdownList.ClearOptions();
        dropdownList.options.Add(new TMP_Dropdown.OptionData("Expansible Circle"));
        dropdownList.options.Add(new TMP_Dropdown.OptionData("Circle"));
        dropdownList.options.Add(new TMP_Dropdown.OptionData("Sphere"));

        // Template height adjustment (your original code)
        RectTransform templateRect = dropdownList.template;
        if (templateRect != null)
        {
            templateRect.sizeDelta = new Vector2(templateRect.sizeDelta.x, 200f);

            Transform viewport = templateRect.Find("Viewport");
            if (viewport != null)
            {
                RectTransform viewportRect = viewport as RectTransform;
                viewportRect.anchorMin = new Vector2(0, 0);
                viewportRect.anchorMax = new Vector2(1, 1);
                viewportRect.sizeDelta = Vector2.zero;

                Transform content = viewport.Find("Content");
                if (content != null)
                {
                    RectTransform contentRect = content as RectTransform;
                    contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, 200f);
                    contentRect.anchorMin = new Vector2(0, 1);
                    contentRect.anchorMax = new Vector2(1, 1);
                    contentRect.pivot = new Vector2(0.5f, 1);

                    VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
                    if (vlg == null)
                    {
                        vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
                    }
                }
            }

            ScrollRect scrollRect = templateRect.GetComponentInChildren<ScrollRect>(true);
            if (scrollRect != null)
            {
                scrollRect.vertical = true;
                scrollRect.horizontal = false;
            }
        }

        dropdownList.value = 0;
        dropdownList.RefreshShownValue();
        dropdownList.captionText.text = "Expansible Circle";
        dropdownList.onValueChanged.AddListener(OnDropDownValueChanged);
        OnDropDownValueChanged(dropdownList.value);
    }

    private void ShowPanel(int index)
    {
        // Hide all panels
        foreach (var panel in optionPanels)
        {
            if (panel != null) panel.SetActive(false);
        }

        // Show selected panel
        if (index >= 0 && index < optionPanels.Length && optionPanels[index] != null)
        {
            optionPanels[index].SetActive(true);

            if (index == 1) // PROFILE
            {
                RefreshProfileVisuals();
            }
        }

        // Update title
        if (panelTitle != null)
        {
            panelTitle.text = (index >= 0 && index < optionPanelsTitle.Length) ? optionPanelsTitle[index] : "UNKNOWN PANEL";
        }

        UpdateNavigationButtons();
    }

    private void RefreshProfileVisuals()
    {
        // Reset all to deselected first (safe fallback)
        foreach (var opt in genderOptions)
        {
            if (opt.targetImage != null && opt.deselectedSprite != null)
                opt.targetImage.sprite = opt.deselectedSprite;
        }
        foreach (var opt in accentOptions)
        {
            if (opt.targetImage != null && opt.deselectedSprite != null)
                opt.targetImage.sprite = opt.deselectedSprite;
        }

        // Then apply current selection
        if (selectedGenderIndex >= 0 && selectedGenderIndex < genderOptions.Length)
        {
            UpdateGenderVisuals(selectedGenderIndex);
        }
        if (selectedAccentIndex >= 0 && selectedAccentIndex < accentOptions.Length)
        {
            UpdateAccentVisuals(selectedAccentIndex);
        }
    }

    private void UpdateNavigationButtons()
    {
        if (prevPanelButton != null)
            prevPanelButton.interactable = currentPanelIndex > 0;

        if (nextPanelButton != null)
            nextPanelButton.interactable = currentPanelIndex < optionPanels.Length - 1;
    }

    // Selection Logic
    private void SelectGender(int index)
    {
        if (index == selectedGenderIndex) return;

        // Deselect previous
        if (selectedGenderIndex >= 0 && selectedGenderIndex < genderOptions.Length)
        {
            var prev = genderOptions[selectedGenderIndex];
            if (prev.targetImage != null && prev.deselectedSprite != null)
                prev.targetImage.sprite = prev.deselectedSprite;
        }

        selectedGenderIndex = index;
        UpdateGenderVisuals(index);
        SavePreferences();

        Debug.Log($"Gender selected: {GetSelectedGenderLabel()}");
    }

    private void SelectAccent(int index)
    {
        if (index == selectedAccentIndex) return;

        // Deselect previous
        if (selectedAccentIndex >= 0 && selectedAccentIndex < accentOptions.Length)
        {
            var prev = accentOptions[selectedAccentIndex];
            if (prev.targetImage != null && prev.deselectedSprite != null)
                prev.targetImage.sprite = prev.deselectedSprite;
        }

        selectedAccentIndex = index;
        UpdateAccentVisuals(index);
        SavePreferences();

        Debug.Log($"Accent selected: {GetSelectedAccentLabel()}");
    }

    private void UpdateGenderVisuals(int index)
    {
        if (index < 0 || index >= genderOptions.Length) return;
        var current = genderOptions[index];
        if (current.targetImage != null && current.selectedSprite != null)
            current.targetImage.sprite = current.selectedSprite;
    }

    private void UpdateAccentVisuals(int index)
    {
        if (index < 0 || index >= accentOptions.Length) return;
        var current = accentOptions[index];
        if (current.targetImage != null && current.selectedSprite != null)
            current.targetImage.sprite = current.selectedSprite;
    }

    // Preferences
    private void LoadPreferences()
    {
        string prefix = "Prefs_" + currentUsername + "_";

        // Visualization
        if (dropdownList != null)
        {
            int savedViz = PlayerPrefs.GetInt(prefix + "Visualization", 0);
            dropdownList.value = savedViz;
            dropdownList.RefreshShownValue();
            OnDropDownValueChanged(savedViz);
        }

        // Snow effect
        isOn = PlayerPrefs.GetInt(prefix + "SnowEffect", 1) == 1;
        if (snowEffect != null) snowEffect.SetActive(isOn);
        if (snowButtonImage != null)
            snowButtonImage.sprite = isOn ? snowButtonOn : snowButtonOff;

        // Gender
        string savedGender = PlayerPrefs.GetString(prefix + "Gender", "Male");
        selectedGenderIndex = Array.IndexOf(genderLabels, savedGender);
        if (selectedGenderIndex == -1 && genderOptions.Length > 0)
            selectedGenderIndex = 0;

        // Accent
        string savedAccent = PlayerPrefs.GetString(prefix + "Accent", "Calm");
        selectedAccentIndex = Array.IndexOf(accentLabels, savedAccent);
        if (selectedAccentIndex == -1 && accentOptions.Length > 0)
            selectedAccentIndex = 0;
    }

    private void SavePreferences()
    {
        if (string.IsNullOrEmpty(currentUsername)) return;

        string prefix = "Prefs_" + currentUsername + "_";

        PlayerPrefs.SetInt(prefix + "Visualization", dropdownList?.value ?? 0);
        PlayerPrefs.SetInt(prefix + "SnowEffect", isOn ? 1 : 0);
        PlayerPrefs.SetString(prefix + "Gender", GetSelectedGenderLabel());
        PlayerPrefs.SetString(prefix + "Accent", GetSelectedAccentLabel());

        PlayerPrefs.Save();
        Debug.Log($"Preferences saved for user: {currentUsername}");
    }

    private void OnDropDownValueChanged(int index)
    {
        if (visualizatorScript == null) return;

        RhythmVisualizatorPro.Visualizations newViz = index switch
        {
            0 => RhythmVisualizatorPro.Visualizations.ExpansibleCircle,
            1 => RhythmVisualizatorPro.Visualizations.Circle,
            2 => RhythmVisualizatorPro.Visualizations.Sphere,
            _ => RhythmVisualizatorPro.Visualizations.ExpansibleCircle
        };

        visualizatorScript.visualization = newViz;
        visualizatorScript.UpdateVisualizations();
        SavePreferences();
    }

    public void SnowEffectController()
    {
        isOn = !isOn;
        if (snowButtonImage != null)
            snowButtonImage.sprite = isOn ? snowButtonOn : snowButtonOff;
        if (snowEffect != null)
            snowEffect.SetActive(isOn);
        SavePreferences();
    }

    public bool GetIsOn() => isOn;

    public string GetSelectedGenderLabel() =>
        (selectedGenderIndex >= 0 && selectedGenderIndex < genderLabels.Length)
            ? genderLabels[selectedGenderIndex]
            : "None";

    public string GetSelectedAccentLabel() =>
        (selectedAccentIndex >= 0 && selectedAccentIndex < accentLabels.Length)
            ? accentLabels[selectedAccentIndex]
            : "None";

    public void NextPanel()
    {
        if (currentPanelIndex >= optionPanels.Length - 1) return;
        currentPanelIndex++;
        ShowPanel(currentPanelIndex);
    }

    public void PreviousPanel()
    {
        if (currentPanelIndex <= 0) return;
        currentPanelIndex--;
        ShowPanel(currentPanelIndex);
    }

    [Serializable]
    public struct ToggleSpriteSet
    {
        public Button button;
        public Image targetImage;
        public Sprite selectedSprite;
        public Sprite deselectedSprite;
    }
}

using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    [Header("Script Reference")]
    public RhythmVisualizatorPro visualizatorScript;

    [Header("UI Reference")]
    [SerializeField] TMP_Dropdown dropdownList;
    [SerializeField] private GameObject snowEffect;
    [SerializeField] private TMP_Text snowButtonText;

    private bool isOn = true;
    private string currentUsername = "";

    private void Start()
    {
        currentUsername = PlayerPrefs.GetString("CurrentUsername", "");

        if (string.IsNullOrEmpty(currentUsername))
        {
            Debug.LogWarning("No username found → using default preferences");
        }

        LoadPreferences();

        if (dropdownList == null)
        {
            Debug.LogWarning("DropdownList is not assigned!");
            return;
        }

        dropdownList.ClearOptions();
        dropdownList.options.Add(new TMP_Dropdown.OptionData("Expansible Circle"));
        dropdownList.options.Add(new TMP_Dropdown.OptionData("Circle"));
        dropdownList.options.Add(new TMP_Dropdown.OptionData("Sphere"));

        RectTransform templateRect = dropdownList.template;

        if (templateRect != null)
        {
            // Set the dropdown panel (template) height to 200
            templateRect.sizeDelta = new Vector2(templateRect.sizeDelta.x, 200f);

            // Find the Viewport
            Transform viewport = templateRect.Find("Viewport");
            if (viewport != null)
            {
                RectTransform viewportRect = viewport as RectTransform;
                viewportRect.anchorMin = new Vector2(0, 0);
                viewportRect.anchorMax = new Vector2(1, 1);
                viewportRect.sizeDelta = Vector2.zero; // Fill parent
            }

            // Find the Content
            Transform content = viewport != null ? viewport.Find("Content") : null;
            if (content != null)
            {
                RectTransform contentRect = content as RectTransform;

                // Set fixed height for Content (same as template)
                contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, 200f);
                contentRect.anchorMin = new Vector2(0, 1); // Top-left anchor
                contentRect.anchorMax = new Vector2(1, 1);
                contentRect.pivot = new Vector2(0.5f, 1);

                // Add or get Vertical Layout Group
                VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
                if (vlg == null)
                {
                    vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
                }
            }

            // Make sure ScrollRect doesn't interfere
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

    private void LoadPreferences()
    {
        string prefix = "Prefs_" + currentUsername + "_";

        int savedViz = PlayerPrefs.GetInt(prefix + "Visualization", 0);
        dropdownList.value = savedViz;
        dropdownList.RefreshShownValue();

        isOn = PlayerPrefs.GetInt(prefix + "SnowEffect", 1) == 0;
        snowEffect.SetActive(isOn);
        snowButtonText.text = isOn ? "On" : "Off";
    }

    private void SavePreferences()
    {
        if (string.IsNullOrEmpty(currentUsername)) return;

        string prefix = "Prefs_" + currentUsername + "_";

        PlayerPrefs.SetInt(prefix + "Visualization", dropdownList.value);
        PlayerPrefs.SetInt(prefix + "SnowEffect", isOn ? 1 : 0);
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
        snowButtonText.text = isOn ? "On" : "Off";
        snowEffect.SetActive(isOn);

        SavePreferences();
    }

    public bool GetIsOn() => isOn;
}

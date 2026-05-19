using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderValueDisplay : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueText;

    private void Awake()
    {
        if(slider != null)
        {
            slider.wholeNumbers = true;
            slider.minValue = 1f;
            slider.maxValue = 100f;

            slider.onValueChanged.AddListener(UpdateDisplay);
            UpdateDisplay(slider.value);
        }
    }

    private void UpdateDisplay(float value)
    {
        if(valueText != null)
        {
            int intValue = Mathf.RoundToInt(value);
            valueText.text = intValue.ToString("D2");
        }
    }
}

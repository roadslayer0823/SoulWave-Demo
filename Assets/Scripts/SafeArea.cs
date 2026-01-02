using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeArea : MonoBehaviour
{
    private RectTransform rectTransform;
    private ScreenOrientation? lastOrientation;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    private void Update()
    {
        if(lastOrientation != Screen.orientation)
        {
            lastOrientation = Screen.orientation;
            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        var safeArea = Screen.safeArea;
        var normalizedMin = new Vector2(safeArea.x / Screen.width, safeArea.y / Screen.height);
        var normalizedMax = new Vector2((safeArea.x + safeArea.width) / Screen.width,
                                        (safeArea.y + safeArea.height) / Screen.height);
        rectTransform.anchorMin = normalizedMin;
        rectTransform.anchorMax = normalizedMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}

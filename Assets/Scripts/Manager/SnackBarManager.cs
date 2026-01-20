using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnackBarManager : MonoBehaviour
{
    [SerializeField] private GameObject snackBarPrefab;

    public static SnackBarManager Instance { get; private set; }
    private GameObject currentSnackBar = null;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowSnackbar(string message, float duration = -1, Color? textColor = null, bool isLoading = false)
    {
        if(currentSnackBar != null)
        {
            Destroy(currentSnackBar);
            currentSnackBar = null;
        }

        if (snackBarPrefab == null) return;

        Transform canvasRoot = FindActiveCanvasRoot();

        if (canvasRoot == null)
        {
            return;
        }

        var instance = Instantiate(snackBarPrefab, canvasRoot);
        currentSnackBar = instance;
        var snack = instance.GetComponent<SnackBar>();

        if (snack != null)
        {
            if (isLoading)
            {
                snack.ShowLoading(message);
            }
            else
            {
                snack.Show(message, duration, textColor);
            }
        }
        else
        {
            Destroy(instance);
        }
    }

    private Transform FindActiveCanvasRoot()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();

        foreach(Canvas canvas in canvases)
        {
            if(canvas.gameObject.activeInHierarchy && (canvas.renderMode == RenderMode.ScreenSpaceOverlay || canvas.gameObject.CompareTag("MainCanvas")))
            {
                return canvas.transform;
            }
        }

        foreach(Canvas canvas in canvases)
        {
            if (canvas.gameObject.activeInHierarchy)
            {
                return canvas.transform;
            }
        }

        return null;
    }
}

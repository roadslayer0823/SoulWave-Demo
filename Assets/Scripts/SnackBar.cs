using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SnackBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text messageText;

    [Header("Animation Settings")]
    [SerializeField] private float showDuration = 2.8f;
    [SerializeField] private float animationTime = 0.35f;

    private float bottomMargin = -350f;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private bool isLoadingMode = false;

    private static SnackBar _currentLoadingInstance;
    private Coroutine loadingDotsCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        if(rectTransform == null)
        {
            return;
        }

        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = new Vector2(0, -200);
    }

    public void Show(string message, float customDuration = -1f, Color? textColor = null)
    {
        if(_currentLoadingInstance != null && _currentLoadingInstance != this)
        {
            _currentLoadingInstance.Dismiss();
        }

        isLoadingMode = false;
        _currentLoadingInstance = null;

        InternalShow(message, customDuration > 0 ? customDuration : showDuration, textColor);
    }

    public void ShowLoading(string message)
    {
        if(_currentLoadingInstance != null && _currentLoadingInstance != this)
        {
            _currentLoadingInstance.Dismiss();
        }

        isLoadingMode = true;
        _currentLoadingInstance = this;

        InternalShow(message, Mathf.Infinity, Color.white);

        if (loadingDotsCoroutine != null) StopCoroutine(loadingDotsCoroutine);
        loadingDotsCoroutine = StartCoroutine(LoadingDotsAnimation(message));
    }

    private void InternalShow(string message, float duration, Color? textColor)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        messageText.text = message;

        if (textColor.HasValue)
        {
            messageText.color = textColor.Value;
        }

        LeanTween.cancel(gameObject);

        rectTransform.anchoredPosition = new Vector2(0, -200);
        canvasGroup.alpha = 0f;

        LeanTween.moveLocalY(gameObject, bottomMargin, animationTime).setEase(LeanTweenType.easeOutBack);
        LeanTween.alphaCanvas(canvasGroup, 1f, animationTime * 0.8f);

        if(!isLoadingMode && duration < Mathf.Infinity)
        {
            LTSeq sequence = LeanTween.sequence();
            sequence.append(duration);
            sequence.append(LeanTween.alphaCanvas(canvasGroup, 0f, animationTime));
            sequence.append(LeanTween.moveLocalY(gameObject, bottomMargin - 40f, animationTime * 0.8f).setEase(LeanTweenType.easeInQuad));
            sequence.append(() =>
            {
                Destroy(gameObject);
            });
        }
    }

    public void Dismiss()
    {
        if (isLoadingMode)
        {
            LeanTween.cancel(gameObject);
            LeanTween.alphaCanvas(canvasGroup, 0f, animationTime).setOnComplete(() => Destroy(gameObject));
            _currentLoadingInstance = null;
        }

        if(loadingDotsCoroutine != null)
        {
            StopCoroutine(loadingDotsCoroutine);
            loadingDotsCoroutine = null;
        }
    }

    // Quick static helpers
    public static void QuickShow(string message, float duration = -1f, Color? color = null)
    {
        if (SnackBarManager.Instance == null)
        {
            Debug.LogWarning("SnackbarManager not found!");
            return;
        }
        SnackBarManager.Instance.ShowSnackbar(message, duration, color);
    }

    public static void Success(string msg) => QuickShow(msg, 3.2f, new Color(0.8f, 1f, 0.75f));
    public static void Error(string msg) => QuickShow(msg, 3.6f, new Color(1f, 0.65f, 0.65f));
    public static void Warning(string msg) => QuickShow(msg, 3.2f, new Color(1f, 0.92f, 0.55f));
    public static void Info(string msg) => QuickShow(msg);

    public static void Loading(string message)
    {
        if (SnackBarManager.Instance == null) return;
        SnackBarManager.Instance.ShowSnackbar(message, Mathf.Infinity, Color.white, isLoading: true);
    }

    public static void DismissLoading()
    {
        if (_currentLoadingInstance != null)
        {
            _currentLoadingInstance.Dismiss();
        }
    }

    private IEnumerator LoadingDotsAnimation(string baseMessage)
    {
        int dotCount = 0;
        while (isLoadingMode)
        {
            string dots = new string('.', dotCount % 4);
            messageText.text = baseMessage + dots;
            dotCount++;
            yield return new WaitForSeconds(0.4f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject Background;
    public GameObject MainPagePanel;
    public GameObject CreateSoundPanel;
    public GameObject RecordVoicePanel;
    public GameObject UploadMp3Panel;
    public GameObject CustomTextPanel;
    public GameObject GenerateSFXPanel;
    public GameObject RecordingVoiceVisualizerPanel;
    public GameObject UploadVoiceVisualizerPanel;
    public GameObject TextToSpeechVisualizerPanel;
    public GameObject CommonUI;

    bool isRecordPanel;
    public bool isCustomText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        HomepageButton();
    }

    public void OpenVisualizerPanel()
    {
        if (isRecordPanel)
        {
            SetupCurrentUI(RecordingVoiceVisualizerPanel, false, true);
            RecordVoicePanel.SetActive(false);
        }
        else
        {
            SetupCurrentUI(UploadVoiceVisualizerPanel, false, true);
            UploadMp3Panel.SetActive(false);
        }
        VoiceCloningManager.Instance.submitButton.gameObject.SetActive(false);
        Background.SetActive(false);
    }

    public void OpenTextToSpeechVisualizerPanel()
    {
        TextToSpeechVisualizerPanel.SetActive(true);
        CustomTextPanel.SetActive(false);
        Background.SetActive(false);
    }

    public void OpenRecordVoicePanel()
    {
        isRecordPanel = true;
        isCustomText = false;
        SetupCurrentUI(RecordVoicePanel, false, true);
        VoiceCloningManager.Instance.ResetRecordingVoicePanel();
        CreateSoundPanel.SetActive(false);
    }

    public void OpenUploadMp3Panel()
    {
        isRecordPanel = false;
        isCustomText = false;
        SetupCurrentUI(UploadMp3Panel, false, true);
        VoiceCloningManager.Instance.ResetUploadingVoicePanel();
        CreateSoundPanel.SetActive(false);
    }

    public void OpenTextToSpeechPanel()
    {
        isCustomText = true;
        SetupCurrentUI(CustomTextPanel, false, true);
        VoiceCloningManager.Instance.ResetTextToSpeechPanel();
        CreateSoundPanel.SetActive(false);
        CommonUI.SetActive(true);
    }

    public void OpenGenerateSFXPanel()
    {
        SetupCurrentUI(GenerateSFXPanel, false, true);
    }

    public void OpenCreateSoundPanel()
    {
        SetupCurrentUI(CreateSoundPanel, false, true);
        CommonUI.SetActive(true);
    }

    public void CloseRecordingVoiceVisualizerPanel()
    {
        RecordingVoiceVisualizerPanel.SetActive(false);
    }

    public void CloseUploadVoiceVisualizerPanel()
    {
        UploadVoiceVisualizerPanel.SetActive(false);
    }

    public void CloseTextToSpeechVisualizerPanel()
    {
        TextToSpeechVisualizerPanel.SetActive(false);
    }

    public void SetupCurrentUI(GameObject targetPanel, bool isOpenMainPage, bool isOpenTargetPanel)
    {
        MainPagePanel.SetActive(isOpenMainPage);
        targetPanel.SetActive(isOpenTargetPanel);
    }

    public void HomepageButton()
    {
        MainPagePanel.SetActive(true);
        RecordVoicePanel.SetActive(false);
        UploadMp3Panel.SetActive(false);
        CustomTextPanel.SetActive(false);
        GenerateSFXPanel.SetActive(false);
        CommonUI.SetActive(false);
        CreateSoundPanel.SetActive(false);
        RecordingVoiceVisualizerPanel.SetActive(false);
        UploadVoiceVisualizerPanel.SetActive(false);
        TextToSpeechVisualizerPanel.SetActive(false);
        Background.SetActive(true);
        VoiceCloningManager.Instance.statusUpdate("");
        VoiceCloningManager.Instance.statusText.gameObject.SetActive(false);
        VoiceCloningManager.Instance.StopAudioSource();
    }
}

using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [Header("Sound Panel")]
    public GameObject CreateSoundPanel;
    public GameObject RecordVoicePanel;
    public GameObject UploadMp3Panel;
    public GameObject UploadPDFPanel;
    public GameObject CustomTextPanel;
    public GameObject GenerateSFXPanel;

    [Header("Visualizer Panel")]
    public GameObject RecordingVoiceVisualizerPanel;
    public GameObject UploadVoiceVisualizerPanel;
    public GameObject TextToSpeechVisualizerPanel;
    public Button[] visualizerButtons;
    
    [Header("Generic UI")]
    public GameObject SettingPanel;
    public GameObject CommonUI;
    public GameObject Background;
    public GameObject MainPagePanel;
    public GameObject snowEffect;

    [Header("Manager")]
    public SettingManager settingManager;

    private bool isRecordPanel;
    private bool isCustomText;
    private bool isUploadMP3;


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
            VoiceCloningManager.Instance.statusText.text = "";
            RecordVoicePanel.SetActive(false);
        }
        else
        {
            VoiceCloningManager.Instance.uploadAgainButton.onClick.RemoveAllListeners();
            SetupCurrentUI(UploadVoiceVisualizerPanel, false, true);
            if (isUploadMP3)
            {
                VoiceCloningManager.Instance.uploadAgainButton.onClick.AddListener(() => MP3PanelUploadAgainButton());
                UploadMp3Panel.SetActive(false);
            }
            else
            {
                VoiceCloningManager.Instance.uploadAgainButton.onClick.AddListener(() => PDFPanelUploadAgainButton());
                UploadPDFPanel.SetActive(false);
            }
        }
        VoiceCloningManager.Instance.submitButton.gameObject.SetActive(false);
        snowEffect.SetActive(false);
        Background.SetActive(false);
    }
    
    public void OpenTextToSpeechVisualizerPanel()
    {
        TextToSpeechVisualizerPanel.SetActive(true);
        CustomTextPanel.SetActive(false);
        snowEffect.SetActive(false);
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
        isUploadMP3 = true;
        isRecordPanel = false;
        isCustomText = false;
        SetupCurrentUI(UploadMp3Panel, false, true);
        VoiceCloningManager.Instance.ResetUploadingPanel();
        CreateSoundPanel.SetActive(false);
    }

    public void OpenUploadPDFPanel()
    {
        isUploadMP3 = false;
        isRecordPanel = false;
        isCustomText = false;
        CommonUI.SetActive(true);
        SetupCurrentUI(UploadPDFPanel, false, true);
        VoiceCloningManager.Instance.ResetUploadingPanel();
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

    public void OpenSettingPanel()
    {
        SettingPanel.SetActive(true);
        MainPagePanel.SetActive(false);
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

    public void RecordAgainButton()
    {
        OpenRecordVoicePanel();
        CloseRecordingVoiceVisualizerPanel();
        VoiceCloningManager.Instance.GetAudioSource().Stop();
    }

    public void InputAgainButton()
    {
        OpenTextToSpeechPanel();
        CloseTextToSpeechVisualizerPanel();
        VoiceCloningManager.Instance.customTextInput.text = "";
        VoiceCloningManager.Instance.GetAudioSource().Stop();
    }

    public void MP3PanelUploadAgainButton()
    {
        OpenUploadMp3Panel();
        CloseUploadVoiceVisualizerPanel();
        VoiceCloningManager.Instance.GetAudioSource().Stop();
    }

    public void PDFPanelUploadAgainButton()
    {
        OpenUploadPDFPanel();
        CloseUploadVoiceVisualizerPanel();
        VoiceCloningManager.Instance.GetAudioSource().Stop();
    }

    public void OKButton()
    {
        HomepageButton();
        CloseRecordingVoiceVisualizerPanel();
        CloseUploadVoiceVisualizerPanel();
        VoiceCloningManager.Instance.statusUpdate("");
        VoiceCloningManager.Instance.GetAudioSource().Stop();
    }

    public void DisableVisualizerButtons()
    {
        if (visualizerButtons == null) return;
        foreach(var btn in visualizerButtons)
        {
            if(btn != null) btn.interactable = false;
        }
    }

    public void EnableVisualizerButtons()
    {
        if (visualizerButtons == null) return;
        foreach (var btn in visualizerButtons)
        {
            if (btn != null) btn.interactable = true;
        }
    }

    public void SetupCurrentUI(GameObject targetPanel, bool isOpenMainPage, bool isOpenTargetPanel)
    {
        MainPagePanel.SetActive(isOpenMainPage);
        targetPanel.SetActive(isOpenTargetPanel);
        Background.SetActive(true);
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey("CurrentUsername");
        PlayerPrefs.Save();

        SceneManager.LoadScene("LoginScene");
    }

    public void HomepageButton()
    {
        MainPagePanel.SetActive(true);
        RecordVoicePanel.SetActive(false);
        SettingPanel.SetActive(false);
        UploadMp3Panel.SetActive(false);
        UploadPDFPanel.SetActive(false);
        CustomTextPanel.SetActive(false);
        GenerateSFXPanel.SetActive(false);
        CreateSoundPanel.SetActive(false);
        RecordingVoiceVisualizerPanel.SetActive(false);
        UploadVoiceVisualizerPanel.SetActive(false);
        TextToSpeechVisualizerPanel.SetActive(false);
        CommonUI.SetActive(false);
        Background.SetActive(true);
        VoiceCloningManager.Instance.SetIsLoadingPDF(false);
        VoiceCloningManager.Instance.submitButton.gameObject.SetActive(false);
        VoiceCloningManager.Instance.statusUpdate("");
        VoiceCloningManager.Instance.statusText.gameObject.SetActive(false);
        VoiceCloningManager.Instance.StopAudioSource();

        if(settingManager.GetIsOn() == true)
        {
            snowEffect.SetActive(true);
        }
    }
}

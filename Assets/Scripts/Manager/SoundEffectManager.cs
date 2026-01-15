using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class SoundEffectManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField sfxPromptInput;
    public Slider durationSlider;
    public Toggle loopToogle;
    public Button generateSFXButton;
    public TMP_Text statusText;
    public TMP_Text durationValueText;
    public TMP_Text promptInfluenceValueText;
    public AudioSource audioSource;

    private string elevenLabsAPIKey = "677dcc6d90d944bbddedcd2256ee3f0aba73e23c8b322322259b1a27fdebd6e1";

    private void Awake()
    {
        SetupSlider(durationSlider, 0.1f, 30f, 5f);
    }

    private void Update()
    {
        if (durationSlider != null && durationValueText != null)
            durationValueText.text = durationSlider.value.ToString("0.0") + "s";
    }

    private void Start()
    {
        generateSFXButton.onClick.AddListener(() => StartCoroutine(GenerateSFX()));
    }

    IEnumerator GenerateSFX()
    {
        string prompt = sfxPromptInput.text;
        if (string.IsNullOrEmpty(prompt))
        {
            UpdateStatus("Enter description.");
            yield break;
        }

        float duration = durationSlider != null ? durationSlider.value : 2f;
        float promptInfluence = 0.3f;
        bool loop = loopToogle != null ? loopToogle.isOn : false;

        UpdateStatus("Generating sound effect");
        string url = "https://api.elevenlabs.io/v1/sound-generation";
        var body = new SoundEffectRequest
        {
            text = prompt,
            duration_seconds = duration,
            prompt_influence = promptInfluence,
            loop = loop ? true : null
        };
        string jsonBody = JsonUtility.ToJson(body);
        Debug.Log("JSON SENT: " + jsonBody);
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] raw = Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(raw);

            www.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
            www.SetRequestHeader("xi-api-key", elevenLabsAPIKey);
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if(www.result != UnityWebRequest.Result.Success)
            {
                UpdateStatus("Generation failed: " + www.error);
                Debug.Log(www.downloadHandler.text);
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            if(clip != null)
            {
                audioSource.clip = clip;
                audioSource.loop = loop;
                audioSource.Play();
                UpdateStatus("Playing generated SFX (" + duration.ToString("0.0") + "s, " + (promptInfluence * 100f).ToString("0") + "% influence).");
            }
            else
            {
                UpdateStatus("Failed to load audio clip.");
            }
        }
    }

    private void SetupSlider(Slider targetSlider, float min, float max, float defaultValue)
    {
        if (targetSlider != null)
        {
            targetSlider.minValue = min;
            targetSlider.maxValue = max;
            targetSlider.value = defaultValue;
        }
    }

    void UpdateStatus(string s)
    {
        if (statusText)
        {
            statusText.text = s;
        }
        else
        {
            Debug.Log(s);
        }
    }

    [Serializable]
    public class SoundEffectRequest
    {
        public string text;
        public float duration_seconds;
        public float prompt_influence;
        public bool? loop;
    }
}

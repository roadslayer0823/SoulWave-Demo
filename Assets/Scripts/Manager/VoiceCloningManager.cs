using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System;
using System.IO;
using System.Text;
using System.Collections;
using TMPro;

public class VoiceCloningManager: MonoBehaviour
{
    [Header("UI Element")]
    public Button generateFromCustomTextButton;
    public Button recordingVoiceButton;
    public Button submitButton;
    public Button uploadMP3Button;
    public Button uploadPDFButton;
    public Button uploadAgainButton;
    public TMP_Text statusText;

    [Header("UI Input")]
    public TMP_InputField customTextInput;
    public Button generateCustomTextButton;

    [Header("Settings")]
    public string elevenLabsAPIKey = "sk_2e2194f3b42529a4b1071ab40ac1e82126704ca60a9d9f3a";  // Update with your real key!
    private string mp3FilePath = "";
    private string textFileName;
    private string micDevice;
    private string recordedFilePath;
    private string voiceId = "";
    private AudioSource audioSource;
    public static VoiceCloningManager Instance;
    public RhythmVisualizatorPro visualizer;

    private const int RECORD_DURATION = 30;
    private const int SAMPLE_RATE = 44100;

    [Header("Validation Settings")]
    public int minTextLength = 10;
    public int maxTextLength = 300;

    private bool wasValidLastFrame = false;
    private string lastShownError = "";

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        micDevice = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        visualizer = FindAnyObjectByType<RhythmVisualizatorPro>();

        if(visualizer != null)
        {
            visualizer.audioSource = audioSource;
        }

        if(customTextInput != null)
        {
            customTextInput.onValueChanged.AddListener(_ => UpdateCustomTextValidationUI());
        }

        // Null-check buttons before adding listeners
        if (generateCustomTextButton != null) generateFromCustomTextButton.onClick.AddListener(() => GenerateSpeechFromCustomText());
        if (uploadMP3Button != null) uploadMP3Button.onClick.AddListener(() => StartCoroutine(SelectAndUploadMP3()));
        if (recordingVoiceButton != null) recordingVoiceButton.onClick.AddListener(StartAudioRecordAndUpload);
        if (submitButton != null) submitButton.onClick.AddListener(() => GenerateSpeechFromTextFile());
        if (uploadPDFButton != null) uploadPDFButton.onClick.AddListener(() => StartCoroutine(SelectAndReadPDF()));

        // Initially hide submit button
        if (submitButton != null) submitButton.gameObject.SetActive(false);
    }

    public void GenerateSpeechFromTextFile()
    {
        string textToRead;
        string textFilePath = Path.Combine(Application.streamingAssetsPath, textFileName);

        if (!File.Exists(textFilePath))
        {
            Debug.Log($"Missing text file: {textFilePath}");
            return;
        }
        textToRead = File.ReadAllText(textFilePath);

        UIManager.Instance.OpenVisualizerPanel();
        StartCoroutine(GenerateSpeechFromString(textToRead));
    }

    public void GenerateSpeechFromCustomText()
    {
        string text = customTextInput.text.Trim();

        if (!generateCustomTextButton.interactable)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                SnackBar.Warning("Please enter some text first");
            }
            else if (text.Length < minTextLength)
            {
                SnackBar.Error($"Need at least {minTextLength} characters");
            }
            else if (text.Length > maxTextLength)
            {
                SnackBar.Error($"Too long — maximum {maxTextLength} characters");
            }
            return;
        }

        UIManager.Instance.OpenTextToSpeechVisualizerPanel();
        if (string.IsNullOrWhiteSpace(text)) return;
        StartCoroutine(GenerateSpeechFromString(text));
    }

    public void GenerateSpeechFromPDF_File()
    {
        StartCoroutine(SelectAndReadPDF());
    }

    public void StartAudioRecordAndUpload()
    {
        // Hide Start Recording button
        if (recordingVoiceButton != null)
        {
            recordingVoiceButton.gameObject.SetActive(false);
        }

        // Show Submit button but keep it disabled
        if (submitButton != null)
        {
            submitButton.gameObject.SetActive(true);
            submitButton.interactable = false;
        }

        statusUpdate("Preparing recording...");
        StartCoroutine(AutoRecordAndUpload());
    }

    IEnumerator SelectAndUploadMP3()
    {
#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select Audio File", "", "mp3,wav,ogg,m4a");
        if (string.IsNullOrEmpty(path))
        {
            yield break;
        }
        mp3FilePath = path;
#else
        string selectedPath = null;
        NativeFilePicker.PickFile((path) => { selectedPath = path; },
        new string[] { "public.audio", "audio/*", "audio/mpeg", "audio/mp3", "audio/wav", "audio/x-wav", "audio/ogg", "audio/x-m4a" });

        float timeout = 60f;
        float elapsed = 0f;
        while (selectedPath == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (string.IsNullOrEmpty(selectedPath))
        {
            statusUpdate("Audio selection cancelled");
            yield break;
        }
        string path = selectedPath;
#endif
        string ext = Path.GetExtension(path).ToLower();
        bool isValidAudio = ext == ".mp3" || ext == ".wav" || ext == ".ogg" || ext == ".m4a";

        if(!isValidAudio || !File.Exists(path))
        {
            SnackBar.Error("Please select a valid audio file (mp3, wav, ogg, m4a)");
            yield break;
        }

        mp3FilePath = path;
        Debug.Log("Valid audio selected: " + path);

        if (uploadMP3Button != null)
        {
            uploadMP3Button.gameObject.SetActive(false);
        }

        if (submitButton != null)
        {
            submitButton.gameObject.SetActive(true);
            submitButton.interactable = false;
        }

        string isolatedPath = Path.Combine(Application.persistentDataPath, "isolatedVoice.mp3");
        yield return IsolateVoice(mp3FilePath, isolatedPath);
    }

    IEnumerator AutoRecordAndUpload()
    {
        if (string.IsNullOrEmpty(micDevice))
        {
            SnackBar.Error("No microphone detected on this device");
            statusUpdate("No microphone detected");
            yield break;
        }
        AudioClip clip = Microphone.Start(micDevice, false, RECORD_DURATION, SAMPLE_RATE);


        statusUpdate($"Recording voice...");
        StartCoroutine(RecordingCountdownUI(RECORD_DURATION));

        yield return new WaitForSeconds(RECORD_DURATION);

        Microphone.End(micDevice);
        statusUpdate("Recording complete. Saving file...");

        recordedFilePath = Path.Combine(Application.persistentDataPath, "recordedVoice.wav");
        SaveWavFile(clip, recordedFilePath);

        statusUpdate("Uploading voice...");
        yield return DetectLanguageAndSetContext(recordedFilePath);
        yield return UploadVoiceToElevenLabs(recordedFilePath);
        
    }

    IEnumerator RecordingCountdownUI(int duration)
    {
        int timeLeft = duration;

        while (timeLeft > 0)
        {
            statusText.text = $"Recording in progress\nPress Submit when you are finished... { timeLeft}s.";

            timeLeft--;
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator UploadVoiceToElevenLabs(string audioPath)
    {
        if (!File.Exists(audioPath))
        {
            statusUpdate("Audio file not found: " + audioPath);
            yield break;
        }

        string fileExt = Path.GetExtension(audioPath).ToLower();
        string mimeType = fileExt == "mp3" ? "audio/mpeg" : "audio/wav";

        WWWForm form = new WWWForm();
        form.AddField("name", "UnityDemoVoice");
        form.AddBinaryData("files", File.ReadAllBytes(audioPath), Path.GetFileName(audioPath), mimeType);

        using (UnityWebRequest www = UnityWebRequest.Post("https://api.elevenlabs.io/v1/voices/add", form))
        {
            www.SetRequestHeader("xi-api-key", elevenLabsAPIKey);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                statusUpdate("Upload failed: " + www.error);
                SnackBar.Error("Failed to upload voice to server");
                Debug.Log("Upload Response: " + www.downloadHandler.text);
            }
            else
            {
                string json = www.downloadHandler.text;
                Debug.Log("Upload response: " + json);
                SnackBar.Success("Voice uploaded successfully!");

                var response = JsonUtility.FromJson<VoiceUploadResponse>(json);
                if (!string.IsNullOrEmpty(response.voice_id))
                {
                    voiceId = response.voice_id;
                    Debug.Log("Voice ID: " + voiceId);
                }

                EnableSubmitButton();
            }
        }
    }

    IEnumerator FilterAndUpload(string mp3Path)
    {
        string ext = Path.GetExtension(mp3Path).ToLower();

        if(ext == ".ogg")
        {
            mp3Path = ConvertOggToWav(mp3Path);
            ext = ".wav";
        }

        AudioType audioType = GetAudioTypeFromExtension(mp3Path);
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + mp3Path, audioType))
        {
            yield return www.SendWebRequest();
            if(www.result != UnityWebRequest.Result.Success)
            {
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            AudioClip filtered = PreprocessAudioClip(clip);
            AudioClip trimmed = TrimAudioClip(filtered, 10f);

            string filteredPath = Path.Combine(Application.persistentDataPath, "filteredVoice.wav");
            SaveWavFile(trimmed, filteredPath);

            statusUpdate("uploading...");
            yield return UploadVoiceToElevenLabs(filteredPath);
        }
    }

    private IEnumerator GenerateSpeechFromString(string textToSpeak)
    {
        if (string.IsNullOrWhiteSpace(textToSpeak))
        {
            SnackBar.Error("No text to speak");
            yield break;
        }

        UIManager.Instance.DisableVisualizerButtons();
        SnackBar.Loading("Generating voice");

        // Use cloned voice if available, otherwise fallback to default
        string usedVoiceId = string.IsNullOrEmpty(voiceId)
            ? "21m00Tcm4TlvDq8ikWAM"   // ← Rachel (default English voice)
            : voiceId;

        string url = $"https://api.elevenlabs.io/v1/text-to-speech/{usedVoiceId}";

        var request = new ElevenLabsTTSRequest(textToSpeak);
        string jsonBody = JsonUtility.ToJson(request);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("xi-api-key", elevenLabsAPIKey);
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "audio/mpeg");

            www.SetRequestHeader("xi-api-prefer-streaming", "false");
            www.timeout = 60;

            SnackBar.Loading("Sending request");

            yield return www.SendWebRequest();
            UIManager.Instance.EnableVisualizerButtons();
            SnackBar.DismissLoading();

            if (www.result != UnityWebRequest.Result.Success)
            {
                string error = www.downloadHandler.text;
                Debug.LogError($"TTS failed: {www.error}\nResponse: {error}");
                SnackBar.Error("Failed to generate voice - check API key & internet");
            }
            else
            {
                byte[] audioData = www.downloadHandler.data;
                yield return PlayGeneratedAudio(audioData);
                SnackBar.Info("Playing Audio...");
            }
        }
    }

    IEnumerator PlayGeneratedAudio(byte[] audioData)
    {
        string tempPath = Path.Combine(Application.persistentDataPath, "generated.mp3");
        File.WriteAllBytes(tempPath, audioData);

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (audioSource != null)
                {
                    audioSource.clip = clip;
                    audioSource.Play();
                }
                else
                {
                    Debug.Log("❌ AudioSource missing—add component!");
                }
            }
            else
            {
                Debug.Log("❌ Playback failed: " + www.error);
            }
        }
    }

    //detect recorded voice language
    IEnumerator DetectLanguageAndSetContext(string wavPath)
    {
        if (!File.Exists(wavPath))
        {
            statusUpdate("Audio file missing for language detection.");
            yield break;
        }

        byte[] audioBytes = File.ReadAllBytes(wavPath);

        WWWForm form = new WWWForm();
        form.AddField("model_id", "scribe_v1");
        form.AddBinaryData("file", audioBytes, Path.GetFileName(wavPath), "audio/wav");

        using (UnityWebRequest www = UnityWebRequest.Post("https://api.elevenlabs.io/v1/speech-to-text", form))
        {
            www.SetRequestHeader("xi-api-key", elevenLabsAPIKey);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                statusUpdate("Language detection failed: " + www.error);
                Debug.LogError("ElevenLabs STT Error: " + www.downloadHandler.text);
                yield break;
            }

            string json = www.downloadHandler.text;
            Debug.Log("ElevenLabs STT Response: " + json);

            STTResponse resp = JsonUtility.FromJson<STTResponse>(json);
            if (!string.IsNullOrEmpty(resp.language_code))
            {
                if (resp.language_code.StartsWith("zh"))
                {
                    textFileName = "speech_chinese.txt";
                    statusUpdate("Language detected: Chinese");
                }
                else
                {
                    textFileName = "speech_english.txt";
                    statusUpdate("Language detected: English");
                }
                Debug.Log("Language detected: " + resp.language_code);
            }
            else
            {
                textFileName = "speech_english.txt";
                statusUpdate("Language detection failed, defaulting to English");
                Debug.Log("No language_code returned, defaulting to English");
            }
        }
    }

    IEnumerator IsolateVoice(string inputPath, string outputPath)
    {
        if (!File.Exists(inputPath))
        {
            SnackBar.Error("Selected audio file not found");
            Debug.Log("Input file missing for isolation.");
            yield break;
        }

        string fileExt = Path.GetExtension(inputPath).ToLower();
        string mimeType = fileExt == ".mp3" ? "audio/mpeg" : "audio/wav";

        WWWForm form = new WWWForm();
        form.AddField("output_format", "wav");
        form.AddBinaryData("audio", File.ReadAllBytes(inputPath), Path.GetFileName(inputPath), mimeType);

        using (UnityWebRequest www = UnityWebRequest.Post("https://api.elevenlabs.io/v1/audio-isolation", form))
        {
            www.SetRequestHeader("xi-api-key", elevenLabsAPIKey);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                string errorMsg = www.error + " | Response: " + www.downloadHandler.text;
                Debug.Log("Isolation failed: " + errorMsg);
                yield break;
            }

            // Save isolated WAV
            byte[] isolatedData = www.downloadHandler.data;
            File.WriteAllBytes(outputPath, isolatedData);
            Debug.Log("Voice isolated successfully!");

            // --- ADD DETAILED DEBUG LOG ---
            Debug.Log($"[Isolation Debug] Isolated WAV path: {outputPath}");
            Debug.Log($"[Isolation Debug] Isolated WAV size: {isolatedData.Length} bytes");

            yield return DetectLanguageAndSetContext(outputPath);
            yield return FilterAndUpload(inputPath);
        }
    }

    //Upload PDF file
    IEnumerator SelectAndReadPDF()
    {
        string pdfPath = null;

#if UNITY_EDITOR
        pdfPath = UnityEditor.EditorUtility.OpenFilePanel("Select PDF File", "", "pdf");
#else
       bool filePicked = false;
       NativeFilePicker.PickFile((path) =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                pdfPath = path;
                filePicked = true;
            }
        }, new string[] { "com.adobe.pdf", "application/pdf" });
        
        yield return new WaitUntil(() => filePicked || pdfPath != null);
#endif
        if (string.IsNullOrEmpty(pdfPath))
        {
            SnackBar.Error("Not PDF Selected");
            yield break;
        }

        string ext = Path.GetExtension(pdfPath).ToLower();

        if (ext != ".pdf" || !File.Exists(pdfPath))
        {
            SnackBar.Error("Please select a valid PDF file");
            yield break;
        }

        UIManager.Instance.DisableVisualizerButtons();
        SnackBar.Loading("Extracting text");

        string extractedText = ExtractTextFromPDF(pdfPath);

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            SnackBar.Error("Could not extract any text from this PDF");
            SnackBar.DismissLoading();
            UIManager.Instance.EnableVisualizerButtons();
            yield break;
        }

        if (extractedText.Length > 5000)
        {
            int lastPeriod = extractedText.LastIndexOf('.', 5000);
            if (lastPeriod > 1000)
            {
                extractedText = extractedText.Substring(0, lastPeriod + 1);
            }
            else
            {
                extractedText = extractedText.Substring(0, 5000);
            }
            SnackBar.Warning("Text truncated to 5000 chars (API limit)");
        }

        UIManager.Instance.OpenVisualizerPanel();

        SnackBar.Loading("Generating voice");

        yield return GenerateSpeechFromString(extractedText);

        SnackBar.DismissLoading();
        SnackBar.Success("Generated Complete");
    }

    private void EnableSubmitButton()
    {
        if (submitButton != null)
        {
            submitButton.interactable = true;
        }
    }

    public void statusUpdate(string message)
    {
        if (statusText != null)
            statusText.text = message;
        else
            Debug.Log(message);
    }

    public void SaveGeneratedAudio()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "generated.mp3");

        if (!File.Exists(filePath))
        {
            Debug.Log("Audio file not found!");
            return;
        }

        NativeShare share = new NativeShare();
        share.AddFile(filePath, "audio/mpeg");
        share.SetSubject("My Generated Voice");
        share.SetText("Check out this generated audio!");
        share.Share();

        Debug.Log("Opening share options...");
    }

    public void ReplayGeneratedAudio()
    {
        if (!audioSource || !audioSource.clip) return;

        audioSource.Stop();
        audioSource.timeSamples = 0;
        audioSource.Play();
    }

    public void ResetRecordingVoicePanel()
    {
        statusText.text = "Press “Start Recording” and read the passage above, or read anything you like.";
        submitButton.gameObject.SetActive(false);
        statusText.gameObject.SetActive(true);
        recordingVoiceButton.gameObject.SetActive(true);
    }

    public void ResetUploadingPanel()
    {
        submitButton.gameObject.SetActive(false);
        statusText.gameObject.SetActive(false);
        uploadMP3Button.gameObject.SetActive(true);
    }

    public void ResetTextToSpeechPanel()
    {
        submitButton.gameObject.SetActive(false);
        statusText.gameObject.SetActive(false);
        customTextInput.text = "";
    }

    public void StopAudioSource()
    {
        if (!audioSource || !audioSource.clip) return;
        audioSource.Stop();
    }

    void SaveWavFile(AudioClip clip, string path)
    {
        var samples = new float[clip.samples];
        clip.GetData(samples, 0);

        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        int byteRate = SAMPLE_RATE * 2;
        int fileSize = 36 + samples.Length * 2;

        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(fileSize);
        writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(SAMPLE_RATE);
        writer.Write(byteRate);
        writer.Write((short)2);
        writer.Write((short)16);
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(samples.Length * 2);

        foreach (float s in samples)
        {
            short val = (short)(Mathf.Clamp(s, -1f, 1f) * short.MaxValue);
            writer.Write(val);
        }

        File.WriteAllBytes(path, stream.ToArray());
    }

    //return value
    private AudioClip PreprocessAudioClip(AudioClip input)
    {
        int sampleCount = input.samples * input.channels;
        float[] samples = new float[sampleCount];
        input.GetData(samples, 0);

        float max = 0f;
        for(int i = 0; i < samples.Length; i++)
        {
            max = Mathf.Max(max, Mathf.Abs(samples[i]));
        }
        if(max > 10f)
        {
            for(int i = 0; i < samples.Length; i++)
            {
                samples[i] /= max;
            }
        }

        int start = 0;
        int end = samples.Length - 1;
        float threshold = 0.02f;

        while(start < samples.Length && Mathf.Abs(samples[start]) < threshold)
        {
            start++;
        }
        while(end > start && Mathf.Abs(samples[end]) < threshold)
        {
            end--;
        }

        int newLength = end - start + 1;
        float[] trimmed = new float[newLength];
        Array.Copy(samples, start, trimmed, 0, newLength);

        AudioClip output = AudioClip.Create("FilteredVoice", newLength / input.channels, input.channels, input.frequency, false);
        output.SetData(trimmed, 0);
        return output;
    }

    private AudioClip TrimAudioClip(AudioClip clip, float seconds)
    {
        int samplesToKeep = Mathf.Min((int)(seconds * clip.frequency * clip.channels), clip.samples * clip.channels);

        float[] data = new float[samplesToKeep];
        clip.GetData(data, 0);

        AudioClip trimmed = AudioClip.Create(

            "TrimmedAudio",
            samplesToKeep / clip.channels,
            clip.channels,
            clip.frequency,
            false
        );
        trimmed.SetData(data, 0);
        return trimmed;
    }

    AudioType GetAudioTypeFromExtension(string path)
    {
        string ext = Path.GetExtension(path).ToLower();
        return ext switch
        {
            ".mp3" => AudioType.MPEG,
            ".wav" => AudioType.WAV,
            ".ogg" => AudioType.OGGVORBIS,
            _ => AudioType.UNKNOWN
        };
    }

    private string ConvertOggToWav(string oggPath)
    {
        string wavPath = Path.Combine(Application.persistentDataPath, Path.GetFileNameWithoutExtension(oggPath) + "_converted.wav");
        string ffmpegPath = "/opt/homebrew/bin/ffmpeg";

        if (!File.Exists(ffmpegPath))
        {
            Debug.LogError("FFmpeg not found at: " + ffmpegPath);
            return oggPath;
        }

        try
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = ffmpegPath;
            process.StartInfo.Arguments = $"-y -i \"{oggPath}\" -ar 44100 -ac 1 \"{wavPath}\"";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit();

            if (File.Exists(wavPath))
            {
                Debug.Log($"Converted .ogg → .wav: {wavPath}");
                return wavPath;
            }
            else
            {
                Debug.Log("FFmpeg conversion failed — no output file found.");
                return oggPath;
            }
        }
        catch (Exception e)
        {
            Debug.Log($"FFmpeg conversion error: {e.Message}");
            return oggPath;
        }
    }

    private string ExtractTextFromPDF(string pdfPath)
    {
        try
        {
            using (PdfDocument document = PdfDocument.Open(pdfPath))
            {
                StringBuilder sb = new StringBuilder();
                int pageCount = 0;
                int charCount = 0;

                foreach (Page page in document.GetPages())
                {
                    pageCount++;
                    string pageText = page.Text;
                    charCount += pageText.Length;
                    sb.Append(pageText);
                    sb.Append("\n\n");

                    Debug.Log($"Page {pageCount}: {pageText.Length} chars extracted");
                    if (pageText.Length > 0)
                        Debug.Log("First 50 chars: " + pageText.Substring(0, Math.Min(50, pageText.Length)));
                }

                Debug.Log($"Total pages: {pageCount} | Total chars: {charCount}");
                return sb.ToString().Trim();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"PDF error: {ex.Message}");
            return "";
        }
    }

    //validation
    private void UpdateCustomTextValidationUI()
    {
        if (customTextInput == null) return;

        string text = customTextInput.text.Trim();
        bool isEmpty = string.IsNullOrWhiteSpace(text);
        bool tooShort = !isEmpty && text.Length < minTextLength;
        bool tooLong = text.Length > maxTextLength;
        bool isValid = !isEmpty && !tooShort && !tooLong;

        generateCustomTextButton.interactable = !isEmpty && !tooShort && !tooLong;

        string currentError = "";

        if (isEmpty)
            currentError = "Please enter some text";
        else if (tooShort)
            currentError = $"Text too short (minimum {minTextLength} characters)";
        else if (tooLong)
            currentError = $"Text too long (maximum {maxTextLength} characters)";

        // Only show/change message if state changed or error is different
        if (!isValid && currentError != lastShownError)
        {
            if (isEmpty)
                SnackBar.Warning(currentError);
            else
                SnackBar.Error(currentError);

            lastShownError = currentError;
        }
        else if (isValid && !wasValidLastFrame)
        {
            lastShownError = "";
        }

        wasValidLastFrame = isValid;
    }

    [Serializable]
    public class STTResponse
    {
        public string language_code;
        public float language_probability;
    }

    [Serializable]
    public class SpeechRecognitionResponse
    {
        public Result[] results;
    }

    [Serializable]
    public class Result
    {
        public Alternative[] alternatives;
        public string languageCode;
    }

    [Serializable]
    public class Alternative
    {
        public string transcript;
    }

    [Serializable]
    class VoiceUploadResponse
    {
        public string voice_id;
    }

    [Serializable]
    public class ElevenLabsTTSRequest
    {
        public string text;
        public string model_id = "eleven_turbo_v2_5";
        public uint? seed = 42;
        public VoiceSettings voice_settings;

        public ElevenLabsTTSRequest(string t)
        {
            text = t;
            voice_settings = new VoiceSettings
            {
                stability = 0.7f,
                similarity_boost = 0.8f,
                style = 0.0f,
                use_speaker_boost = true
            };
        }
    }

    [Serializable]
    public class VoiceSettings
    {
        public float stability;
        public float similarity_boost;
        public float style;
        public bool use_speaker_boost;
    }

    public AudioSource GetAudioSource() => audioSource;
}

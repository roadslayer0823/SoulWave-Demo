using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    [Header("Session Settings")]
    [SerializeField] private float inactivityTimeoutMinutes = 10f;
    [SerializeField] private string loginSceneName = "LoginScene";

    private float lastInteractionTime;
    private bool isLoggedIn = false;
    private float lastBackgroundTime = 0f;
    private const string LAST_BACKGROUND_TIME_KEY = "LastBackgroundTime";

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            lastBackgroundTime = Time.time;
            PlayerPrefs.SetFloat(LAST_BACKGROUND_TIME_KEY, lastBackgroundTime);
            PlayerPrefs.Save();

            Debug.Log($"[Session] Backgrounded at {lastBackgroundTime:F1}");
        }
        else
        {
            float savedTime = PlayerPrefs.GetFloat(LAST_BACKGROUND_TIME_KEY, Time.time);
            float timeAway = Time.time - savedTime;

            Debug.Log($"[Session] Resumed after {timeAway:F1} seconds away");

            if (isLoggedIn && timeAway > inactivityTimeoutMinutes * 60f)
            {
                Debug.Log("[Session] Background inactivity exceeded timeout → Auto-logout");
                AutoLogout();
            }
            else
            {
                ResetTimer();
            }
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[SessionManager] Awake called — Instance created successfully");
    }

    private void Start()
    {
        ResetTimer();
        Debug.Log("[SessionManager] Start called — Timer initialized");
    }

    private void Update()
    {
        if (!isLoggedIn) return;

        if (Input.anyKeyDown || Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Debug.Log($"[Session] User interacted → Timer reset. Time since last: {Time.time - lastInteractionTime:F1}s");
            ResetTimer();
        }

        if (Time.frameCount % 300 == 0) // every ~5 seconds
        {
            Debug.Log($"[Session Debug] isLoggedIn = {isLoggedIn} | lastInteractionTime = {lastInteractionTime:F1} | current time = {Time.time:F1}");
        }

        if (Time.time - lastInteractionTime > inactivityTimeoutMinutes * 60f)
        {
            float idleDuration = Time.time - lastInteractionTime;
            Debug.Log($"[Session] Inactivity timeout reached! Idle for {idleDuration:F1}s > {inactivityTimeoutMinutes * 60f:F0}s → Auto-logout triggered");
            AutoLogout();
        }
    }

    public void OnLoginSuccess(bool isLogIn)
    {
        isLoggedIn = isLogIn;
    }

    public void OnLogout()
    {
        isLoggedIn = false;
        PlayerPrefs.DeleteKey("CurrentUsername");
        PlayerPrefs.Save();
        SceneManager.LoadScene(loginSceneName);
    }

    private void ResetTimer()
    {
        lastInteractionTime = Time.time;
    }

    private void AutoLogout()
    {
        SnackBar.Info("You have been logged out due to inactivity.");
        OnLogout();
    }
}

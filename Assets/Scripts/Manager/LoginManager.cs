using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;

    private string sheetUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vRFLRuvOIqaoywpeGrI3NzTZYBRUqla8gFpQSOks1tMoFYiixQN9Adrtlvj6g9Negfz5512U8GjSwvw/pub?output=csv";

    public void TryLogin()
    {
        StartCoroutine(LoginRoutine());
        SnackBar.Info("Logging in...");
    }

    IEnumerator LoginRoutine()
    {
        UnityWebRequest www = UnityWebRequest.Get(sheetUrl);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            string csvData = www.downloadHandler.text;

            string[] rows = csvData.Split('\n');
            bool loginSuccess = false;

            for(int i = 1; i < rows.Length; i++)
            {
                string row = rows[i].Trim();
                if (string.IsNullOrEmpty(row)) continue;

                string[] cols = row.Split(',');
                if (cols.Length < 2) continue;

                string username = cols[0].Trim();
                string password = cols[1].Trim();

                if (username == usernameField.text && password == passwordField.text)
                {
                    loginSuccess = true;
                    break;
                }
            }

            if (loginSuccess)
            {
                PlayerPrefs.SetString("CurrentUsername", usernameField.text);
                PlayerPrefs.Save();

                if(SessionManager.Instance != null)
                {
                    SessionManager.Instance.OnLoginSuccess(true);
                }
                SceneManager.LoadScene("MainScene");
            }
            else
            {
                SnackBar.Error("Invalid username or password. Please try again.");
                Debug.Log("Login failed: Invalid credentials");
            }
        }
        else
        {
            SnackBar.Error("Failed to connect to server. Check your internet connection.");
            Debug.Log("Error fetching sheet: " + www.error);
        }
    }
}

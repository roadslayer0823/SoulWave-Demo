using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour
{
    public float delay = 3f;

    private void Start()
    {
        StartCoroutine(LoadNext());
    }

    IEnumerator LoadNext()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("LoginScene");
    }
}

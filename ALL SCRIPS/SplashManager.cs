// 08/11/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SplashManager : MonoBehaviour
{
    public string nextScene = "InitialSetup";
    public float splashDuration = 3f;
    public Slider loadingSlider; // Remplace Image par Slider

    void Start()
    {
        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        float elapsed = 0f;
        while (elapsed < splashDuration)
        {
            elapsed += Time.deltaTime;
            if (loadingSlider != null)
            {
                loadingSlider.value = elapsed / splashDuration; // Utilise la valeur du Slider
            }
            yield return null;
        }
        SceneManager.LoadScene(nextScene);
    }
}

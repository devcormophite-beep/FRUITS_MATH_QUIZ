using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Contrôleur de navigation entre les scènes
/// Gère les transitions et sauvegarde automatique
/// </summary>
public class SceneController : MonoBehaviour
{
    private static SceneController _instance;
    public static SceneController Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SceneController");
                _instance = go.AddComponent<SceneController>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [Header("Loading Screen")]
    public GameObject loadingScreenPrefab;
    private GameObject loadingScreenInstance;
    private Image loadingBar;
    private CanvasGroup loadingCanvasGroup;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ========== NAVIGATION ==========

    public void GoToMainMenu()
    {
        LoadSceneWithTransition("MainMenu");
    }

    public void GoToGame()
    {
        LoadSceneWithTransition("Game");
    }

    public void GoToLeaderboard()
    {
        LoadSceneWithTransition("Leaderboard");
    }

    public void GoToProfile()
    {
        LoadSceneWithTransition("Profile");
    }

    public void GoToSettings()
    {
        LoadSceneWithTransition("Settings");
    }

    public void GoToAvatarSelection()
    {
        LoadSceneWithTransition("AvatarSelection");
    }

    public void GoToCountrySelection()
    {
        LoadSceneWithTransition("CountrySelection");
    }

    public void GoToUsernameSetup()
    {
        LoadSceneWithTransition("UsernameSetup");
    }

    public void QuitGame()
    {
        // Sauvegarder avant de quitter
        SaveBeforeExit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    // ========== CHARGEMENT DE SCÈNE ==========

    void LoadSceneWithTransition(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        // Sauvegarder avant de changer de scène
        SaveBeforeSceneChange();

        // Afficher l'écran de chargement
        ShowLoadingScreen();

        yield return new WaitForSeconds(0.3f);

        // Commencer le chargement asynchrone
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // Mettre à jour la barre de progression
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            UpdateLoadingBar(progress);

            // La scène est prête
            if (asyncLoad.progress >= 0.9f)
            {
                UpdateLoadingBar(1f);
                yield return new WaitForSeconds(0.3f);
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // Masquer l'écran de chargement
        yield return new WaitForSeconds(0.2f);
        HideLoadingScreen();
    }

    // ========== ÉCRAN DE CHARGEMENT ==========

    void ShowLoadingScreen()
    {
        if (loadingScreenPrefab != null && loadingScreenInstance == null)
        {
            loadingScreenInstance = Instantiate(loadingScreenPrefab);
            DontDestroyOnLoad(loadingScreenInstance);

            loadingCanvasGroup = loadingScreenInstance.GetComponent<CanvasGroup>();
            loadingBar = loadingScreenInstance.transform.Find("LoadingBar")?.GetComponent<Image>();

            if (loadingCanvasGroup != null)
            {
                loadingCanvasGroup.alpha = 0f;
                LeanTween.alphaCanvas(loadingCanvasGroup, 1f, 0.3f);
            }
        }
        else if (loadingScreenInstance != null)
        {
            loadingScreenInstance.SetActive(true);
            if (loadingCanvasGroup != null)
            {
                LeanTween.alphaCanvas(loadingCanvasGroup, 1f, 0.3f);
            }
        }
    }

    void HideLoadingScreen()
    {
        if (loadingScreenInstance != null && loadingCanvasGroup != null)
        {
            LeanTween.alphaCanvas(loadingCanvasGroup, 0f, 0.3f).setOnComplete(() =>
            {
                loadingScreenInstance.SetActive(false);
            });
        }
    }

    void UpdateLoadingBar(float progress)
    {
        if (loadingBar != null)
        {
            loadingBar.fillAmount = progress;
        }
    }

    // ========== SAUVEGARDE ==========

    void SaveBeforeSceneChange()
    {
        var dataService = DataService.Instance;
        if (dataService != null)
        {
            dataService.SaveCurrentPlayer();
            Debug.Log("✓ Données sauvegardées avant changement de scène");
        }
    }

    void SaveBeforeExit()
    {
        var dataService = DataService.Instance;
        if (dataService != null)
        {
            dataService.SaveCurrentPlayer();
            Debug.Log("✓ Données sauvegardées avant fermeture");
        }
    }

    // ========== MÉTHODES STATIQUES ==========

    public static void LoadScene(string sceneName)
    {
        Instance.LoadSceneWithTransition(sceneName);
    }

    public static void RestartCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Instance.LoadSceneWithTransition(currentScene);
    }

    // ========== BOUTONS UI (à attacher directement) ==========

    public void OnMainMenuButtonClicked() => GoToMainMenu();
    public void OnPlayButtonClicked() => GoToGame();
    public void OnLeaderboardButtonClicked() => GoToLeaderboard();
    public void OnProfileButtonClicked() => GoToProfile();
    public void OnSettingsButtonClicked() => GoToSettings();
    public void OnQuitButtonClicked() => QuitGame();
}
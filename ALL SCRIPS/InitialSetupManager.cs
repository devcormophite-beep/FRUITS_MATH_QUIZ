// 08/11/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class InitialSetupManager : MonoBehaviour
{
    [Header("Scènes")]
    public string languageSelectionSceneName = "LanguageSelection";
    public string usernameSceneName = "UsernameSetup";
    public string avatarSelectionSceneName = "AvatarSelection";
    public string countrySelectionSceneName = "CountrySelection";
    public string mainMenuSceneName = "MainMenu";

    [Header("Paramètres")]
    public bool requireLanguage = true;
    public bool requireUsername = true;
    public bool requireAvatar = true;
    public bool requireCountry = true;
    public float delayBetweenScenes = 0.5f;

    [Header("Debug")]
    public bool resetOnStart = false;

    void Start()
    {
        if (resetOnStart)
        {
            ResetAllSetup();
        }

        StartCoroutine(CheckSetupProgress());
    }

    IEnumerator CheckSetupProgress()
    {
        Debug.Log("========================================");
        Debug.Log("VÉRIFICATION DU SETUP INITIAL");
        Debug.Log("========================================");

        // 1. Vérifier la langue EN PREMIER
        bool hasLanguage = PlayerPrefs.HasKey("GameLanguage") && !string.IsNullOrEmpty(PlayerPrefs.GetString("GameLanguage"));
        Debug.Log($"Langue configurée: {hasLanguage}");

        if (requireLanguage && !hasLanguage)
        {
            Debug.Log("→ Chargement de la scène: Language Selection");
            yield return new WaitForSeconds(delayBetweenScenes);
            SceneManager.LoadScene(languageSelectionSceneName);
            yield break;
        }

        // 2. Vérifier le pseudo
        bool hasUsername = PlayerPrefs.HasKey("PlayerName") && !string.IsNullOrEmpty(PlayerPrefs.GetString("PlayerName"));
        Debug.Log($"Pseudo configuré: {hasUsername}");

        if (requireUsername && !hasUsername)
        {
            Debug.Log("→ Chargement de la scène: Username Setup");
            yield return new WaitForSeconds(delayBetweenScenes);
            SceneManager.LoadScene(usernameSceneName);
            yield break;
        }

        // 3. Vérifier l'avatar
        bool hasAvatar = PlayerPrefs.HasKey("SelectedAvatarId") && PlayerPrefs.GetInt("SelectedAvatarId", -1) > 0;
        Debug.Log($"Avatar configuré: {hasAvatar}");

        if (requireAvatar && !hasAvatar)
        {
            Debug.Log("→ Chargement de la scène: Avatar Selection");
            yield return new WaitForSeconds(delayBetweenScenes);
            SceneManager.LoadScene(avatarSelectionSceneName);
            yield break;
        }

        // 4. Vérifier le pays
        bool hasCountry = PlayerPrefs.HasKey("SelectedCountryId") && PlayerPrefs.GetInt("SelectedCountryId", -1) >= 0;
        Debug.Log($"Pays configuré: {hasCountry}");

        if (requireCountry && !hasCountry)
        {
            Debug.Log("→ Chargement de la scène: Country Selection");
            yield return new WaitForSeconds(delayBetweenScenes);
            SceneManager.LoadScene(countrySelectionSceneName);
            yield break;
        }

        // ✅ TOUT est configuré, aller au menu principal
        Debug.Log("✓ Setup complet!");
        Debug.Log($"  - Langue: {PlayerPrefs.GetString("GameLanguage")}");
        Debug.Log($"  - Pseudo: {PlayerPrefs.GetString("PlayerName")}");
        Debug.Log($"  - Avatar ID: {PlayerPrefs.GetInt("SelectedAvatarId")}");
        Debug.Log($"  - Pays ID: {PlayerPrefs.GetInt("SelectedCountryId")}");
        Debug.Log("→ Chargement du menu principal");
        Debug.Log("========================================\n");

        // Marquer le setup comme complété (optionnel, pour stats)
        PlayerPrefs.SetInt("SetupCompleted", 1);
        PlayerPrefs.Save();

        yield return new WaitForSeconds(delayBetweenScenes);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void ResetAllSetup()
    {
        PlayerPrefs.DeleteKey("GameLanguage");
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.DeleteKey("SelectedAvatarId");
        PlayerPrefs.DeleteKey("SelectedCountryId");
        PlayerPrefs.DeleteKey("SetupCompleted");
        PlayerPrefs.DeleteKey("FirstLaunch");
        PlayerPrefs.DeleteKey("HasSelectedLanguage");
        PlayerPrefs.Save();
        Debug.Log("✅ Setup réinitialisé!");
    }

    // Méthode statique pour vérifier si le setup est complet
    public static bool IsSetupComplete()
    {
        bool hasLanguage = PlayerPrefs.HasKey("GameLanguage") && !string.IsNullOrEmpty(PlayerPrefs.GetString("GameLanguage"));
        bool hasUsername = PlayerPrefs.HasKey("PlayerName") && !string.IsNullOrEmpty(PlayerPrefs.GetString("PlayerName"));
        bool hasAvatar = PlayerPrefs.HasKey("SelectedAvatarId") && PlayerPrefs.GetInt("SelectedAvatarId", -1) > 0;
        bool hasCountry = PlayerPrefs.HasKey("SelectedCountryId") && PlayerPrefs.GetInt("SelectedCountryId", -1) >= 0;

        return hasLanguage && hasUsername && hasAvatar && hasCountry;
    }

    // Méthode pour forcer le rechargement du setup
    public static void RedoSetup()
    {
        PlayerPrefs.DeleteKey("GameLanguage");
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.DeleteKey("SelectedAvatarId");
        PlayerPrefs.DeleteKey("SelectedCountryId");
        PlayerPrefs.DeleteKey("SetupCompleted");
        PlayerPrefs.Save();

        SceneManager.LoadScene("InitialSetup");
    }

    // Méthode pour tester le flow (utile en développement)
    [ContextMenu("Tester Flow Complet")]
    public void TestCompleteFlow()
    {
        ResetAllSetup();
        Debug.Log("⚠️ Redémarrez la scène InitialSetup pour tester le flow complet");
    }
}

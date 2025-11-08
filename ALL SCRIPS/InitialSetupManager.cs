// 08/11/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class InitialSetupManager : MonoBehaviour
{
    [Header("Scènes")]
    public string languageSelectionSceneName = "LanguageSelection"; // Ajout de la scène LanguageSelection
    public string usernameSceneName = "UsernameSetup";
    public string avatarSelectionSceneName = "AvatarSelection";
    public string countrySelectionSceneName = "CountrySelection";
    public string mainMenuSceneName = "MainMenu";

    [Header("Paramètres")]
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

        // Vérifier si c'est le premier démarrage
        if (!PlayerPrefs.HasKey("FirstLaunch"))
        {
            Debug.Log("Premier démarrage détecté. Chargement de la scène LanguageSelection.");
            PlayerPrefs.SetInt("FirstLaunch", 1); // Marquer le premier démarrage comme terminé
            PlayerPrefs.Save();
            SceneManager.LoadScene(languageSelectionSceneName);
            return;
        }

        StartCoroutine(CheckSetupProgress());
    }

    IEnumerator CheckSetupProgress()
    {
        Debug.Log("========================================");
        Debug.Log("VÉRIFICATION DU SETUP INITIAL");
        Debug.Log("========================================");

        // Vérifier le pseudo
        bool hasUsername = PlayerPrefs.HasKey("PlayerName") && !string.IsNullOrEmpty(PlayerPrefs.GetString("PlayerName"));
        Debug.Log($"Pseudo configuré: {hasUsername}");

        if (requireUsername && !hasUsername)
        {
            Debug.Log("→ Chargement de la scène: Username Setup");
            yield return new WaitForSeconds(delayBetweenScenes);
            SceneManager.LoadScene(usernameSceneName);
            yield break;
        }

        // Vérifier l'avatar
        bool hasAvatar = PlayerPrefs.HasKey("SelectedAvatarId") && PlayerPrefs.GetInt("SelectedAvatarId", -1) > 0;
        Debug.Log($"Avatar configuré: {hasAvatar}");

        if (requireAvatar && !hasAvatar)
        {
            Debug.Log("→ Chargement de la scène: Avatar Selection");
            yield return new WaitForSeconds(delayBetweenScenes);
            SceneManager.LoadScene(avatarSelectionSceneName);
            yield break;
        }

        // Vérifier le pays
        bool hasCountry = PlayerPrefs.HasKey("SelectedCountryId") && PlayerPrefs.GetInt("SelectedCountryId", -1) >= 0;
        Debug.Log($"Pays configuré: {hasCountry}");

        if (requireCountry && !hasCountry)
        {
            Debug.Log("→ Chargement de la scène: Country Selection");
            yield return new WaitForSeconds(delayBetweenScenes);
            SceneManager.LoadScene(countrySelectionSceneName);
            yield break;
        }

        // Tout est configuré, aller au menu principal
        Debug.Log("✓ Setup complet!");
        Debug.Log($"  - Pseudo: {PlayerPrefs.GetString("PlayerName")}");
        Debug.Log($"  - Avatar ID: {PlayerPrefs.GetInt("SelectedAvatarId")}");
        Debug.Log($"  - Pays ID: {PlayerPrefs.GetInt("SelectedCountryId")}");
        Debug.Log("→ Chargement du menu principal");
        Debug.Log("========================================\\n");

        yield return new WaitForSeconds(delayBetweenScenes);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void ResetAllSetup()
    {
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.DeleteKey("SelectedAvatarId");
        PlayerPrefs.DeleteKey("SelectedCountryId");
        PlayerPrefs.DeleteKey("HasCompletedSetup");
        PlayerPrefs.DeleteKey("FirstLaunch"); // Réinitialiser le premier démarrage
        PlayerPrefs.Save();
        Debug.Log("Setup réinitialisé!");
    }

    // Méthode statique pour vérifier si le setup est complet
    public static bool IsSetupComplete()
    {
        bool hasUsername = PlayerPrefs.HasKey("PlayerName") && !string.IsNullOrEmpty(PlayerPrefs.GetString("PlayerName"));
        bool hasAvatar = PlayerPrefs.HasKey("SelectedAvatarId");
        bool hasCountry = PlayerPrefs.HasKey("SelectedCountryId");

        return hasUsername && hasAvatar && hasCountry;
    }

    // Méthode pour forcer le rechargement du setup
    public static void RedoSetup()
    {
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.DeleteKey("SelectedAvatarId");
        PlayerPrefs.DeleteKey("SelectedCountryId");
        PlayerPrefs.Save();

        SceneManager.LoadScene("InitialSetup");
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Script de debug pour tester et vérifier la cohérence des PlayerPrefs
/// À attacher à un GameObject dans votre scène de test
/// </summary>
public class PlayerPrefsDebugger : MonoBehaviour
{
    [Header("UI de Debug")]
    public TextMeshProUGUI debugText;
    public Button printButton;
    public Button testSaveButton;
    public Button testLoadButton;
    public Button resetButton;
    public Button testLeaderboardButton;

    [Header("Champs de test")]
    public TMP_InputField testNameField;
    public TMP_InputField testAvatarField;
    public TMP_InputField testCountryField;
    public TMP_InputField testScoreField;

    void Start()
    {
        SetupButtons();
        PrintCurrentState();
    }

    void SetupButtons()
    {
        if (printButton != null)
            printButton.onClick.AddListener(PrintCurrentState);

        if (testSaveButton != null)
            testSaveButton.onClick.AddListener(TestSave);

        if (testLoadButton != null)
            testLoadButton.onClick.AddListener(TestLoad);

        if (resetButton != null)
            resetButton.onClick.AddListener(TestReset);

        if (testLeaderboardButton != null)
            testLeaderboardButton.onClick.AddListener(TestLeaderboardData);
    }

    void PrintCurrentState()
    {
        Debug.Log("========================================");
        Debug.Log("🔍 DEBUG - ÉTAT DES PLAYERPREFS");
        Debug.Log("========================================");

        var manager = PlayerPrefsManager.Instance;
        manager.PrintAllPlayerPrefs();

        if (debugText != null)
        {
            string info = "ÉTAT ACTUEL:\n\n";
            info += $"Nom: {manager.GetPlayerName()}\n";
            info += $"Avatar ID: {manager.GetAvatarId()}\n";
            info += $"Pays ID: {manager.GetCountryId()}\n";
            info += $"Pays Nom: {manager.GetCountryName()}\n";
            info += $"Score: {manager.GetTotalScore()}\n";
            info += $"Niveau: {manager.GetHighestLevel()}\n";
            info += $"Parties: {manager.GetGamesPlayed()}\n";
            info += $"Victoires: {manager.GetGamesWon()}\n";
            info += $"WinRate: {manager.GetWinRate():F1}%\n";
            info += $"\nProfil complet: {(manager.IsProfileComplete() ? "✅ OUI" : "❌ NON")}";

            debugText.text = info;
        }
    }

    void TestSave()
    {
        Debug.Log("🧪 TEST - Sauvegarde de données de test");

        var manager = PlayerPrefsManager.Instance;

        string name = testNameField != null && !string.IsNullOrEmpty(testNameField.text)
            ? testNameField.text
            : "TestPlayer";

        int avatarId = 1;
        if (testAvatarField != null && int.TryParse(testAvatarField.text, out int parsedAvatar))
        {
            avatarId = parsedAvatar;
        }

        int countryId = 1;
        if (testCountryField != null && int.TryParse(testCountryField.text, out int parsedCountry))
        {
            countryId = parsedCountry;
        }

        int score = 1000;
        if (testScoreField != null && int.TryParse(testScoreField.text, out int parsedScore))
        {
            score = parsedScore;
        }

        // Sauvegarde complète
        manager.SaveCompleteProfile(name, avatarId, countryId, "TestCountry");
        manager.SetTotalScore(score);
        manager.SetHighestLevel(5);
        manager.IncrementGamesPlayed();
        manager.IncrementGamesWon();

        Debug.Log("✅ Données de test sauvegardées");
        PrintCurrentState();
    }

    void TestLoad()
    {
        Debug.Log("🧪 TEST - Chargement des données");
        PrintCurrentState();
    }

    void TestReset()
    {
        Debug.Log("🧪 TEST - Réinitialisation");

#if UNITY_EDITOR
        if (EditorUtility.DisplayDialog(
            "Confirmation",
            "Voulez-vous vraiment réinitialiser TOUTES les données ?",
            "Oui", "Non"))
        {
            PlayerPrefsManager.Instance.ResetAllData();
            Debug.Log("✅ Données réinitialisées");
            PrintCurrentState();
        }
#else
        // En build, demander confirmation via un autre moyen
        Debug.LogWarning("⚠️ Réinitialisation demandée - implémentez une UI de confirmation");
        PlayerPrefsManager.Instance.ResetAllData();
        Debug.Log("✅ Données réinitialisées");
        PrintCurrentState();
#endif
    }

    void TestLeaderboardData()
    {
        Debug.Log("🧪 TEST - Données pour le leaderboard");
        Debug.Log("========================================");

        var manager = PlayerPrefsManager.Instance;
        PlayerData data = manager.GetPlayerDataForLeaderboard();

        string output = "DONNÉES POUR LOOTLOCKER:\n\n";
        output += $"Player ID: {data.playerId}\n";
        output += $"Player Name: {data.playerName}\n";
        output += $"Avatar ID: {data.avatarId}\n";
        output += $"Country ID: {data.countryId}\n";
        output += $"Score: {data.totalScore}\n";
        output += $"Highest Level: {data.highestLevel}\n";
        output += $"Games Played: {data.gamesPlayed}\n";
        output += $"Games Won: {data.gamesWon}\n";
        output += $"Win Rate: {data.GetWinRate():F1}%\n";

        if (debugText != null)
        {
            debugText.text = output;
        }

        Debug.Log(output);

        // Test de création des métadonnées JSON
        LeaderboardEntryMetadata metadata = new LeaderboardEntryMetadata
        {
            level = data.highestLevel,
            avatar_id = data.avatarId,
            country_id = data.countryId,
            win_rate = data.GetWinRate()
        };

        string json = JsonUtility.ToJson(metadata, true);
        Debug.Log($"JSON Metadata:\n{json}");
    }

    // Touches de raccourci pour le debug
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PrintCurrentState();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            TestSave();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            TestLoad();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            TestLeaderboardData();
        }
    }
}

#if UNITY_EDITOR
// Classe pour afficher un bouton dans l'Inspector
[CustomEditor(typeof(PlayerPrefsDebugger))]
public class PlayerPrefsDebuggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerPrefsDebugger debugger = (PlayerPrefsDebugger)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Actions Rapides", EditorStyles.boldLabel);

        if (GUILayout.Button("Afficher l'état actuel (P)"))
        {
            debugger.SendMessage("PrintCurrentState");
        }

        if (GUILayout.Button("Sauvegarder données de test (S)"))
        {
            debugger.SendMessage("TestSave");
        }

        if (GUILayout.Button("Charger données (L)"))
        {
            debugger.SendMessage("TestLoad");
        }

        if (GUILayout.Button("Test données Leaderboard (T)"))
        {
            debugger.SendMessage("TestLeaderboardData");
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("RÉINITIALISER TOUT", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog(
                "Confirmation",
                "Voulez-vous vraiment réinitialiser TOUTES les données ?",
                "Oui", "Non"))
            {
                PlayerPrefsManager.Instance.ResetAllData();
                Debug.Log("✅ PlayerPrefs réinitialisés");
            }
        }
    }
}
#endif
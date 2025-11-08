#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Outils de développement pour faciliter le testing
/// Accessible via le menu Unity "Tools/Game Debug"
/// </summary>
public class DevTools : EditorWindow
{
    private Vector2 scrollPosition;
    private int dummyPlayersCount = 20;
    private int scoreToAdd = 100;
    private int levelToSet = 1;

    [MenuItem("Tools/Game Debug/Dev Panel")]
    static void Init()
    {
        DevTools window = (DevTools)EditorWindow.GetWindow(typeof(DevTools));
        window.titleContent = new GUIContent("Dev Tools");
        window.Show();
    }

    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label("🛠️ OUTILS DE DÉVELOPPEMENT", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // ========== SECTION JOUEURS ==========
        DrawSection("👤 Gestion des Joueurs", () =>
        {
            if (GUILayout.Button("📊 Afficher Info Joueur Actuel"))
            {
                PrintCurrentPlayerInfo();
            }

            if (GUILayout.Button("🆕 Créer Nouveau Joueur"))
            {
                CreateNewPlayer();
            }

            dummyPlayersCount = EditorGUILayout.IntField("Nombre de joueurs:", dummyPlayersCount);
            if (GUILayout.Button($"🎭 Générer {dummyPlayersCount} Joueurs Fictifs"))
            {
                GenerateTestPlayers();
            }
        });

        GUILayout.Space(10);

        // ========== SECTION SCORES ==========
        DrawSection("🎯 Modification des Scores", () =>
        {
            scoreToAdd = EditorGUILayout.IntField("Points à ajouter:", scoreToAdd);
            if (GUILayout.Button($"➕ Ajouter {scoreToAdd} Points"))
            {
                AddScore();
            }

            if (GUILayout.Button("🔄 Réinitialiser Score"))
            {
                ResetScore();
            }

            levelToSet = EditorGUILayout.IntField("Niveau à définir:", levelToSet);
            if (GUILayout.Button($"📈 Définir Niveau {levelToSet}"))
            {
                SetLevel();
            }
        });

        GUILayout.Space(10);

        // ========== SECTION STATISTIQUES ==========
        DrawSection("📈 Statistiques", () =>
        {
            if (GUILayout.Button("📊 Afficher Leaderboard Console"))
            {
                PrintLeaderboard();
            }

            if (GUILayout.Button("🏆 Afficher Mon Rang"))
            {
                PrintMyRank();
            }

            if (GUILayout.Button("📉 Statistiques Détaillées"))
            {
                PrintDetailedStats();
            }
        });

        GUILayout.Space(10);

        // ========== SECTION DONNÉES ==========
        DrawSection("💾 Gestion des Données", () =>
        {
            if (GUILayout.Button("💾 Sauvegarder Manuellement"))
            {
                SaveData();
            }

            if (GUILayout.Button("🔄 Recharger Données"))
            {
                ReloadData();
            }

            GUILayout.Space(5);
            GUI.color = Color.red;
            if (GUILayout.Button("⚠️ EFFACER TOUTES LES DONNÉES"))
            {
                if (EditorUtility.DisplayDialog("Confirmation",
                    "Êtes-vous sûr de vouloir effacer TOUTES les données?",
                    "Oui, effacer", "Annuler"))
                {
                    ClearAllData();
                }
            }
            GUI.color = Color.white;

            if (GUILayout.Button("🗑️ Effacer Leaderboard (Garder Joueur)"))
            {
                ClearLeaderboard();
            }
        });

        GUILayout.Space(10);

        // ========== SECTION PROFIL ==========
        DrawSection("👨‍💼 Modification du Profil", () =>
        {
            if (GUILayout.Button("🎨 Débloquer Tous les Avatars"))
            {
                UnlockAllAvatars();
            }

            if (GUILayout.Button("🌍 Changer de Pays Aléatoire"))
            {
                ChangeRandomCountry();
            }

            if (GUILayout.Button("✏️ Nom Aléatoire"))
            {
                ChangeRandomName();
            }
        });

        GUILayout.Space(10);

        // ========== SECTION NAVIGATION ==========
        DrawSection("🧭 Navigation Rapide", () =>
        {
            if (GUILayout.Button("🏠 Aller au Menu"))
            {
                LoadScene("MainMenu");
            }

            if (GUILayout.Button("🎮 Aller au Jeu"))
            {
                LoadScene("Game");
            }

            if (GUILayout.Button("🏆 Aller au Leaderboard"))
            {
                LoadScene("Leaderboard");
            }

            if (GUILayout.Button("👤 Aller au Profil"))
            {
                LoadScene("Profile");
            }
        });

        EditorGUILayout.EndScrollView();
    }

    void DrawSection(string title, System.Action content)
    {
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label(title, EditorStyles.boldLabel);
        GUILayout.Space(5);
        content();
        GUILayout.EndVertical();
    }

    // ========== IMPLÉMENTATIONS ==========

    void PrintCurrentPlayerInfo()
    {
        var player = DataService.Instance?.GetCurrentPlayer();
        if (player == null)
        {
            Debug.LogWarning("❌ Aucun joueur actuel trouvé");
            return;
        }

        Debug.Log("========================================");
        Debug.Log("📊 INFORMATIONS DU JOUEUR ACTUEL");
        Debug.Log("========================================");
        Debug.Log($"👤 Nom: {player.playerName}");
        Debug.Log($"🆔 ID: {player.playerId}");
        Debug.Log($"🎯 Score: {player.totalScore}");
        Debug.Log($"📈 Niveau Max: {player.highestLevel}");
        Debug.Log($"🎨 Avatar ID: {player.avatarId}");
        Debug.Log($"🏴 Pays ID: {player.countryId}");
        Debug.Log($"🎮 Parties: {player.gamesWon}/{player.gamesPlayed}");
        Debug.Log($"✅ Précision: {player.GetAccuracy():F1}%");
        Debug.Log($"🏆 Rang: {player.GetRank()}");
        Debug.Log($"📅 Créé: {player.createdDate:dd/MM/yyyy}");
        Debug.Log($"⏰ Dernière connexion: {player.lastPlayedDate:dd/MM/yyyy HH:mm}");
        Debug.Log("========================================\n");
    }

    void CreateNewPlayer()
    {
        PlayerPrefs.DeleteKey("CurrentPlayer");
        DataService.Instance?.LoadAllData();
        Debug.Log("✅ Nouveau joueur créé");
    }

    void GenerateTestPlayers()
    {
        DataService.Instance?.GenerateDummyPlayers(dummyPlayersCount);
        Debug.Log($"✅ {dummyPlayersCount} joueurs fictifs générés");
    }

    void AddScore()
    {
        DataService.Instance?.AddScore(scoreToAdd);
        Debug.Log($"✅ {scoreToAdd} points ajoutés");
        PrintCurrentPlayerInfo();
    }

    void ResetScore()
    {
        var player = DataService.Instance?.GetCurrentPlayer();
        if (player != null)
        {
            player.totalScore = 0;
            DataService.Instance.SaveCurrentPlayer();
            Debug.Log("✅ Score réinitialisé à 0");
        }
    }

    void SetLevel()
    {
        DataService.Instance?.UpdateLevel(levelToSet);
        Debug.Log($"✅ Niveau défini à {levelToSet}");
    }

    void PrintLeaderboard()
    {
        var entries = DataService.Instance?.GetLeaderboard(10);
        if (entries == null || entries.Count == 0)
        {
            Debug.LogWarning("❌ Leaderboard vide");
            return;
        }

        Debug.Log("========================================");
        Debug.Log("🏆 TOP 10 LEADERBOARD");
        Debug.Log("========================================");
        foreach (var entry in entries)
        {
            string star = entry.isLocalPlayer ? "⭐" : "  ";
            Debug.Log($"{star} #{entry.rank} | {entry.playerName} | Score: {entry.score} | Niv: {entry.level}");
        }
        Debug.Log("========================================\n");
    }

    void PrintMyRank()
    {
        int rank = DataService.Instance?.GetCurrentPlayerRank() ?? -1;
        if (rank > 0)
        {
            Debug.Log($"🏆 Votre rang: #{rank}");
        }
        else
        {
            Debug.LogWarning("❌ Non classé");
        }
    }

    void PrintDetailedStats()
    {
        var player = DataService.Instance?.GetCurrentPlayer();
        if (player == null) return;

        Debug.Log("========================================");
        Debug.Log("📊 STATISTIQUES DÉTAILLÉES");
        Debug.Log("========================================");
        Debug.Log($"🎯 Score Total: {player.totalScore}");
        Debug.Log($"📈 Niveau Maximum: {player.highestLevel}");
        Debug.Log($"🎮 Parties Jouées: {player.gamesPlayed}");
        Debug.Log($"🏆 Parties Gagnées: {player.gamesWon}");
        Debug.Log($"📊 Taux de Victoire: {player.GetWinRate():F1}%");
        Debug.Log($"✅ Réponses Correctes: {player.totalCorrectAnswers}");
        Debug.Log($"❌ Réponses Incorrectes: {player.totalWrongAnswers}");
        Debug.Log($"🎯 Précision: {player.GetAccuracy():F1}%");
        Debug.Log($"⏱️ Temps Moyen/Question: {player.averageTimePerQuestion:F1}s");
        Debug.Log($"💔 Vies Perdues: {player.totalLivesLost}");
        Debug.Log("========================================\n");
    }

    void SaveData()
    {
        DataService.Instance?.SaveCurrentPlayer();
        Debug.Log("✅ Données sauvegardées");
    }

    void ReloadData()
    {
        DataService.Instance?.LoadAllData();
        Debug.Log("✅ Données rechargées");
    }

    void ClearAllData()
    {
        DataService.Instance?.ResetAllData();
        PlayerPrefs.DeleteAll();
        Debug.Log("✅ Toutes les données ont été effacées");
    }

    void ClearLeaderboard()
    {
        PlayerPrefs.DeleteKey("AllPlayers");
        DataService.Instance?.LoadAllData();
        Debug.Log("✅ Leaderboard effacé (joueur actuel conservé)");
    }

    void UnlockAllAvatars()
    {
        for (int i = 1; i <= 50; i++)
        {
            PlayerPrefs.SetInt($"Avatar_{i}_Unlocked", 1);
        }
        PlayerPrefs.Save();
        Debug.Log("✅ Tous les avatars débloqués");
    }

    void ChangeRandomCountry()
    {
        int randomCountry = Random.Range(1, 195);
        var player = DataService.Instance?.GetCurrentPlayer();
        if (player != null)
        {
            player.countryId = randomCountry;
            DataService.Instance.SaveCurrentPlayer();
            Debug.Log($"✅ Pays changé: ID {randomCountry}");
        }
    }

    void ChangeRandomName()
    {
        string[] names = { "Alex", "Sophie", "Mohammed", "Maria", "Chen", "Yuki",
                          "Diego", "Emma", "Ivan", "Fatima", "Lucas", "Aisha" };
        string newName = names[Random.Range(0, names.Length)] + Random.Range(100, 999);

        var player = DataService.Instance?.GetCurrentPlayer();
        if (player != null)
        {
            player.playerName = newName;
            DataService.Instance.SaveCurrentPlayer();
            Debug.Log($"✅ Nom changé: {newName}");
        }
    }

    void LoadScene(string sceneName)
    {
        if (Application.isPlaying)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("⚠️ L'application doit être en lecture pour changer de scène");
        }
    }
}

// ========== MENU ITEMS RAPIDES ==========

public static class QuickDebugMenu
{
    [MenuItem("Tools/Game Debug/Quick Actions/Generate 20 Players")]
    static void Quick_Generate20Players()
    {
        DataService.Instance?.GenerateDummyPlayers(20);
        Debug.Log("✅ 20 joueurs générés");
    }

    [MenuItem("Tools/Game Debug/Quick Actions/Add 1000 Score")]
    static void Quick_Add1000Score()
    {
        DataService.Instance?.AddScore(1000);
        Debug.Log("✅ +1000 points");
    }

    [MenuItem("Tools/Game Debug/Quick Actions/Reset All Data")]
    static void Quick_ResetAllData()
    {
        if (EditorUtility.DisplayDialog("Confirmation",
            "Effacer toutes les données?",
            "Oui", "Non"))
        {
            DataService.Instance?.ResetAllData();
            PlayerPrefs.DeleteAll();
            Debug.Log("✅ Données effacées");
        }
    }

    [MenuItem("Tools/Game Debug/Quick Actions/Print Player Info")]
    static void Quick_PrintPlayerInfo()
    {
        var player = DataService.Instance?.GetCurrentPlayer();
        if (player != null)
        {
            Debug.Log($"👤 {player.playerName} | Score: {player.totalScore} | Niveau: {player.highestLevel}");
        }
    }
}
#endif
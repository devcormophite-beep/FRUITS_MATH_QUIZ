using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Service centralisé pour gérer toutes les données du jeu
/// VERSION CORRIGÉE : Synchronise avec PlayerPrefs pour le MainMenu
/// </summary>
public class DataService : MonoBehaviour
{
    private static DataService _instance;
    public static DataService Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("DataService");
                _instance = go.AddComponent<DataService>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private PlayerData currentPlayer;
    private List<PlayerData> allPlayers = new List<PlayerData>();
    private const string CURRENT_PLAYER_KEY = "CurrentPlayer";
    private const string ALL_PLAYERS_KEY = "AllPlayers";

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllData();
    }

    // ========== GESTION DU JOUEUR ACTUEL ==========

    public void LoadAllData()
    {
        LoadCurrentPlayer();
        LoadAllPlayers();
    }

    void LoadCurrentPlayer()
    {
        Debug.Log("========================================");
        Debug.Log("📂 CHARGEMENT DU JOUEUR");
        Debug.Log("========================================");

        if (PlayerPrefs.HasKey(CURRENT_PLAYER_KEY))
        {
            // Charger depuis le JSON
            string json = PlayerPrefs.GetString(CURRENT_PLAYER_KEY);
            currentPlayer = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log($"✓ Joueur chargé depuis JSON: {currentPlayer.playerName}");
            Debug.Log($"  Score: {currentPlayer.totalScore}");
            Debug.Log($"  Niveau: {currentPlayer.highestLevel}");
        }
        else
        {
            // ✅ NOUVEAU: Créer depuis les PlayerPrefs existants
            string name = PlayerPrefs.GetString("PlayerName", "Joueur");
            int avatarId = PlayerPrefs.GetInt("SelectedAvatarId", 1);
            int countryId = PlayerPrefs.GetInt("SelectedCountryId", 1);
            int score = PlayerPrefs.GetInt("TotalScore", 0);
            int level = PlayerPrefs.GetInt("HighestLevel", 1);
            int played = PlayerPrefs.GetInt("GamesPlayed", 0);
            int won = PlayerPrefs.GetInt("GamesWon", 0);

            currentPlayer = new PlayerData(name, avatarId, countryId);
            currentPlayer.totalScore = score;
            currentPlayer.highestLevel = level;
            currentPlayer.gamesPlayed = played;
            currentPlayer.gamesWon = won;

            Debug.Log("✓ Nouveau joueur créé avec données existantes:");
            Debug.Log($"  Nom: {name}");
            Debug.Log($"  Avatar: {avatarId}");
            Debug.Log($"  Pays: {countryId}");
            Debug.Log($"  Score: {score}");

            SaveCurrentPlayer();
        }

        Debug.Log("========================================\n");
    }

    public void SaveCurrentPlayer()
    {
        if (currentPlayer == null)
        {
            Debug.LogWarning("⚠️ Tentative de sauvegarde avec currentPlayer null!");
            return;
        }

        currentPlayer.lastPlayedDate = DateTime.Now;

        // 1. Sauvegarder le JSON (pour le système DataService)
        string json = JsonUtility.ToJson(currentPlayer);
        PlayerPrefs.SetString(CURRENT_PLAYER_KEY, json);

        // ✅ 2. NOUVEAU: Synchroniser avec PlayerPrefs individuels (pour MainMenu)
        PlayerPrefs.SetString("PlayerName", currentPlayer.playerName);
        PlayerPrefs.SetInt("SelectedAvatarId", currentPlayer.avatarId);
        PlayerPrefs.SetInt("SelectedCountryId", currentPlayer.countryId);
        PlayerPrefs.SetInt("TotalScore", currentPlayer.totalScore);
        PlayerPrefs.SetInt("HighestLevel", currentPlayer.highestLevel);
        PlayerPrefs.SetInt("GamesPlayed", currentPlayer.gamesPlayed);
        PlayerPrefs.SetInt("GamesWon", currentPlayer.gamesWon);

        PlayerPrefs.Save();

        // Mettre à jour aussi dans la liste globale
        UpdatePlayerInGlobalList(currentPlayer);

        Debug.Log("========================================");
        Debug.Log("💾 SAUVEGARDE SYNCHRONISÉE");
        Debug.Log("========================================");
        Debug.Log($"Joueur: {currentPlayer.playerName}");
        Debug.Log($"Score: {currentPlayer.totalScore}");
        Debug.Log($"Niveau: {currentPlayer.highestLevel}");
        Debug.Log($"Parties: {currentPlayer.gamesPlayed} (Gagnées: {currentPlayer.gamesWon})");
        Debug.Log($"WinRate: {currentPlayer.GetWinRate():F1}%");
        Debug.Log("✅ JSON + PlayerPrefs individuels synchronisés");
        Debug.Log("========================================\n");
    }

    public PlayerData GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public void UpdateCurrentPlayerProfile(string name, int avatarId, int countryId)
    {
        if (currentPlayer == null)
        {
            Debug.LogWarning("⚠️ currentPlayer est null dans UpdateCurrentPlayerProfile");
            return;
        }

        currentPlayer.playerName = name;
        currentPlayer.avatarId = avatarId;
        currentPlayer.countryId = countryId;
        SaveCurrentPlayer();

        Debug.Log($"✅ Profil mis à jour: {name}, Avatar: {avatarId}, Pays: {countryId}");
    }

    public void AddScore(int points)
    {
        if (currentPlayer == null)
        {
            Debug.LogWarning("⚠️ currentPlayer est null dans AddScore");
            return;
        }

        currentPlayer.totalScore += points;
        SaveCurrentPlayer();

        Debug.Log($"✅ Score ajouté: +{points} → Total: {currentPlayer.totalScore}");
    }

    public void UpdateLevel(int level)
    {
        if (currentPlayer == null)
        {
            Debug.LogWarning("⚠️ currentPlayer est null dans UpdateLevel");
            return;
        }

        if (level > currentPlayer.highestLevel)
        {
            currentPlayer.highestLevel = level;
            SaveCurrentPlayer();
            Debug.Log($"✅ Nouveau niveau max: {level}");
        }
    }

    public void RecordGameResult(bool won, int correctAnswers, int wrongAnswers, float avgTime)
    {
        if (currentPlayer == null)
        {
            Debug.LogWarning("⚠️ currentPlayer est null dans RecordGameResult");
            return;
        }

        currentPlayer.gamesPlayed++;
        if (won) currentPlayer.gamesWon++;
        currentPlayer.totalCorrectAnswers += correctAnswers;
        currentPlayer.totalWrongAnswers += wrongAnswers;

        // Moyenne pondérée du temps
        float totalQuestions = currentPlayer.totalCorrectAnswers + currentPlayer.totalWrongAnswers;
        if (totalQuestions > 0)
        {
            currentPlayer.averageTimePerQuestion =
                (currentPlayer.averageTimePerQuestion * (totalQuestions - correctAnswers - wrongAnswers) + avgTime * (correctAnswers + wrongAnswers))
                / totalQuestions;
        }

        SaveCurrentPlayer();

        Debug.Log("========================================");
        Debug.Log("📊 RÉSULTAT DE LA PARTIE");
        Debug.Log("========================================");
        Debug.Log($"Résultat: {(won ? "✅ VICTOIRE" : "❌ DÉFAITE")}");
        Debug.Log($"Bonnes réponses: {correctAnswers}");
        Debug.Log($"Mauvaises réponses: {wrongAnswers}");
        Debug.Log($"Temps moyen: {avgTime:F1}s");
        Debug.Log($"Total parties: {currentPlayer.gamesPlayed}");
        Debug.Log($"WinRate: {currentPlayer.GetWinRate():F1}%");
        Debug.Log("========================================\n");
    }

    // ========== GESTION DU LEADERBOARD GLOBAL ==========

    void LoadAllPlayers()
    {
        if (PlayerPrefs.HasKey(ALL_PLAYERS_KEY))
        {
            string json = PlayerPrefs.GetString(ALL_PLAYERS_KEY);
            PlayerDataList list = JsonUtility.FromJson<PlayerDataList>(json);
            allPlayers = list.players.ToList();
            Debug.Log($"✓ {allPlayers.Count} joueurs chargés dans le leaderboard");
        }
        else
        {
            allPlayers = new List<PlayerData>();
            Debug.Log("✓ Nouveau leaderboard créé");
        }

        // Ajouter le joueur actuel s'il n'existe pas
        if (currentPlayer != null)
        {
            UpdatePlayerInGlobalList(currentPlayer);
        }
    }

    void UpdatePlayerInGlobalList(PlayerData player)
    {
        if (player == null) return;

        // Chercher si le joueur existe déjà
        int index = allPlayers.FindIndex(p => p.playerId == player.playerId);

        if (index >= 0)
        {
            // Mettre à jour
            allPlayers[index] = player;
        }
        else
        {
            // Ajouter
            allPlayers.Add(player);
        }

        SaveAllPlayers();
    }

    void SaveAllPlayers()
    {
        PlayerDataList list = new PlayerDataList
        {
            players = allPlayers.ToArray()
        };

        string json = JsonUtility.ToJson(list);
        PlayerPrefs.SetString(ALL_PLAYERS_KEY, json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Retourne les entrées du leaderboard triées
    /// </summary>
    public List<LeaderboardEntry> GetLeaderboard(int maxEntries = 100)
    {
        List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

        // Convertir les PlayerData en LeaderboardEntry
        foreach (var player in allPlayers)
        {
            LeaderboardEntry entry = new LeaderboardEntry(player);

            // Marquer le joueur local
            if (currentPlayer != null && player.playerId == currentPlayer.playerId)
            {
                entry.isLocalPlayer = true;
            }

            entries.Add(entry);
        }

        // Trier par score décroissant
        entries.Sort();

        // Assigner les rangs
        for (int i = 0; i < entries.Count; i++)
        {
            entries[i].rank = i + 1;
        }

        // Limiter le nombre d'entrées
        if (entries.Count > maxEntries)
        {
            entries = entries.Take(maxEntries).ToList();
        }

        return entries;
    }

    /// <summary>
    /// Retourne le leaderboard filtré par pays
    /// </summary>
    public List<LeaderboardEntry> GetLeaderboardByCountry(int countryId, int maxEntries = 50)
    {
        var allEntries = GetLeaderboard(1000);
        var filteredEntries = allEntries.Where(e => e.countryId == countryId).ToList();

        // Réassigner les rangs
        for (int i = 0; i < filteredEntries.Count; i++)
        {
            filteredEntries[i].rank = i + 1;
        }

        return filteredEntries.Take(maxEntries).ToList();
    }

    /// <summary>
    /// Retourne le rang du joueur actuel
    /// </summary>
    public int GetCurrentPlayerRank()
    {
        if (currentPlayer == null) return -1;

        var leaderboard = GetLeaderboard(1000);
        var entry = leaderboard.Find(e => e.playerId == currentPlayer.playerId);
        return entry != null ? entry.rank : -1;
    }

    /// <summary>
    /// Ajoute des joueurs fictifs pour le développement
    /// </summary>
    public void GenerateDummyPlayers(int count = 20)
    {
        string[] names = { "Alex", "Sophie", "Mohammed", "Maria", "Chen", "Yuki",
                          "Diego", "Emma", "Ivan", "Fatima", "Lucas", "Aisha",
                          "Marco", "Nina", "Omar", "Léa", "Raj", "Ana", "Kim", "Tom" };

        for (int i = 0; i < count; i++)
        {
            PlayerData dummy = new PlayerData
            {
                playerId = Guid.NewGuid().ToString(),
                playerName = names[i % names.Length] + UnityEngine.Random.Range(100, 999),
                avatarId = UnityEngine.Random.Range(1, 15),
                countryId = UnityEngine.Random.Range(1, 195),
                totalScore = UnityEngine.Random.Range(100, 10000),
                highestLevel = UnityEngine.Random.Range(1, 50),
                gamesPlayed = UnityEngine.Random.Range(5, 100),
                gamesWon = UnityEngine.Random.Range(2, 50),
                totalCorrectAnswers = UnityEngine.Random.Range(50, 500),
                totalWrongAnswers = UnityEngine.Random.Range(10, 200),
                createdDate = DateTime.Now.AddDays(-UnityEngine.Random.Range(1, 365))
            };

            allPlayers.Add(dummy);
        }

        SaveAllPlayers();
        Debug.Log($"✓ {count} joueurs fictifs ajoutés au leaderboard");
    }

    /// <summary>
    /// Réinitialise toutes les données (pour développement)
    /// </summary>
    public void ResetAllData()
    {
        Debug.LogWarning("========================================");
        Debug.LogWarning("🗑️ RÉINITIALISATION COMPLÈTE");
        Debug.LogWarning("========================================");

        PlayerPrefs.DeleteKey(CURRENT_PLAYER_KEY);
        PlayerPrefs.DeleteKey(ALL_PLAYERS_KEY);

        // ✅ NOUVEAU: Supprimer aussi les PlayerPrefs individuels
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.DeleteKey("SelectedAvatarId");
        PlayerPrefs.DeleteKey("SelectedCountryId");
        PlayerPrefs.DeleteKey("TotalScore");
        PlayerPrefs.DeleteKey("HighestLevel");
        PlayerPrefs.DeleteKey("GamesPlayed");
        PlayerPrefs.DeleteKey("GamesWon");

        PlayerPrefs.Save();

        allPlayers.Clear();
        currentPlayer = null;

        Debug.LogWarning("✅ Toutes les données ont été réinitialisées");
        Debug.LogWarning("========================================\n");
    }

    // ========== MÉTHODES DE DEBUG ==========

    public void PrintCurrentPlayerInfo()
    {
        if (currentPlayer == null)
        {
            Debug.Log("❌ Aucun joueur actuel");
            return;
        }

        Debug.Log("========================================");
        Debug.Log("👤 INFORMATIONS DU JOUEUR ACTUEL");
        Debug.Log("========================================");
        Debug.Log($"ID: {currentPlayer.playerId}");
        Debug.Log($"Nom: {currentPlayer.playerName}");
        Debug.Log($"Avatar: {currentPlayer.avatarId}");
        Debug.Log($"Pays: {currentPlayer.countryId}");
        Debug.Log($"Score: {currentPlayer.totalScore}");
        Debug.Log($"Niveau max: {currentPlayer.highestLevel}");
        Debug.Log($"Parties jouées: {currentPlayer.gamesPlayed}");
        Debug.Log($"Parties gagnées: {currentPlayer.gamesWon}");
        Debug.Log($"WinRate: {currentPlayer.GetWinRate():F1}%");
        Debug.Log($"Précision: {currentPlayer.GetAccuracy():F1}%");
        Debug.Log($"Créé le: {currentPlayer.createdDate}");
        Debug.Log($"Dernière partie: {currentPlayer.lastPlayedDate}");
        Debug.Log("========================================\n");
    }

    [ContextMenu("Print Current Player")]
    public void DebugPrintPlayer()
    {
        PrintCurrentPlayerInfo();
    }

    [ContextMenu("Add 1000 Score")]
    public void DebugAddScore()
    {
        AddScore(1000);
        PrintCurrentPlayerInfo();
    }

    [ContextMenu("Generate 20 Dummy Players")]
    public void DebugGenerateDummies()
    {
        GenerateDummyPlayers(20);
    }
}

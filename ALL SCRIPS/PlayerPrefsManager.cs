using UnityEngine;
using System;

/// <summary>
/// Gestionnaire centralisé pour toutes les données du joueur
/// Corrige les incohérences entre PlayerPrefs et le leaderboard
/// </summary>
public class PlayerPrefsManager : MonoBehaviour
{
    private static PlayerPrefsManager _instance;
    public static PlayerPrefsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("PlayerPrefsManager");
                _instance = go.AddComponent<PlayerPrefsManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // ========== CONSTANTES DES CLÉS ==========
    private const string KEY_PLAYER_NAME = "PlayerName";
    private const string KEY_AVATAR_ID = "SelectedAvatarId";
    private const string KEY_COUNTRY_ID = "SelectedCountryId";
    private const string KEY_COUNTRY_NAME = "SelectedCountryName";
    private const string KEY_TOTAL_SCORE = "TotalScore";
    private const string KEY_HIGHEST_LEVEL = "HighestLevel";
    private const string KEY_GAMES_PLAYED = "GamesPlayed";
    private const string KEY_GAMES_WON = "GamesWon";
    private const string KEY_LAST_SYNC = "LastSyncTimestamp";

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

    // ========== GETTERS SÉCURISÉS ==========

    public string GetPlayerName()
    {
        return PlayerPrefs.GetString(KEY_PLAYER_NAME, "Joueur");
    }

    public int GetAvatarId()
    {
        return PlayerPrefs.GetInt(KEY_AVATAR_ID, -1);
    }

    public int GetCountryId()
    {
        return PlayerPrefs.GetInt(KEY_COUNTRY_ID, -1);
    }

    public string GetCountryName()
    {
        return PlayerPrefs.GetString(KEY_COUNTRY_NAME, "");
    }

    public int GetTotalScore()
    {
        return PlayerPrefs.GetInt(KEY_TOTAL_SCORE, 0);
    }

    public int GetHighestLevel()
    {
        return PlayerPrefs.GetInt(KEY_HIGHEST_LEVEL, 0);
    }

    public int GetGamesPlayed()
    {
        return PlayerPrefs.GetInt(KEY_GAMES_PLAYED, 0);
    }

    public int GetGamesWon()
    {
        return PlayerPrefs.GetInt(KEY_GAMES_WON, 0);
    }

    public float GetWinRate()
    {
        int played = GetGamesPlayed();
        if (played == 0) return 0f;
        return ((float)GetGamesWon() / played) * 100f;
    }

    // ========== SETTERS AVEC AUTO-SAVE ==========

    public void SetPlayerName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("⚠️ Tentative d'enregistrer un nom vide !");
            return;
        }
        PlayerPrefs.SetString(KEY_PLAYER_NAME, name);
        SaveAndLog($"Nom du joueur mis à jour : {name}");
    }

    public void SetAvatarId(int avatarId)
    {
        PlayerPrefs.SetInt(KEY_AVATAR_ID, avatarId);
        SaveAndLog($"Avatar mis à jour : ID {avatarId}");
    }

    public void SetCountryId(int countryId)
    {
        PlayerPrefs.SetInt(KEY_COUNTRY_ID, countryId);
        SaveAndLog($"Pays mis à jour : ID {countryId}");
    }

    public void SetCountryName(string countryName)
    {
        PlayerPrefs.SetString(KEY_COUNTRY_NAME, countryName);
        SaveAndLog($"Nom du pays mis à jour : {countryName}");
    }

    public void SetTotalScore(int score)
    {
        PlayerPrefs.SetInt(KEY_TOTAL_SCORE, score);
        SaveAndLog($"Score total mis à jour : {score}");
    }

    public void SetHighestLevel(int level)
    {
        int currentHighest = GetHighestLevel();
        if (level > currentHighest)
        {
            PlayerPrefs.SetInt(KEY_HIGHEST_LEVEL, level);
            SaveAndLog($"Nouveau niveau maximum : {level}");
        }
    }

    public void AddScore(int scoreToAdd)
    {
        int newScore = GetTotalScore() + scoreToAdd;
        SetTotalScore(newScore);
    }

    public void IncrementGamesPlayed()
    {
        int newValue = GetGamesPlayed() + 1;
        PlayerPrefs.SetInt(KEY_GAMES_PLAYED, newValue);
        SaveAndLog($"Parties jouées : {newValue}");
    }

    public void IncrementGamesWon()
    {
        int newValue = GetGamesWon() + 1;
        PlayerPrefs.SetInt(KEY_GAMES_WON, newValue);
        SaveAndLog($"Parties gagnées : {newValue}");
    }

    // ========== SYNCHRONISATION AVEC LOOTLOCKER ==========

    public PlayerData GetPlayerDataForLeaderboard()
    {
        PlayerData data = new PlayerData
        {
            playerId = SystemInfo.deviceUniqueIdentifier,
            playerName = GetPlayerName(),
            avatarId = GetAvatarId(),
            countryId = GetCountryId(),
            totalScore = GetTotalScore(),
            highestLevel = GetHighestLevel(),
            gamesPlayed = GetGamesPlayed(),
            gamesWon = GetGamesWon(),
            lastPlayedDate = DateTime.Now
        };

        Debug.Log("📊 Données préparées pour le leaderboard :");
        Debug.Log($"  • Nom: {data.playerName}");
        Debug.Log($"  • Avatar ID: {data.avatarId}");
        Debug.Log($"  • Pays ID: {data.countryId}");
        Debug.Log($"  • Score: {data.totalScore}");
        Debug.Log($"  • Niveau: {data.highestLevel}");
        Debug.Log($"  • WinRate: {data.GetWinRate():F1}%");

        return data;
    }

    // ========== MÉTHODE DE SAUVEGARDE ATOMIQUE ==========

    public void SaveCompleteProfile(string name, int avatarId, int countryId, string countryName)
    {
        Debug.Log("💾 SAUVEGARDE COMPLÈTE DU PROFIL");

        PlayerPrefs.SetString(KEY_PLAYER_NAME, name);
        PlayerPrefs.SetInt(KEY_AVATAR_ID, avatarId);
        PlayerPrefs.SetInt(KEY_COUNTRY_ID, countryId);
        PlayerPrefs.SetString(KEY_COUNTRY_NAME, countryName);

        PlayerPrefs.Save();

        Debug.Log($"✅ Profil sauvegardé :");
        Debug.Log($"  • Nom: {name}");
        Debug.Log($"  • Avatar: {avatarId}");
        Debug.Log($"  • Pays: {countryName} (ID: {countryId})");
    }

    // ========== DEBUG & LOGS ==========

    private void SaveAndLog(string message)
    {
        PlayerPrefs.Save();
        Debug.Log($"💾 {message}");

        // Timestamp de la dernière modification
        PlayerPrefs.SetString(KEY_LAST_SYNC, DateTime.Now.ToString("o"));
    }

    public void PrintAllPlayerPrefs()
    {
        Debug.Log("========================================");
        Debug.Log("📋 ÉTAT ACTUEL DES PLAYERPREFS");
        Debug.Log("========================================");
        Debug.Log($"Nom: {GetPlayerName()}");
        Debug.Log($"Avatar ID: {GetAvatarId()}");
        Debug.Log($"Pays ID: {GetCountryId()}");
        Debug.Log($"Pays Nom: {GetCountryName()}");
        Debug.Log($"Score Total: {GetTotalScore()}");
        Debug.Log($"Niveau Max: {GetHighestLevel()}");
        Debug.Log($"Parties jouées: {GetGamesPlayed()}");
        Debug.Log($"Parties gagnées: {GetGamesWon()}");
        Debug.Log($"WinRate: {GetWinRate():F1}%");
        Debug.Log($"Dernière sync: {PlayerPrefs.GetString(KEY_LAST_SYNC, "Jamais")}");
        Debug.Log("========================================\n");
    }

    public void ResetAllData()
    {
        Debug.LogWarning("🗑️ RÉINITIALISATION DE TOUTES LES DONNÉES !");
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    // ========== VALIDATION DES DONNÉES ==========

    public bool IsProfileComplete()
    {
        bool hasName = !string.IsNullOrEmpty(GetPlayerName()) && GetPlayerName() != "Joueur";
        bool hasAvatar = GetAvatarId() > 0;
        bool hasCountry = GetCountryId() > 0;

        Debug.Log($"Profil complet ? {hasName && hasAvatar && hasCountry}");
        Debug.Log($"  • Nom valide: {hasName}");
        Debug.Log($"  • Avatar sélectionné: {hasAvatar}");
        Debug.Log($"  • Pays sélectionné: {hasCountry}");

        return hasName && hasAvatar && hasCountry;
    }
}
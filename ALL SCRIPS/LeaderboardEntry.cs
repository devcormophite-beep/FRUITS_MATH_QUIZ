using System;
using UnityEngine;

/// <summary>
/// Représente une entrée dans le leaderboard
/// Version simplifiée de PlayerData pour l'affichage
/// </summary>
[Serializable]
public class LeaderboardEntry : IComparable<LeaderboardEntry>
{
    public int rank;                 // Position dans le classement
    public string playerId;          // ID unique
    public string playerName;        // Pseudo
    public int avatarId;            // ID avatar
    public int countryId;           // ID pays
    public int score;               // Score total
    public int level;               // Niveau atteint
    public int gamesWon;            // Parties gagnées
    public float winRate;           // % victoires
    public bool isLocalPlayer;      // Est-ce le joueur actuel ?

    public LeaderboardEntry()
    {
    }

    public LeaderboardEntry(PlayerData playerData)
    {
        playerId = playerData.playerId;
        playerName = playerData.playerName;
        avatarId = playerData.avatarId;
        countryId = playerData.countryId;
        score = playerData.totalScore;
        level = playerData.highestLevel;
        gamesWon = playerData.gamesWon;
        winRate = playerData.GetWinRate();
        isLocalPlayer = false;
    }

    /// <summary>
    /// Compare par score (décroissant)
    /// </summary>
    public int CompareTo(LeaderboardEntry other)
    {
        if (other == null) return 1;

        // Trier par score décroissant
        int scoreComparison = other.score.CompareTo(this.score);
        if (scoreComparison != 0) return scoreComparison;

        // En cas d'égalité, trier par niveau
        int levelComparison = other.level.CompareTo(this.level);
        if (levelComparison != 0) return levelComparison;

        // Sinon par winRate
        return other.winRate.CompareTo(this.winRate);
    }
}

/// <summary>
/// Wrapper pour la sérialisation JSON
/// </summary>
[Serializable]
public class LeaderboardData
{
    public LeaderboardEntry[] entries;
    public DateTime lastUpdated;

    public LeaderboardData()
    {
        entries = new LeaderboardEntry[0];
        lastUpdated = DateTime.Now;
    }
}
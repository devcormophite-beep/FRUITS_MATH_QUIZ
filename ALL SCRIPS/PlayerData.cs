using System;
using UnityEngine;

/// <summary>
/// Classe contenant toutes les données d'un joueur
/// Utilisée pour la sauvegarde et le leaderboard
/// </summary>
[Serializable]
public class PlayerData
{
    public string playerId;          // ID unique du joueur
    public string playerName;         // Pseudo
    public int avatarId;             // ID de l'avatar
    public int countryId;            // ID du pays
    public int totalScore;           // Score total
    public int highestLevel;         // Niveau maximum atteint
    public int gamesPlayed;          // Nombre de parties jouées
    public int gamesWon;             // Nombre de parties gagnées
    public DateTime lastPlayedDate;  // Dernière connexion
    public DateTime createdDate;     // Date de création du profil

    // Stats détaillées
    public int totalCorrectAnswers;
    public int totalWrongAnswers;
    public float averageTimePerQuestion;
    public int totalLivesLost;

    public PlayerData()
    {
        playerId = Guid.NewGuid().ToString();
        playerName = "Joueur";
        avatarId = -1;
        countryId = -1;
        totalScore = 0;
        highestLevel = 0;
        gamesPlayed = 0;
        gamesWon = 0;
        createdDate = DateTime.Now;
        lastPlayedDate = DateTime.Now;
    }

    public PlayerData(string name, int avatar, int country)
    {
        playerId = Guid.NewGuid().ToString();
        playerName = name;
        avatarId = avatar;
        countryId = country;
        totalScore = 0;
        highestLevel = 0;
        gamesPlayed = 0;
        gamesWon = 0;
        createdDate = DateTime.Now;
        lastPlayedDate = DateTime.Now;
    }

    /// <summary>
    /// Calcule le ratio victoire/défaite en pourcentage
    /// </summary>
    public float GetWinRate()
    {
        if (gamesPlayed == 0) return 0f;
        return (float)gamesWon / gamesPlayed * 100f;
    }

    /// <summary>
    /// Calcule le ratio réponses correctes
    /// </summary>
    public float GetAccuracy()
    {
        int totalAnswers = totalCorrectAnswers + totalWrongAnswers;
        if (totalAnswers == 0) return 0f;
        return (float)totalCorrectAnswers / totalAnswers * 100f;
    }

    /// <summary>
    /// Retourne un rang basé sur le score
    /// </summary>
    public string GetRank()
    {
        if (totalScore >= 10000) return "Légende";
        if (totalScore >= 5000) return "Expert";
        if (totalScore >= 2500) return "Pro";
        if (totalScore >= 1000) return "Avancé";
        if (totalScore >= 500) return "Intermédiaire";
        return "Débutant";
    }
}

/// <summary>
/// Wrapper pour la sérialisation JSON d'une liste de joueurs
/// </summary>
[Serializable]
public class PlayerDataList
{
    public PlayerData[] players;
}
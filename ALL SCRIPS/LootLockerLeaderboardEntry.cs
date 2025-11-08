using System;

/// <summary>
/// Structure de données unifiée pour les entrées du leaderboard LootLocker
/// Compatible avec LeaderboardEntryUI
/// </summary>
[Serializable]
public class LootLockerLeaderboardEntry
{
    public int rank;
    public string playerId;
    public string playerName;
    public int score;
    public int avatarId;
    public int countryId;
    public int level;
    public bool isLocalPlayer;

    public LootLockerLeaderboardEntry()
    {
        rank = 0;
        playerId = "";
        playerName = "Unknown";
        score = 0;
        avatarId = -1;
        countryId = -1;
        level = 0;
        isLocalPlayer = false;
    }

    public LootLockerLeaderboardEntry(LeaderboardEntry entry)
    {
        rank = entry.rank;
        playerId = entry.playerId;
        playerName = entry.playerName;
        score = entry.score;
        avatarId = entry.avatarId;
        countryId = entry.countryId;
        level = entry.level;
        isLocalPlayer = entry.isLocalPlayer;
    }

    /// <summary>
    /// Convertit vers LeaderboardEntry pour compatibilité
    /// </summary>
    public LeaderboardEntry ToLeaderboardEntry()
    {
        return new LeaderboardEntry
        {
            rank = this.rank,
            playerId = this.playerId,
            playerName = this.playerName,
            avatarId = this.avatarId,
            countryId = this.countryId,
            score = this.score,
            level = this.level,
            isLocalPlayer = this.isLocalPlayer
        };
    }
}
using UnityEngine;
using LootLocker.Requests;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Service d'intégration avec LootLocker - VERSION CORRIGÉE
/// Utilise PlayerPrefsManager pour garantir la cohérence des données
/// </summary>
public class LootLockerService : MonoBehaviour
{
    private static LootLockerService _instance;
    public static LootLockerService Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("LootLockerService");
                _instance = go.AddComponent<LootLockerService>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [Header("Configuration")]
    public string leaderboardKey = "global_leaderboard";
    public bool useLocalFallback = true;

    [Header("Status")]
    public bool isAuthenticated = false;
    public string currentPlayerId = "";
    public string currentPlayerName = "";

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

    void Start()
    {
        StartCoroutine(AuthenticatePlayer());
    }

    // ========== AUTHENTIFICATION ==========

    public IEnumerator AuthenticatePlayer()
    {
        Debug.Log("🔐 Tentative de connexion à LootLocker...");
        bool authCompleted = false;

        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("✅ Authentification LootLocker réussie!");
                isAuthenticated = true;
                currentPlayerId = response.player_id.ToString();

                // CORRECTION : Utiliser PlayerPrefsManager au lieu de DataService
                currentPlayerName = PlayerPrefsManager.Instance.GetPlayerName();
                SetPlayerName(currentPlayerName);

                // Afficher les données qui seront envoyées
                PlayerPrefsManager.Instance.PrintAllPlayerPrefs();
            }
            else
            {
                Debug.LogError($"❌ Échec authentification LootLocker: {response.errorData?.message ?? "Erreur inconnue"}");
                isAuthenticated = false;
            }
            authCompleted = true;
        });

        yield return new WaitUntil(() => authCompleted);
    }

    public void SetPlayerName(string playerName)
    {
        if (!isAuthenticated) return;

        LootLockerSDKManager.SetPlayerName(playerName, (response) =>
        {
            if (response.success)
            {
                currentPlayerName = playerName;
                Debug.Log($"✅ Nom du joueur défini sur LootLocker: {playerName}");
            }
            else
            {
                Debug.LogError($"❌ Erreur lors de la définition du nom: {response.errorData?.message ?? "Erreur inconnue"}");
            }
        });
    }

    // ========== SOUMISSION DU SCORE AVEC MÉTADONNÉES ==========

    public void SubmitScore(int score, System.Action<bool> onComplete = null)
    {
        if (!isAuthenticated)
        {
            Debug.LogWarning("⚠️ Non authentifié, le score n'a pas été soumis à LootLocker.");
            onComplete?.Invoke(false);
            return;
        }

        Debug.Log("📤 PRÉPARATION DE L'ENVOI AU LEADERBOARD");

        // CORRECTION : Créer les métadonnées depuis PlayerPrefsManager
        string metadataJson = CreateMetadataJson();

        Debug.Log($"📊 Métadonnées à envoyer : {metadataJson}");

        LootLockerSDKManager.SubmitScore(
            currentPlayerId,
            score,
            leaderboardKey,
            metadataJson,
            (response) =>
            {
                if (response.success)
                {
                    Debug.Log($"✅ Score {score} soumis avec succès avec les métadonnées !");
                    onComplete?.Invoke(true);
                }
                else
                {
                    Debug.LogError($"❌ Erreur soumission score: {response.errorData?.message ?? "Erreur inconnue"}");
                    onComplete?.Invoke(false);
                }
            }
        );
    }

    // ========== CRÉATION DES MÉTADONNÉES CORRIGÉE ==========

    private string CreateMetadataJson()
    {
        // CORRECTION MAJEURE : Utiliser PlayerPrefsManager au lieu de DataService
        var prefsManager = PlayerPrefsManager.Instance;

        int level = prefsManager.GetHighestLevel();
        int avatarId = prefsManager.GetAvatarId();
        int countryId = prefsManager.GetCountryId();
        float winRate = prefsManager.GetWinRate();

        // Validation des données
        if (avatarId <= 0)
        {
            Debug.LogWarning("⚠️ Avatar ID invalide ! Vérifiez que l'avatar a été sélectionné.");
        }
        if (countryId <= 0)
        {
            Debug.LogWarning("⚠️ Country ID invalide ! Vérifiez que le pays a été sélectionné.");
        }

        LeaderboardEntryMetadata metadata = new LeaderboardEntryMetadata
        {
            level = level,
            avatar_id = avatarId,
            country_id = countryId,
            win_rate = winRate
        };

        string json = JsonUtility.ToJson(metadata);

        Debug.Log("🔍 MÉTADONNÉES CRÉÉES :");
        Debug.Log($"  • Level: {level}");
        Debug.Log($"  • Avatar ID: {avatarId}");
        Debug.Log($"  • Country ID: {countryId}");
        Debug.Log($"  • Win Rate: {winRate:F1}%");
        Debug.Log($"  • JSON: {json}");

        return json;
    }

    // ========== RÉCUPÉRATION DU LEADERBOARD ==========

    public void GetLeaderboard(int count, System.Action<List<LootLockerLeaderboardEntry>> onComplete)
    {
        if (!isAuthenticated)
        {
            Debug.LogWarning("⚠️ Non authentifié, chargement du leaderboard local");
            LoadLocalLeaderboard(count, onComplete);
            return;
        }

        Debug.Log($"📥 Récupération du top {count} du leaderboard...");

        LootLockerSDKManager.GetScoreList(leaderboardKey, count, 0, (response) =>
            ProcessLeaderboardResponse(response, count, onComplete));
    }

    public void GetLeaderboardAroundPlayer(int count, System.Action<List<LootLockerLeaderboardEntry>> onComplete)
    {
        if (!isAuthenticated)
        {
            LoadLocalLeaderboard(count, onComplete);
            return;
        }

        LootLockerSDKManager.GetMemberRank(leaderboardKey, currentPlayerId, (rankResponse) =>
        {
            if (rankResponse.success && rankResponse.rank > 0)
            {
                int startFrom = Mathf.Max(0, rankResponse.rank - (count / 2));
                Debug.Log($"📥 Récupération du leaderboard autour du rang {rankResponse.rank}...");

                LootLockerSDKManager.GetScoreList(leaderboardKey, count, startFrom, (scoreResponse) =>
                    ProcessLeaderboardResponse(scoreResponse, count, onComplete));
            }
            else
            {
                Debug.Log("⚠️ Joueur non classé, affichage du top du leaderboard");
                GetLeaderboard(count, onComplete);
            }
        });
    }

    // ========== TRAITEMENT DE LA RÉPONSE ==========

    private void ProcessLeaderboardResponse(
        LootLockerGetScoreListResponse response,
        int count,
        System.Action<List<LootLockerLeaderboardEntry>> onComplete)
    {
        if (response != null && response.success)
        {
            List<LootLockerLeaderboardEntry> entries = new List<LootLockerLeaderboardEntry>();

            Debug.Log($"📊 Traitement de {response.items.Length} entrées du leaderboard...");

            foreach (var item in response.items)
            {
                if (item == null || item.player == null) continue;

                var entry = new LootLockerLeaderboardEntry
                {
                    rank = item.rank,
                    playerId = item.player.id.ToString(),
                    playerName = item.player.name ?? $"Player {item.player.id}",
                    score = item.score,
                    isLocalPlayer = item.player.id.ToString() == currentPlayerId,
                    // Valeurs par défaut
                    avatarId = -1,
                    countryId = -1,
                    level = 0
                };

                // Parser les métadonnées JSON
                if (!string.IsNullOrEmpty(item.metadata))
                {
                    try
                    {
                        LeaderboardEntryMetadata metadata = JsonUtility.FromJson<LeaderboardEntryMetadata>(item.metadata);

                        entry.level = metadata.level;
                        entry.avatarId = metadata.avatar_id;
                        entry.countryId = metadata.country_id;

                        Debug.Log($"  ✅ {entry.playerName} - Rank {entry.rank} - Avatar: {entry.avatarId}, Country: {entry.countryId}");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"  ⚠️ Métadonnées invalides pour {entry.playerName}: {ex.Message}");
                        Debug.LogWarning($"  JSON reçu: {item.metadata}");
                    }
                }
                else
                {
                    Debug.LogWarning($"  ⚠️ {entry.playerName} n'a pas de métadonnées");
                }

                entries.Add(entry);
            }

            Debug.Log($"✅ {entries.Count} entrées traitées avec succès");
            onComplete?.Invoke(entries);
        }
        else
        {
            Debug.LogError($"❌ Erreur récupération leaderboard: {response?.errorData?.message ?? "Erreur inconnue"}");
            LoadLocalLeaderboard(count, onComplete);
        }
    }

    // ========== LEADERBOARD LOCAL (FALLBACK) ==========

    void LoadLocalLeaderboard(int count, System.Action<List<LootLockerLeaderboardEntry>> onComplete)
    {
        Debug.Log("📊 Chargement du leaderboard local (fallback)");

        // Créer une entrée pour le joueur local
        var localEntry = new LootLockerLeaderboardEntry
        {
            rank = 1,
            playerId = SystemInfo.deviceUniqueIdentifier,
            playerName = PlayerPrefsManager.Instance.GetPlayerName(),
            score = PlayerPrefsManager.Instance.GetTotalScore(),
            avatarId = PlayerPrefsManager.Instance.GetAvatarId(),
            countryId = PlayerPrefsManager.Instance.GetCountryId(),
            level = PlayerPrefsManager.Instance.GetHighestLevel(),
            isLocalPlayer = true
        };

        List<LootLockerLeaderboardEntry> entries = new List<LootLockerLeaderboardEntry> { localEntry };
        onComplete?.Invoke(entries);
    }

    // ========== UTILITAIRES ==========

    public bool IsOnline()
    {
        return isAuthenticated;
    }

    public void ForceSync()
    {
        if (!isAuthenticated)
        {
            Debug.LogWarning("⚠️ Impossible de synchroniser : non authentifié");
            return;
        }

        Debug.Log("🔄 Synchronisation forcée des données...");

        int currentScore = PlayerPrefsManager.Instance.GetTotalScore();
        SubmitScore(currentScore, (success) =>
        {
            if (success)
            {
                Debug.Log("✅ Synchronisation réussie !");
            }
            else
            {
                Debug.LogError("❌ Échec de la synchronisation");
            }
        });
    }
}
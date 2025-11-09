using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// LeaderboardManager - VERSION AMÉLIORÉE
/// ✅ Envoie automatiquement le score au démarrage
/// ✅ Affiche le statut de synchronisation
/// ✅ Gère les erreurs proprement
/// </summary>
public class LeaderboardManager : MonoBehaviour
{
    [Header("UI Références")]
    public Transform leaderboardContainer;
    public GameObject leaderboardEntryPrefab;
    public TextMeshProUGUI titleText;
    public Button refreshButton;
    public Button aroundPlayerButton;

    [Header("Synchronisation")]
    public GameObject syncPanel; // Panel "Synchronisation en cours..."
    public TextMeshProUGUI syncStatusText;
    public bool autoSubmitScoreOnStart = true; // ✅ NOUVEAU

    [Header("Options")]
    public bool loadAroundPlayer = false;
    public int leaderboardCount = 20;

    [Header("Couleurs UI")]
    public Color localPlayerColor = new Color(0.3f, 0.6f, 1f, 0.2f);
    public Color firstPlaceColor = new Color(1f, 0.85f, 0.2f, 0.4f);
    public Color secondPlaceColor = new Color(0.8f, 0.8f, 0.8f, 0.4f);
    public Color thirdPlaceColor = new Color(0.9f, 0.6f, 0.3f, 0.4f);

    [Header("Debug")]
    public bool showDebugLogs = true;

    private List<GameObject> currentEntries = new List<GameObject>();
    private LootLockerService lootLocker;
    private bool isScoreSynced = false;

    void Start()
    {
        if (showDebugLogs)
        {
            Debug.Log("========================================");
            Debug.Log("🏆 INITIALISATION DU LEADERBOARD");
            Debug.Log("========================================");
        }

        lootLocker = LootLockerService.Instance;

        // Setup des boutons
        if (refreshButton != null)
            refreshButton.onClick.AddListener(OnRefreshClicked);

        if (aroundPlayerButton != null)
            aroundPlayerButton.onClick.AddListener(OnAroundPlayerClicked);

        // ✅ NOUVEAU: Synchroniser le score avant de charger le leaderboard
        if (autoSubmitScoreOnStart)
        {
            StartCoroutine(SyncScoreAndLoadLeaderboard());
        }
        else
        {
            LoadLeaderboard();
        }
    }

    // ========== SYNCHRONISATION AUTOMATIQUE AU DÉMARRAGE ==========

    IEnumerator SyncScoreAndLoadLeaderboard()
    {
        if (showDebugLogs)
        {
            Debug.Log("🔄 Démarrage de la synchronisation automatique...");
        }

        // Afficher le panel de synchronisation
        ShowSyncPanel(true, "Synchronisation en cours...");

        // Vérifier si LootLocker est en ligne
        if (lootLocker == null || !lootLocker.IsOnline())
        {
            if (showDebugLogs)
            {
                Debug.Log("⚠️ Mode hors ligne - Chargement du leaderboard local");
            }
            ShowSyncPanel(true, "Mode hors ligne");
            yield return new WaitForSeconds(1f);
            ShowSyncPanel(false);
            LoadLeaderboard();
            yield break;
        }

        // Récupérer le score actuel depuis PlayerPrefsManager
        int currentScore = PlayerPrefsManager.Instance.GetTotalScore();
        string playerName = PlayerPrefsManager.Instance.GetPlayerName();

        if (showDebugLogs)
        {
            Debug.Log($"📊 Données à envoyer :");
            Debug.Log($"  • Joueur: {playerName}");
            Debug.Log($"  • Score: {currentScore}");
            Debug.Log($"  • Avatar: {PlayerPrefsManager.Instance.GetAvatarId()}");
            Debug.Log($"  • Pays: {PlayerPrefsManager.Instance.GetCountryId()}");
        }

        // Variable pour attendre la fin de la soumission
        bool submitCompleted = false;
        bool submitSuccess = false;

        // Soumettre le score
        lootLocker.SubmitScore(currentScore, (success) =>
        {
            submitSuccess = success;
            submitCompleted = true;
        });

        // Attendre la fin de la soumission (max 5 secondes)
        float timeout = 5f;
        float elapsed = 0f;

        while (!submitCompleted && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Gérer le résultat
        if (submitCompleted && submitSuccess)
        {
            isScoreSynced = true;
            ShowSyncPanel(true, "✅ Score synchronisé !");

            if (showDebugLogs)
            {
                Debug.Log($"✅ Score {currentScore} envoyé avec succès à LootLocker!");
            }

            yield return new WaitForSeconds(1f);
        }
        else if (submitCompleted && !submitSuccess)
        {
            ShowSyncPanel(true, "⚠️ Échec de la synchronisation");

            if (showDebugLogs)
            {
                Debug.LogWarning("⚠️ Échec de l'envoi du score - Affichage du leaderboard quand même");
            }

            yield return new WaitForSeconds(1.5f);
        }
        else
        {
            ShowSyncPanel(true, "⏱️ Délai d'attente dépassé");

            if (showDebugLogs)
            {
                Debug.LogWarning("⏱️ Timeout lors de l'envoi du score");
            }

            yield return new WaitForSeconds(1.5f);
        }

        // Masquer le panel et charger le leaderboard
        ShowSyncPanel(false);
        LoadLeaderboard();

        if (showDebugLogs)
        {
            Debug.Log("========================================\n");
        }
    }

    void ShowSyncPanel(bool show, string message = "")
    {
        if (syncPanel != null)
        {
            syncPanel.SetActive(show);
        }

        if (syncStatusText != null && !string.IsNullOrEmpty(message))
        {
            syncStatusText.text = message;
        }
    }

    // ========== CHARGEMENT DU LEADERBOARD ==========

    public void LoadLeaderboard()
    {
        if (showDebugLogs)
        {
            Debug.Log($"📥 Chargement du leaderboard (Autour du joueur: {loadAroundPlayer})");
        }

        titleText.text = "Chargement du classement...";
        ClearEntries();

        if (lootLocker == null)
        {
            Debug.LogError("❌ LootLockerService non initialisé !");
            titleText.text = "Erreur de service";
            return;
        }

        System.Action<List<LootLockerLeaderboardEntry>> onLoaded = (entries) => OnLeaderboardLoaded(entries);

        if (loadAroundPlayer)
            lootLocker.GetLeaderboardAroundPlayer(leaderboardCount, onLoaded);
        else
            lootLocker.GetLeaderboard(leaderboardCount, onLoaded);
    }

    private void OnLeaderboardLoaded(List<LootLockerLeaderboardEntry> entries)
    {
        ClearEntries();

        if (entries == null || entries.Count == 0)
        {
            titleText.text = "Aucune donnée de classement 😢";

            if (showDebugLogs)
            {
                Debug.LogWarning("⚠️ Aucune entrée reçue du leaderboard");
            }
            return;
        }

        // Titre avec info de synchronisation
        string syncInfo = isScoreSynced ? " 🌐" : "";
        titleText.text = loadAroundPlayer ? $"🏅 Autour de toi{syncInfo}" : $"🏆 Classement mondial{syncInfo}";

        if (showDebugLogs)
        {
            Debug.Log($"✅ {entries.Count} entrées reçues du leaderboard");
        }

        foreach (var entryData in entries)
        {
            CreateEntry(entryData);
        }

        if (showDebugLogs)
        {
            Debug.Log($"✅ Leaderboard affiché avec succès");
        }
    }

    private void CreateEntry(LootLockerLeaderboardEntry entryData)
    {
        GameObject go = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
        currentEntries.Add(go);

        LeaderboardEntryUI entryUI = go.GetComponent<LeaderboardEntryUI>();
        if (entryUI != null)
        {
            entryUI.SetupLootLocker(entryData, this);
        }
        else
        {
            Debug.LogError("❌ Le préfab 'leaderboardEntryPrefab' ne contient pas le script LeaderboardEntryUI !");
        }
    }

    private void ClearEntries()
    {
        foreach (var go in currentEntries)
        {
            if (go != null) Destroy(go);
        }
        currentEntries.Clear();
    }

    // ========== BOUTONS ==========

    void OnRefreshClicked()
    {
        if (showDebugLogs)
        {
            Debug.Log("🔄 Rafraîchissement du leaderboard...");
        }

        loadAroundPlayer = false;
        StartCoroutine(SyncScoreAndLoadLeaderboard());
    }

    void OnAroundPlayerClicked()
    {
        if (showDebugLogs)
        {
            Debug.Log("🎯 Affichage autour du joueur...");
        }

        loadAroundPlayer = true;
        StartCoroutine(SyncScoreAndLoadLeaderboard());
    }

    // ========== MÉTHODES PUBLIQUES ==========

    /// <summary>
    /// Force la soumission du score et recharge le leaderboard
    /// </summary>
    public void ForceSubmitAndRefresh()
    {
        if (showDebugLogs)
        {
            Debug.Log("🔄 Soumission forcée du score...");
        }

        StartCoroutine(SyncScoreAndLoadLeaderboard());
    }

    /// <summary>
    /// Affiche le leaderboard sans synchroniser (plus rapide)
    /// </summary>
    public void LoadLeaderboardWithoutSync()
    {
        autoSubmitScoreOnStart = false;
        LoadLeaderboard();
    }

    // ========== COULEURS ==========

    public Color GetLocalPlayerColor() => localPlayerColor;

    public Color GetTopRankColor(int rank = -1)
    {
        switch (rank)
        {
            case 1: return firstPlaceColor;
            case 2: return secondPlaceColor;
            case 3: return thirdPlaceColor;
            default: return Color.clear;
        }
    }

    // ========== MÉTHODES DE DEBUG ==========

    [ContextMenu("Force Submit Score Now")]
    public void DebugForceSubmit()
    {
        if (lootLocker == null)
        {
            Debug.LogError("❌ LootLockerService non disponible");
            return;
        }

        int score = PlayerPrefsManager.Instance.GetTotalScore();
        Debug.Log($"🧪 TEST: Envoi du score {score}...");

        lootLocker.SubmitScore(score, (success) =>
        {
            if (success)
            {
                Debug.Log($"✅ TEST: Score {score} envoyé avec succès!");
            }
            else
            {
                Debug.LogError("❌ TEST: Échec de l'envoi");
            }
        });
    }

    [ContextMenu("Print Current PlayerPrefs")]
    public void DebugPrintPlayerPrefs()
    {
        PlayerPrefsManager.Instance.PrintAllPlayerPrefs();
    }

    [ContextMenu("Check LootLocker Status")]
    public void DebugCheckLootLocker()
    {
        if (lootLocker == null)
        {
            Debug.Log("❌ LootLockerService: NULL");
            return;
        }

        Debug.Log("========================================");
        Debug.Log("🔍 STATUT LOOTLOCKER");
        Debug.Log("========================================");
        Debug.Log($"En ligne: {lootLocker.IsOnline()}");
        Debug.Log($"Authentifié: {lootLocker.isAuthenticated}");
        Debug.Log($"Player ID: {lootLocker.currentPlayerId}");
        Debug.Log($"Player Name: {lootLocker.currentPlayerName}");
        Debug.Log($"Leaderboard Key: {lootLocker.leaderboardKey}");
        Debug.Log("========================================\n");
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // Rafraîchir le leaderboard quand l'app revient au premier plan
        if (hasFocus && isScoreSynced)
        {
            if (showDebugLogs)
            {
                Debug.Log("🔄 App revenue au premier plan - Rafraîchissement du leaderboard");
            }
            LoadLeaderboard();
        }
    }
}

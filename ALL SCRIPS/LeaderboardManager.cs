using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI Références")]
    public Transform leaderboardContainer;
    public GameObject leaderboardEntryPrefab; // Doit avoir le composant LeaderboardEntryUI
    public TextMeshProUGUI titleText;
    public Button refreshButton;
    public Button aroundPlayerButton;

    [Header("Options")]
    public bool loadAroundPlayer = false;
    public int leaderboardCount = 20;

    [Header("Couleurs UI")]
    public Color localPlayerColor = new Color(0.3f, 0.6f, 1f, 0.2f);
    public Color firstPlaceColor = new Color(1f, 0.85f, 0.2f, 0.4f);
    public Color secondPlaceColor = new Color(0.8f, 0.8f, 0.8f, 0.4f);
    public Color thirdPlaceColor = new Color(0.9f, 0.6f, 0.3f, 0.4f);

    private List<GameObject> currentEntries = new List<GameObject>();
    private LootLockerService lootLocker;

    void Start()
    {
        lootLocker = LootLockerService.Instance;

        if (refreshButton != null)
            refreshButton.onClick.AddListener(() => { loadAroundPlayer = false; LoadLeaderboard(); });

        if (aroundPlayerButton != null)
            aroundPlayerButton.onClick.AddListener(() => { loadAroundPlayer = true; LoadLeaderboard(); });

        LoadLeaderboard();
    }

    public void LoadLeaderboard()
    {
        titleText.text = "Chargement du classement...";
        ClearEntries();

        if (lootLocker == null)
        {
            Debug.LogError("❌ LootLockerService non initialisé !");
            titleText.text = "Erreur de service";
            return;
        }

        // Utilisation du callback pour recevoir les données
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
            return;
        }

        titleText.text = loadAroundPlayer ? "🏅 Autour de toi" : "🏆 Classement mondial";

        foreach (var entryData in entries)
        {
            CreateEntry(entryData);
        }
    }

    // CRÉATION D'UNE LIGNE DE CLASSEMENT (CORRIGÉE)
    private void CreateEntry(LootLockerLeaderboardEntry entryData)
    {
        GameObject go = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
        currentEntries.Add(go);

        // Récupérer le composant LeaderboardEntryUI et le configurer.
        // C'est lui qui gère toute la logique d'affichage.
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

    // Les méthodes GetColor restent ici car LeaderboardEntryUI les utilise.
    public Color GetLocalPlayerColor() => localPlayerColor;

    public Color GetTopRankColor(int rank = -1) // -1 pour la couleur par défaut
    {
        switch (rank)
        {
            case 1: return firstPlaceColor;
            case 2: return secondPlaceColor;
            case 3: return thirdPlaceColor;
            default: return Color.clear; // Retourne une couleur transparente si pas un top rang
        }
    }
}
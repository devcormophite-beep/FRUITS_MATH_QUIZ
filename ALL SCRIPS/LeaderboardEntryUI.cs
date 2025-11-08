using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Composant d'entrée du leaderboard compatible avec LootLocker
/// VERSION CORRIGÉE : Gère correctement LootLockerLeaderboardEntry
/// </summary>
public class LeaderboardEntryUI : MonoBehaviour
{
    [Header("Composants UI")]
    public TextMeshProUGUI rankText;
    public Image avatarImage;
    public Image flagImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public Image backgroundImage;
    public GameObject localPlayerIndicator;
    public GameObject crownIcon;

    [Header("Badges")]
    public GameObject firstPlaceBadge;
    public GameObject secondPlaceBadge;
    public GameObject thirdPlaceBadge;

    private LootLockerLeaderboardEntry lootLockerData;
    private LeaderboardManager manager;

    /// <summary>
    /// Configuration avec les données LootLocker (en ligne)
    /// </summary>
    public void SetupLootLocker(LootLockerLeaderboardEntry entry, LeaderboardManager leaderboardManager)
    {
        if (entry == null)
        {
            Debug.LogError("❌ LootLockerLeaderboardEntry est null !");
            return;
        }

        if (leaderboardManager == null)
        {
            Debug.LogError("❌ LeaderboardManager est null !");
            return;
        }

        this.lootLockerData = entry;
        this.manager = leaderboardManager;

        Debug.Log($"🎮 Configuration de l'entrée pour {entry.playerName}");
        Debug.Log($"  • Rank: {entry.rank}");
        Debug.Log($"  • Score: {entry.score}");
        Debug.Log($"  • Avatar ID: {entry.avatarId}");
        Debug.Log($"  • Country ID: {entry.countryId}");
        Debug.Log($"  • Level: {entry.level}");

        UpdateDisplay();
    }

    /// <summary>
    /// Configuration avec les données locales (pour compatibilité)
    /// </summary>
    public void Setup(LeaderboardEntry entry, LeaderboardManager leaderboardManager)
    {
        if (entry == null)
        {
            Debug.LogError("❌ LeaderboardEntry est null !");
            return;
        }

        // Convertir LeaderboardEntry en LootLockerLeaderboardEntry
        LootLockerLeaderboardEntry lootEntry = new LootLockerLeaderboardEntry(entry);
        SetupLootLocker(lootEntry, leaderboardManager);
    }

    private void UpdateDisplay()
    {
        if (lootLockerData == null || manager == null)
        {
            Debug.LogWarning("⚠️ Impossible de mettre à jour l'affichage : données manquantes");
            return;
        }

        // --- Rang, Badges et Couronne ---
        if (rankText != null)
        {
            rankText.text = GetRankDisplay(lootLockerData.rank);

            if (lootLockerData.rank <= 3)
            {
                rankText.color = manager.GetTopRankColor(lootLockerData.rank);
                rankText.fontSize = 28;
                rankText.fontStyle = FontStyles.Bold;
            }
            else
            {
                rankText.color = Color.white;
                rankText.fontSize = 24;
                rankText.fontStyle = FontStyles.Normal;
            }
        }

        ShowPodiumBadge(lootLockerData.rank);

        // --- Avatar ---
        LoadAvatar(lootLockerData.avatarId);

        // --- Drapeau ---
        LoadFlag(lootLockerData.countryId);

        // --- Nom du joueur ---
        if (nameText != null)
        {
            nameText.text = lootLockerData.playerName;

            if (lootLockerData.isLocalPlayer)
            {
                nameText.fontStyle = FontStyles.Bold;
                nameText.color = Color.yellow;
            }
            else
            {
                nameText.fontStyle = FontStyles.Normal;
                nameText.color = Color.white;
            }
        }

        // --- Score ---
        if (scoreText != null)
        {
            scoreText.text = FormatScore(lootLockerData.score);
        }

        // --- Niveau ---
        if (levelText != null)
        {
            if (lootLockerData.level > 0)
            {
                levelText.text = $"Niv. {lootLockerData.level}";
                levelText.gameObject.SetActive(true);
            }
            else
            {
                levelText.gameObject.SetActive(false);
            }
        }

        // --- Fond et indicateur du joueur local ---
        if (lootLockerData.isLocalPlayer)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = manager.GetLocalPlayerColor();
            }
            if (localPlayerIndicator != null)
            {
                localPlayerIndicator.SetActive(true);
            }
        }
        else
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = manager.GetTopRankColor(lootLockerData.rank);
            }
            if (localPlayerIndicator != null)
            {
                localPlayerIndicator.SetActive(false);
            }
        }

        // --- Interaction ---
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            // button.onClick.AddListener(OnEntryClicked); // Ajoutez si vous avez une popup de détails
        }
    }

    void LoadAvatar(int avatarId)
    {
        if (avatarImage == null)
        {
            Debug.LogWarning("⚠️ avatarImage n'est pas assigné dans l'Inspector");
            return;
        }

        if (avatarId <= 0)
        {
            Debug.LogWarning($"⚠️ Avatar ID invalide: {avatarId}");
            avatarImage.gameObject.SetActive(false);
            return;
        }

        // Charger depuis Resources/Avatars/
        Sprite avatarSprite = Resources.Load<Sprite>($"Avatars/{avatarId}");

        if (avatarSprite != null)
        {
            avatarImage.sprite = avatarSprite;
            avatarImage.gameObject.SetActive(true);
            Debug.Log($"✅ Avatar {avatarId} chargé");
        }
        else
        {
            Debug.LogWarning($"⚠️ Avatar {avatarId} introuvable dans Resources/Avatars/");
            avatarImage.gameObject.SetActive(false);
        }
    }

    void LoadFlag(int countryId)
    {
        if (flagImage == null)
        {
            Debug.LogWarning("⚠️ flagImage n'est pas assigné dans l'Inspector");
            return;
        }

        if (countryId <= 0)
        {
            Debug.LogWarning($"⚠️ Country ID invalide: {countryId}");
            flagImage.gameObject.SetActive(false);
            return;
        }

        // Charger depuis Resources/Flags/
        Sprite flagSprite = Resources.Load<Sprite>($"Flags/{countryId}");

        if (flagSprite != null)
        {
            flagImage.sprite = flagSprite;
            flagImage.gameObject.SetActive(true);
            Debug.Log($"✅ Drapeau {countryId} chargé");
        }
        else
        {
            Debug.LogWarning($"⚠️ Drapeau {countryId} introuvable dans Resources/Flags/");
            flagImage.gameObject.SetActive(false);
        }
    }

    void ShowPodiumBadge(int rank)
    {
        if (firstPlaceBadge != null) firstPlaceBadge.SetActive(rank == 1);
        if (secondPlaceBadge != null) secondPlaceBadge.SetActive(rank == 2);
        if (thirdPlaceBadge != null) thirdPlaceBadge.SetActive(rank == 3);
        if (crownIcon != null) crownIcon.SetActive(rank == 1);
    }

    string GetRankDisplay(int rank)
    {
        return rank switch
        {
            1 => "🥇",
            2 => "🥈",
            3 => "🥉",
            _ => $"#{rank}"
        };
    }

    string FormatScore(int score)
    {
        if (score >= 1000000) return $"{score / 1000000f:F1}M";
        if (score >= 1000) return $"{score / 1000f:F1}K";
        return score.ToString();
    }

    void Start()
    {
        // Animation d'apparition
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, Vector3.one, 0.3f)
            .setEaseOutBack()
            .setDelay(Random.Range(0f, 0.2f));
    }

    // Méthode publique pour obtenir les données
    public LootLockerLeaderboardEntry GetData()
    {
        return lootLockerData;
    }

    // Méthode pour rafraîchir l'affichage
    public void RefreshDisplay()
    {
        if (lootLockerData != null && manager != null)
        {
            UpdateDisplay();
        }
    }
}
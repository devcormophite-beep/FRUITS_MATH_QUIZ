using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AvatarSelector : MonoBehaviour
{
    [Header("Références UI")]
    public Transform avatarsContainer;
    public GameObject avatarItemPrefab;
    public ScrollRect scrollRect;
    public Image selectedAvatarPreview;
    public TextMeshProUGUI selectedAvatarText;
    public Button confirmButton;

    [Header("Paramètres")]
    public float itemSize = 100f;
    public Color selectedColor = new Color(0.3f, 0.8f, 1f, 1f);
    public Color normalColor = Color.white;
    public int columns = 3; // Nombre de colonnes dans la grille

    [Header("Événements")]
    public UnityEngine.Events.UnityEvent<int> onAvatarSelected;

    private List<Sprite> availableAvatars = new List<Sprite>();
    private List<GameObject> avatarItems = new List<GameObject>();
    private int selectedAvatarId = -1;

    void Start()
    {
        LoadAvatarsFromResources();
        CreateAvatarGrid();

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
            confirmButton.interactable = false;
        }

        // Charger l'avatar sauvegardé
        LoadSavedAvatar();
    }

    void LoadAvatarsFromResources()
    {
        Debug.Log("========================================");
        Debug.Log("CHARGEMENT DES AVATARS");
        Debug.Log("Dossier: Resources/Avatars");
        Debug.Log("========================================");

        Sprite[] sprites = Resources.LoadAll<Sprite>("Avatars");

        if (sprites.Length == 0)
        {
            Debug.LogError("ERREUR: Aucun avatar trouvé dans Resources/Avatars!");
            Debug.LogError("Assurez-vous que:");
            Debug.LogError("1. Le dossier 'Resources/Avatars' existe");
            Debug.LogError("2. Vos images sont au format PNG");
            Debug.LogError("3. Les noms sont: 1.png, 2.png, 3.png, etc.");
            return;
        }

        Debug.Log($"Trouvé {sprites.Length} avatars");

        // Créer un dictionnaire pour trier par numéro
        Dictionary<int, Sprite> sortedAvatars = new Dictionary<int, Sprite>();

        foreach (Sprite sprite in sprites)
        {
            string spriteName = sprite.name;

            if (int.TryParse(spriteName, out int id))
            {
                sortedAvatars[id] = sprite;
                Debug.Log($"  ✓ Avatar {id} chargé: {spriteName}");
            }
            else
            {
                Debug.LogWarning($"  ✗ Nom invalide ignoré: {spriteName} (attendu: nombre comme 1, 2, 3...)");
            }
        }

        // Trier et ajouter à la liste
        for (int i = 1; i <= sortedAvatars.Count; i++)
        {
            if (sortedAvatars.ContainsKey(i))
            {
                availableAvatars.Add(sortedAvatars[i]);
            }
        }

        Debug.Log("========================================");
        Debug.Log($"RÉSUMÉ: {availableAvatars.Count} avatars chargés et triés");
        Debug.Log("========================================\n");
    }

    void CreateAvatarGrid()
    {
        if (availableAvatars.Count == 0)
        {
            Debug.LogWarning("Aucun avatar à afficher");
            return;
        }

        // Configurer le Grid Layout
        GridLayoutGroup grid = avatarsContainer.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            grid.cellSize = new Vector2(itemSize, itemSize);
            grid.spacing = new Vector2(10, 10);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
        }

        // Créer les items
        for (int i = 0; i < availableAvatars.Count; i++)
        {
            CreateAvatarItem(i + 1, availableAvatars[i]);
        }
    }

    void CreateAvatarItem(int avatarId, Sprite avatarSprite)
    {
        GameObject item = Instantiate(avatarItemPrefab, avatarsContainer);
        
        // Configurer l'image de l'avatar
        Image avatarImage = item.transform.Find("AvatarImage")?.GetComponent<Image>();
        Button button = item.GetComponent<Button>();
        Image background = item.GetComponent<Image>();

        if (avatarImage != null && avatarSprite != null)
        {
            avatarImage.sprite = avatarSprite;
            avatarImage.preserveAspect = true;
        }

        if (button != null)
        {
            int id = avatarId; // Capture locale pour le delegate
            button.onClick.AddListener(() => OnAvatarClicked(id, avatarSprite, item));
        }

        // Stocker l'item
        item.name = $"Avatar_{avatarId}";
        avatarItems.Add(item);
    }

    void OnAvatarClicked(int avatarId, Sprite avatarSprite, GameObject item)
    {
        // Désélectionner tous les avatars
        foreach (var avatarItem in avatarItems)
        {
            Image bg = avatarItem.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = normalColor;
            }

            // Remettre la taille normale
            avatarItem.transform.localScale = Vector3.one;
        }

        // Sélectionner le nouveau
        selectedAvatarId = avatarId;

        Image background = item.GetComponent<Image>();
        if (background != null)
        {
            background.color = selectedColor;
        }

        // Effet de zoom
        item.transform.localScale = Vector3.one * 1.1f;

        // Mettre à jour la prévisualisation
        if (selectedAvatarPreview != null)
        {
            selectedAvatarPreview.sprite = avatarSprite;
            selectedAvatarPreview.gameObject.SetActive(true);
        }

        if (selectedAvatarText != null)
        {
            selectedAvatarText.text = GetLocalizedText("avatar") + " #" + avatarId;
        }

        if (confirmButton != null)
        {
            confirmButton.interactable = true;
        }

        Debug.Log($"Avatar sélectionné: #{avatarId}");
    }

    void OnConfirmClicked()
    {
        if (selectedAvatarId == -1)
        {
            Debug.LogWarning("Aucun avatar sélectionné");
            return;
        }

        // AJOUT : Utiliser PlayerPrefsManager
        PlayerPrefsManager.Instance.SetAvatarId(selectedAvatarId);

        Debug.Log($"Avatar confirmé et sauvegardé: #{selectedAvatarId}");

        // Déclencher l'événement
        onAvatarSelected?.Invoke(selectedAvatarId);
    }

    void LoadSavedAvatar()
    {
        if (PlayerPrefs.HasKey("SelectedAvatarId"))
        {
            int savedId = PlayerPrefs.GetInt("SelectedAvatarId");
            Debug.Log($"Avatar sauvegardé trouvé: #{savedId}");

            // Sélectionner automatiquement cet avatar
            if (savedId > 0 && savedId <= availableAvatars.Count)
            {
                var item = avatarItems.Find(i => i.name == $"Avatar_{savedId}");
                if (item != null)
                {
                    OnAvatarClicked(savedId, availableAvatars[savedId - 1], item);
                }
            }
        }
    }

    string GetLocalizedText(string key)
    {
        // Récupérer la langue depuis le CountrySelector si disponible
        string lang = PlayerPrefs.GetString("GameLanguage", "fr");

        switch (key)
        {
            case "avatar":
                switch (lang)
                {
                    case "fr": return "Avatar";
                    case "en": return "Avatar";
                    case "ru": return "Аватар";
                    case "es": return "Avatar";
                    case "pt": return "Avatar";
                    default: return "Avatar";
                }
            case "selected":
                switch (lang)
                {
                    case "fr": return "Avatar sélectionné";
                    case "en": return "Selected avatar";
                    case "ru": return "Выбранный аватар";
                    case "es": return "Avatar seleccionado";
                    case "pt": return "Avatar selecionado";
                    default: return "Selected avatar";
                }
            case "confirm":
                switch (lang)
                {
                    case "fr": return "Confirmer";
                    case "en": return "Confirm";
                    case "ru": return "Подтвердить";
                    case "es": return "Confirmar";
                    case "pt": return "Confirmar";
                    default: return "Confirm";
                }
            default:
                return key;
        }
    }

    // ========== MÉTHODES PUBLIQUES ==========

    public int GetSelectedAvatarId()
    {
        return selectedAvatarId;
    }

    public Sprite GetSelectedAvatarSprite()
    {
        if (selectedAvatarId > 0 && selectedAvatarId <= availableAvatars.Count)
        {
            return availableAvatars[selectedAvatarId - 1];
        }
        return null;
    }

    public Sprite GetAvatarSpriteById(int avatarId)
    {
        if (avatarId > 0 && avatarId <= availableAvatars.Count)
        {
            return availableAvatars[avatarId - 1];
        }
        return null;
    }

    public void ClearSelection()
    {
        selectedAvatarId = -1;

        foreach (var item in avatarItems)
        {
            Image bg = item.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = normalColor;
            }
            item.transform.localScale = Vector3.one;
        }

        if (selectedAvatarPreview != null)
        {
            selectedAvatarPreview.gameObject.SetActive(false);
        }

        if (selectedAvatarText != null)
        {
            selectedAvatarText.text = GetLocalizedText("selected");
        }

        if (confirmButton != null)
        {
            confirmButton.interactable = false;
        }
    }

    public int GetTotalAvatars()
    {
        return availableAvatars.Count;
    }

    // Déverrouiller des avatars (pour progression du jeu)
    public void UnlockAvatar(int avatarId)
    {
        PlayerPrefs.SetInt($"Avatar_{avatarId}_Unlocked", 1);
        PlayerPrefs.Save();
    }

    public bool IsAvatarUnlocked(int avatarId)
    {
        // Par défaut, tous sont déverrouillés
        // Vous pouvez changer cette logique selon vos besoins
        return PlayerPrefs.GetInt($"Avatar_{avatarId}_Unlocked", 1) == 1;
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerProfile : MonoBehaviour
{
    [Header("Références")]
    public CountrySelector countrySelector;
    public AvatarSelector avatarSelector;

    [Header("UI du Profil")]
    public Image profileAvatarImage;
    public Image profileFlagImage;
    public TextMeshProUGUI profileNameText;
    public TextMeshProUGUI profileCountryText;
    public TMP_InputField nameInputField;
    public Button saveProfileButton;

    [Header("Panneaux")]
    public GameObject profilePanel;
    public GameObject avatarSelectionPanel;
    public GameObject countrySelectionPanel;

    [Header("Boutons d'édition")]
    public Button changeAvatarButton;
    public Button changeCountryButton;
    public Button editNameButton;

    private string playerName = "";
    private int currentAvatarId = -1;
    private int currentCountryId = -1;

    void Start()
    {
        LoadPlayerProfile();
        SetupButtons();
        UpdateProfileDisplay();
    }

    void SetupButtons()
    {
        if (changeAvatarButton != null)
            changeAvatarButton.onClick.AddListener(OpenAvatarSelection);

        if (changeCountryButton != null)
            changeCountryButton.onClick.AddListener(OpenCountrySelection);

        if (saveProfileButton != null)
            saveProfileButton.onClick.AddListener(SavePlayerProfile);

        if (editNameButton != null)
            editNameButton.onClick.AddListener(EnableNameEditing);

        // Écouter les sélections
        if (avatarSelector != null)
            avatarSelector.onAvatarSelected.AddListener(OnAvatarSelected);

        if (countrySelector != null)
            countrySelector.onCountrySelected.AddListener(OnCountrySelected);
    }

    void LoadPlayerProfile()
    {
        // CORRECTION : Utiliser PlayerPrefsManager
        var prefsManager = PlayerPrefsManager.Instance;

        playerName = prefsManager.GetPlayerName();
        currentAvatarId = prefsManager.GetAvatarId();
        currentCountryId = prefsManager.GetCountryId();

        Debug.Log($"✅ Profil chargé depuis PlayerPrefsManager:");
        Debug.Log($"  • Nom: {playerName}");
        Debug.Log($"  • Avatar: {currentAvatarId}");
        Debug.Log($"  • Pays: {currentCountryId}");
    }

    void SavePlayerProfile()
    {
        Debug.Log("💾 SAUVEGARDE DU PROFIL COMPLET");

        // CORRECTION : Sauvegarder TOUTES les données en une seule fois
        var prefsManager = PlayerPrefsManager.Instance;

        // Récupérer le nom du champ
        if (nameInputField != null && !string.IsNullOrEmpty(nameInputField.text))
        {
            playerName = nameInputField.text;
        }

        // Récupérer le nom du pays
        string countryName = "";
        if (countrySelector != null)
        {
            countryName = countrySelector.GetSelectedCountryName();
        }

        // SAUVEGARDE ATOMIQUE DE TOUT LE PROFIL
        prefsManager.SaveCompleteProfile(
            playerName,
            currentAvatarId,
            currentCountryId,
            countryName
        );

        // Afficher l'état complet
        prefsManager.PrintAllPlayerPrefs();

        UpdateProfileDisplay();

        Debug.Log("✅ Profil sauvegardé avec succès !");

        // Optionnel : Synchroniser avec LootLocker si connecté
        if (LootLockerService.Instance != null && LootLockerService.Instance.IsOnline())
        {
            Debug.Log("🔄 Mise à jour du nom sur LootLocker...");
            LootLockerService.Instance.SetPlayerName(playerName);
        }
    }

    void UpdateProfileDisplay()
    {
        // Mettre à jour le nom
        if (profileNameText != null)
        {
            profileNameText.text = playerName;
        }

        if (nameInputField != null)
        {
            nameInputField.text = playerName;
        }

        // Mettre à jour l'avatar
        if (profileAvatarImage != null && avatarSelector != null)
        {
            Sprite avatarSprite = avatarSelector.GetAvatarSpriteById(currentAvatarId);
            if (avatarSprite != null)
            {
                profileAvatarImage.sprite = avatarSprite;
                profileAvatarImage.gameObject.SetActive(true);
            }
            else
            {
                profileAvatarImage.gameObject.SetActive(false);
            }
        }

        // Mettre à jour le drapeau
        if (profileFlagImage != null && currentCountryId > 0)
        {
            Sprite flagSprite = Resources.Load<Sprite>($"Flags/{currentCountryId}");
            if (flagSprite != null)
            {
                profileFlagImage.sprite = flagSprite;
                profileFlagImage.gameObject.SetActive(true);
            }
        }

        // Mettre à jour le nom du pays
        if (profileCountryText != null && countrySelector != null)
        {
            string countryName = countrySelector.GetSelectedCountryName();
            if (!string.IsNullOrEmpty(countryName))
            {
                profileCountryText.text = countryName;
            }
            else
            {
                profileCountryText.text = GetLocalizedText("no_country");
            }
        }
    }

    void OpenAvatarSelection()
    {
        if (profilePanel != null)
            profilePanel.SetActive(false);

        if (avatarSelectionPanel != null)
            avatarSelectionPanel.SetActive(true);
    }

    void OpenCountrySelection()
    {
        if (profilePanel != null)
            profilePanel.SetActive(false);

        if (countrySelectionPanel != null)
            countrySelectionPanel.SetActive(true);
    }

    void OnAvatarSelected(int avatarId)
    {
        Debug.Log($"🎨 Avatar sélectionné: {avatarId}");

        currentAvatarId = avatarId;

        // CORRECTION : Sauvegarder immédiatement via PlayerPrefsManager
        PlayerPrefsManager.Instance.SetAvatarId(avatarId);

        // Retourner au profil
        if (avatarSelectionPanel != null)
            avatarSelectionPanel.SetActive(false);

        if (profilePanel != null)
            profilePanel.SetActive(true);

        UpdateProfileDisplay();
    }

    void OnCountrySelected(string countryName)
    {
        Debug.Log($"🌍 Pays sélectionné: {countryName}");

        // CORRECTION : Récupérer l'ID du pays depuis CountrySelector
        currentCountryId = countrySelector.GetSelectedCountryId();

        // CORRECTION : Sauvegarder immédiatement via PlayerPrefsManager
        PlayerPrefsManager.Instance.SetCountryId(currentCountryId);
        PlayerPrefsManager.Instance.SetCountryName(countryName);

        // Retourner au profil
        if (countrySelectionPanel != null)
            countrySelectionPanel.SetActive(false);

        if (profilePanel != null)
            profilePanel.SetActive(true);

        UpdateProfileDisplay();
    }

    void EnableNameEditing()
    {
        if (nameInputField != null)
        {
            nameInputField.interactable = true;
            nameInputField.Select();
        }
    }

    string GetLocalizedText(string key)
    {
        string lang = PlayerPrefs.GetString("GameLanguage", "fr");

        switch (key)
        {
            case "no_country":
                switch (lang)
                {
                    case "fr": return "Aucun pays sélectionné";
                    case "en": return "No country selected";
                    case "ru": return "Страна не выбрана";
                    case "es": return "Ningún país seleccionado";
                    case "pt": return "Nenhum país selecionado";
                    default: return "No country selected";
                }
            case "no_avatar":
                switch (lang)
                {
                    case "fr": return "Aucun avatar sélectionné";
                    case "en": return "No avatar selected";
                    case "ru": return "Аватар не выбран";
                    case "es": return "Ningún avatar seleccionado";
                    case "pt": return "Nenhum avatar selecionado";
                    default: return "No avatar selected";
                }
            default:
                return key;
        }
    }

    // ========== MÉTHODES PUBLIQUES ==========

    public string GetPlayerName()
    {
        return PlayerPrefsManager.Instance.GetPlayerName();
    }

    public int GetPlayerAvatarId()
    {
        return PlayerPrefsManager.Instance.GetAvatarId();
    }

    public int GetPlayerCountryId()
    {
        return PlayerPrefsManager.Instance.GetCountryId();
    }

    public Sprite GetPlayerAvatarSprite()
    {
        if (avatarSelector != null)
        {
            return avatarSelector.GetAvatarSpriteById(currentAvatarId);
        }
        return null;
    }

    public Sprite GetPlayerFlagSprite()
    {
        int countryId = PlayerPrefsManager.Instance.GetCountryId();
        if (countryId > 0)
        {
            return Resources.Load<Sprite>($"Flags/{countryId}");
        }
        return null;
    }

    public void SetPlayerName(string newName)
    {
        playerName = newName;
        PlayerPrefsManager.Instance.SetPlayerName(newName);
        UpdateProfileDisplay();
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class UsernameSetup : MonoBehaviour
{
    [Header("UI Références")]
    public TMP_InputField usernameInputField;
    public Button continueButton;
    public Button skipButton;
    public TextMeshProUGUI errorMessageText;
    public TextMeshProUGUI characterCountText;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI instructionText;

    [Header("Paramètres")]
    public int minCharacters = 3;
    public int maxCharacters = 15;
    public bool allowSpaces = false;
    public bool allowSpecialCharacters = false;
    public string defaultUsername = "Joueur";
    public string nextSceneName = "MainMenu";

    [Header("Effets visuels")]
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;
    public Color normalColor = Color.white;
    public GameObject loadingPanel;
    public Animator inputAnimator; // Optionnel pour animations

    [Header("Audio")]
    public AudioClip validationSound;
    public AudioClip errorSound;
    public AudioClip typeSound;

    private AudioSource audioSource;
    private bool isValid = false;
    private string currentLanguage = "fr";

    void Start()
    {
        // Charger la langue
        currentLanguage = PlayerPrefs.GetString("GameLanguage", "fr");

        // Setup Audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Setup UI
        SetupUI();
        UpdateLocalizedTexts();

        // Vérifier si le joueur a déjà un pseudo
        if (PlayerPrefs.HasKey("PlayerName") && !string.IsNullOrEmpty(PlayerPrefs.GetString("PlayerName")))
        {
            string savedName = PlayerPrefs.GetString("PlayerName");
            usernameInputField.text = savedName;
            Debug.Log($"Pseudo existant chargé: {savedName}");
        }

        // Événements
        usernameInputField.onValueChanged.AddListener(OnUsernameChanged);
        usernameInputField.onEndEdit.AddListener(OnUsernameEndEdit);
        continueButton.onClick.AddListener(OnContinueClicked);
        
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnSkipClicked);
        }

        // Focus sur le champ
        usernameInputField.Select();
        usernameInputField.ActivateInputField();
    }

    void SetupUI()
    {
        // Configuration de l'InputField
        usernameInputField.characterLimit = maxCharacters;
        usernameInputField.contentType = TMP_InputField.ContentType.Standard;

        // Désactiver le bouton continuer au départ si pas de pseudo
        if (string.IsNullOrEmpty(usernameInputField.text))
        {
            continueButton.interactable = false;
        }

        // Cacher le message d'erreur
        if (errorMessageText != null)
        {
            errorMessageText.gameObject.SetActive(false);
        }
    }

    void UpdateLocalizedTexts()
    {
        if (titleText != null)
        {
            titleText.text = GetLocalizedText("title");
        }

        if (instructionText != null)
        {
            instructionText.text = GetLocalizedText("instruction");
        }

        if (continueButton != null)
        {
            TextMeshProUGUI btnText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = GetLocalizedText("continue");
            }
        }

        if (skipButton != null)
        {
            TextMeshProUGUI btnText = skipButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = GetLocalizedText("skip");
            }
        }
    }

    void OnUsernameChanged(string username)
    {
        // Jouer son de frappe
        PlaySound(typeSound);

        // Valider en temps réel
        ValidateUsername(username);

        // Mettre à jour le compteur de caractères
        UpdateCharacterCount(username.Length);
    }

    void OnUsernameEndEdit(string username)
    {
        ValidateUsername(username);
    }

    void ValidateUsername(string username)
    {
        // Effacer l'ancien message d'erreur
        if (errorMessageText != null)
        {
            errorMessageText.gameObject.SetActive(false);
        }

        // Vérifier si vide
        if (string.IsNullOrEmpty(username))
        {
            SetInputFieldColor(normalColor);
            continueButton.interactable = false;
            isValid = false;
            return;
        }

        // Vérifier la longueur minimum
        if (username.Length < minCharacters)
        {
            ShowError(GetLocalizedText("error_too_short").Replace("{min}", minCharacters.ToString()));
            SetInputFieldColor(invalidColor);
            continueButton.interactable = false;
            isValid = false;
            return;
        }

        // Vérifier la longueur maximum
        if (username.Length > maxCharacters)
        {
            ShowError(GetLocalizedText("error_too_long").Replace("{max}", maxCharacters.ToString()));
            SetInputFieldColor(invalidColor);
            continueButton.interactable = false;
            isValid = false;
            return;
        }

        // Vérifier les espaces
        if (!allowSpaces && username.Contains(" "))
        {
            ShowError(GetLocalizedText("error_no_spaces"));
            SetInputFieldColor(invalidColor);
            continueButton.interactable = false;
            isValid = false;
            return;
        }

        // Vérifier les caractères spéciaux
        if (!allowSpecialCharacters)
        {
            string pattern = @"^[a-zA-Z0-9_]+$";
            if (!Regex.IsMatch(username, pattern))
            {
                ShowError(GetLocalizedText("error_special_chars"));
                SetInputFieldColor(invalidColor);
                continueButton.interactable = false;
                isValid = false;
                return;
            }
        }

        // Vérifier les mots interdits (optionnel)
        if (ContainsBadWords(username))
        {
            ShowError(GetLocalizedText("error_inappropriate"));
            SetInputFieldColor(invalidColor);
            continueButton.interactable = false;
            isValid = false;
            return;
        }

        // Tout est valide !
        SetInputFieldColor(validColor);
        continueButton.interactable = true;
        isValid = true;

        if (inputAnimator != null)
        {
            inputAnimator.SetTrigger("Valid");
        }
    }

    void ShowError(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.gameObject.SetActive(true);
            PlaySound(errorSound);
        }
    }

    void SetInputFieldColor(Color color)
    {
        Image inputImage = usernameInputField.GetComponent<Image>();
        if (inputImage != null)
        {
            inputImage.color = color;
        }
    }

    void UpdateCharacterCount(int currentLength)
    {
        if (characterCountText != null)
        {
            characterCountText.text = $"{currentLength}/{maxCharacters}";

            // Changer la couleur selon la longueur
            if (currentLength < minCharacters)
            {
                characterCountText.color = invalidColor;
            }
            else if (currentLength >= maxCharacters)
            {
                characterCountText.color = Color.yellow;
            }
            else
            {
                characterCountText.color = validColor;
            }
        }
    }

    void OnContinueClicked()
    {
        if (!isValid)
        {
            ShowError(GetLocalizedText("error_invalid"));
            PlaySound(errorSound);
            return;
        }

        string username = usernameInputField.text.Trim();

        // Sauvegarder le pseudo
        PlayerPrefs.SetString("PlayerName", username);
        PlayerPrefs.SetInt("HasCompletedSetup", 1);
        PlayerPrefs.Save();

        Debug.Log($"Pseudo sauvegardé: {username}");

        // Son de validation
        PlaySound(validationSound);

        // Charger la scène suivante
        LoadNextScene();
    }

    void OnSkipClicked()
    {
        // Utiliser le pseudo par défaut
        PlayerPrefs.SetString("PlayerName", defaultUsername);
        PlayerPrefs.SetInt("HasCompletedSetup", 1);
        PlayerPrefs.Save();

        Debug.Log($"Pseudo par défaut utilisé: {defaultUsername}");

        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        // Petit délai pour l'effet visuel
        Invoke(nameof(LoadScene), 0.5f);
    }

    void LoadScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    bool ContainsBadWords(string username)
    {
        // Liste de mots interdits (à personnaliser selon vos besoins)
        string[] badWords = { "admin", "root", "moderator", "test" };

        string lowerUsername = username.ToLower();
        foreach (string badWord in badWords)
        {
            if (lowerUsername.Contains(badWord.ToLower()))
            {
                return true;
            }
        }

        return false;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    string GetLocalizedText(string key)
    {
        switch (key)
        {
            case "title":
                switch (currentLanguage)
                {
                    case "fr": return "Choisissez votre pseudo";
                    case "en": return "Choose your username";
                    case "ru": return "Выберите свой псевдоним";
                    case "es": return "Elige tu nombre de usuario";
                    case "pt": return "Escolha seu nome de usuário";
                    default: return "Choose your username";
                }

            case "instruction":
                switch (currentLanguage)
                {
                    case "fr": return $"Entre {minCharacters} et {maxCharacters} caractères";
                    case "en": return $"Between {minCharacters} and {maxCharacters} characters";
                    case "ru": return $"От {minCharacters} до {maxCharacters} символов";
                    case "es": return $"Entre {minCharacters} y {maxCharacters} caracteres";
                    case "pt": return $"Entre {minCharacters} e {maxCharacters} caracteres";
                    default: return $"Between {minCharacters} and {maxCharacters} characters";
                }

            case "continue":
                switch (currentLanguage)
                {
                    case "fr": return "Continuer";
                    case "en": return "Continue";
                    case "ru": return "Продолжить";
                    case "es": return "Continuar";
                    case "pt": return "Continuar";
                    default: return "Continue";
                }

            case "skip":
                switch (currentLanguage)
                {
                    case "fr": return "Passer";
                    case "en": return "Skip";
                    case "ru": return "Пропустить";
                    case "es": return "Omitir";
                    case "pt": return "Pular";
                    default: return "Skip";
                }

            case "error_too_short":
                switch (currentLanguage)
                {
                    case "fr": return "Minimum {min} caractères requis";
                    case "en": return "Minimum {min} characters required";
                    case "ru": return "Требуется минимум {min} символов";
                    case "es": return "Se requieren mínimo {min} caracteres";
                    case "pt": return "Mínimo de {min} caracteres necessários";
                    default: return "Minimum {min} characters required";
                }

            case "error_too_long":
                switch (currentLanguage)
                {
                    case "fr": return "Maximum {max} caractères autorisés";
                    case "en": return "Maximum {max} characters allowed";
                    case "ru": return "Максимум {max} символов разрешено";
                    case "es": return "Máximo {max} caracteres permitidos";
                    case "pt": return "Máximo de {max} caracteres permitidos";
                    default: return "Maximum {max} characters allowed";
                }

            case "error_no_spaces":
                switch (currentLanguage)
                {
                    case "fr": return "Les espaces ne sont pas autorisés";
                    case "en": return "Spaces are not allowed";
                    case "ru": return "Пробелы не допускаются";
                    case "es": return "No se permiten espacios";
                    case "pt": return "Espaços não são permitidos";
                    default: return "Spaces are not allowed";
                }

            case "error_special_chars":
                switch (currentLanguage)
                {
                    case "fr": return "Seuls les lettres, chiffres et _ sont autorisés";
                    case "en": return "Only letters, numbers and _ are allowed";
                    case "ru": return "Разрешены только буквы, цифры и _";
                    case "es": return "Solo se permiten letras, números y _";
                    case "pt": return "Apenas letras, números e _ são permitidos";
                    default: return "Only letters, numbers and _ are allowed";
                }

            case "error_inappropriate":
                switch (currentLanguage)
                {
                    case "fr": return "Ce pseudo n'est pas autorisé";
                    case "en": return "This username is not allowed";
                    case "ru": return "Этот псевдоним не разрешен";
                    case "es": return "Este nombre de usuario no está permitido";
                    case "pt": return "Este nome de usuário não é permitido";
                    default: return "This username is not allowed";
                }

            case "error_invalid":
                switch (currentLanguage)
                {
                    case "fr": return "Pseudo invalide";
                    case "en": return "Invalid username";
                    case "ru": return "Недействительный псевдоним";
                    case "es": return "Nombre de usuario inválido";
                    case "pt": return "Nome de usuário inválido";
                    default: return "Invalid username";
                }

            default:
                return key;
        }
    }

    // Méthode publique pour vérifier si le setup est terminé
    public static bool HasCompletedSetup()
    {
        return PlayerPrefs.GetInt("HasCompletedSetup", 0) == 1;
    }

    // Réinitialiser le setup (pour le développement)
    public static void ResetSetup()
    {
        PlayerPrefs.DeleteKey("HasCompletedSetup");
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.Save();
    }
}
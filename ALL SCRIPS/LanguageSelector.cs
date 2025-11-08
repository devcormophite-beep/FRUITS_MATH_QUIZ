using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Gestionnaire de sélection de langue pour le jeu
/// Prend en charge: Français, Anglais, Russe, Espagnol, Portugais
/// </summary>
public class LanguageSelector : MonoBehaviour
{
    [System.Serializable]
    public class LanguageOption
    {
        public string languageCode;     // "fr", "en", "ru", "es", "pt"
        public string languageName;     // "Français", "English", etc.
        public string nativeName;       // Nom dans la langue native
        public Sprite flagIcon;         // Drapeau du pays
        public Button languageButton;   // Bouton UI correspondant
    }

    [Header("Configuration des Langues")]
    public List<LanguageOption> availableLanguages = new List<LanguageOption>();

    [Header("Références UI")]
    public Transform languagesContainer;
    public GameObject languageItemPrefab;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI selectedLanguageText;
    public Button confirmButton;
    public Button skipButton;

    [Header("Prévisualisation")]
    public Image selectedFlagImage;
    public GameObject selectedLanguagePanel;

    [Header("Paramètres")]
    public Color selectedColor = new Color(0.3f, 0.8f, 1f, 1f);
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    public bool showNativeNames = true;
    public string nextSceneName = "MainMenu";

    [Header("Effets Audio")]
    public AudioClip selectionSound;
    public AudioClip confirmSound;
    public AudioClip hoverSound;

    [Header("Événements")]
    public UnityEngine.Events.UnityEvent<string> onLanguageChanged;

    private string currentLanguage = "fr";
    private string selectedLanguage = "";
    private List<GameObject> languageItems = new List<GameObject>();
    private AudioSource audioSource;

    // Dictionnaire de traductions pour l'interface
    private Dictionary<string, Dictionary<string, string>> translations = new Dictionary<string, Dictionary<string, string>>();

    void Start()
    {
        InitializeTranslations();
        SetupAudio();
        LoadSavedLanguage();

        if (availableLanguages.Count == 0)
        {
            LoadLanguagesFromResources();
        }

        CreateLanguageItems();
        SetupButtons();
        UpdateUI();
    }

    void InitializeTranslations()
    {
        // Titre
        translations["title"] = new Dictionary<string, string>
        {
            { "fr", "Choisissez votre langue" },
            { "en", "Choose your language" },
            { "ru", "Выберите язык" },
            { "es", "Elige tu idioma" },
            { "pt", "Escolha seu idioma" }
        };

        // Instructions
        translations["instruction"] = new Dictionary<string, string>
        {
            { "fr", "Sélectionnez la langue du jeu" },
            { "en", "Select the game language" },
            { "ru", "Выберите язык игры" },
            { "es", "Selecciona el idioma del juego" },
            { "pt", "Selecione o idioma do jogo" }
        };

        // Langue sélectionnée
        translations["selected"] = new Dictionary<string, string>
        {
            { "fr", "Langue sélectionnée" },
            { "en", "Selected language" },
            { "ru", "Выбранный язык" },
            { "es", "Idioma seleccionado" },
            { "pt", "Idioma selecionado" }
        };

        // Bouton Confirmer
        translations["confirm"] = new Dictionary<string, string>
        {
            { "fr", "Confirmer" },
            { "en", "Confirm" },
            { "ru", "Подтвердить" },
            { "es", "Confirmar" },
            { "pt", "Confirmar" }
        };

        // Bouton Passer
        translations["skip"] = new Dictionary<string, string>
        {
            { "fr", "Passer" },
            { "en", "Skip" },
            { "ru", "Пропустить" },
            { "es", "Omitir" },
            { "pt", "Pular" }
        };

        // Noms des langues
        translations["language_fr"] = new Dictionary<string, string>
        {
            { "fr", "Français" },
            { "en", "French" },
            { "ru", "Французский" },
            { "es", "Francés" },
            { "pt", "Francês" }
        };

        translations["language_en"] = new Dictionary<string, string>
        {
            { "fr", "Anglais" },
            { "en", "English" },
            { "ru", "Английский" },
            { "es", "Inglés" },
            { "pt", "Inglês" }
        };

        translations["language_ru"] = new Dictionary<string, string>
        {
            { "fr", "Russe" },
            { "en", "Russian" },
            { "ru", "Русский" },
            { "es", "Ruso" },
            { "pt", "Russo" }
        };

        translations["language_es"] = new Dictionary<string, string>
        {
            { "fr", "Espagnol" },
            { "en", "Spanish" },
            { "ru", "Испанский" },
            { "es", "Español" },
            { "pt", "Espanhol" }
        };

        translations["language_pt"] = new Dictionary<string, string>
        {
            { "fr", "Portugais" },
            { "en", "Portuguese" },
            { "ru", "Португальский" },
            { "es", "Portugués" },
            { "pt", "Português" }
        };
    }

    void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void LoadSavedLanguage()
    {
        if (PlayerPrefs.HasKey("GameLanguage"))
        {
            currentLanguage = PlayerPrefs.GetString("GameLanguage", "fr");
            selectedLanguage = currentLanguage;
            Debug.Log($"✓ Langue sauvegardée chargée: {currentLanguage}");
        }
        else
        {
            // Détecter la langue du système
            currentLanguage = DetectSystemLanguage();
            selectedLanguage = currentLanguage;
            Debug.Log($"✓ Langue détectée: {currentLanguage}");
        }
    }

    string DetectSystemLanguage()
    {
        SystemLanguage sysLang = Application.systemLanguage;

        switch (sysLang)
        {
            case SystemLanguage.French:
                return "fr";
            case SystemLanguage.English:
                return "en";
            case SystemLanguage.Russian:
                return "ru";
            case SystemLanguage.Spanish:
                return "es";
            case SystemLanguage.Portuguese:
                return "pt";
            default:
                return "en"; // Anglais par défaut
        }
    }

    void LoadLanguagesFromResources()
    {
        Debug.Log("========================================");
        Debug.Log("CHARGEMENT DES LANGUES DEPUIS RESOURCES");
        Debug.Log("========================================");

        // Définir les langues disponibles avec leurs drapeaux
        var languages = new Dictionary<string, (string nativeName, string flagPath)>
        {
            { "fr", ("Français", "Flags/75") },      // France
            { "en", ("English", "Flags/222") },      // UK
            { "ru", ("Русский", "Flags/168") },      // Russie
            { "es", ("Español", "Flags/197") },      // Espagne
            { "pt", ("Português", "Flags/30") }      // Brésil
        };

        foreach (var lang in languages)
        {
            string code = lang.Key;
            string nativeName = lang.Value.nativeName;
            string flagPath = lang.Value.flagPath;

            Sprite flagSprite = Resources.Load<Sprite>(flagPath);

            if (flagSprite == null)
            {
                Debug.LogWarning($"⚠️ Drapeau non trouvé pour {nativeName} à {flagPath}");
            }

            LanguageOption option = new LanguageOption
            {
                languageCode = code,
                languageName = GetTranslation($"language_{code}", code),
                nativeName = nativeName,
                flagIcon = flagSprite
            };

            availableLanguages.Add(option);
            Debug.Log($"  ✓ Langue ajoutée: {nativeName} ({code})");
        }

        Debug.Log($"✓ {availableLanguages.Count} langues chargées");
        Debug.Log("========================================\n");
    }

    void CreateLanguageItems()
    {
        if (languagesContainer == null || languageItemPrefab == null)
        {
            Debug.LogError("❌ Container ou Prefab manquant!");
            return;
        }

        foreach (var language in availableLanguages)
        {
            CreateLanguageItem(language);
        }

        // Sélectionner automatiquement la langue actuelle
        if (!string.IsNullOrEmpty(selectedLanguage))
        {
            var item = languageItems.Find(i => i.name == $"Language_{selectedLanguage}");
            if (item != null)
            {
                OnLanguageClicked(selectedLanguage, item);
            }
        }
    }

    void CreateLanguageItem(LanguageOption language)
    {
        GameObject item = Instantiate(languageItemPrefab, languagesContainer);
        item.name = $"Language_{language.languageCode}";

        // Configurer le drapeau
        Image flagImage = item.transform.Find("FlagImage")?.GetComponent<Image>();
        if (flagImage != null && language.flagIcon != null)
        {
            flagImage.sprite = language.flagIcon;
            flagImage.preserveAspect = true;
        }

        // Configurer le texte
        TextMeshProUGUI nameText = item.transform.Find("LanguageName")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
        {
            if (showNativeNames)
            {
                nameText.text = language.nativeName;
            }
            else
            {
                nameText.text = GetTranslation($"language_{language.languageCode}", currentLanguage);
            }
        }

        // Configurer le bouton
        Button button = item.GetComponent<Button>();
        if (button != null)
        {
            language.languageButton = button;

            // Ajouter les événements
            button.onClick.AddListener(() => OnLanguageClicked(language.languageCode, item));

            // Effet de survol
            var trigger = item.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
            };
            pointerEnter.callback.AddListener((data) => OnLanguageHover(item));
            trigger.triggers.Add(pointerEnter);

            var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit
            };
            pointerExit.callback.AddListener((data) => OnLanguageExit(item));
            trigger.triggers.Add(pointerExit);
        }

        languageItems.Add(item);
    }

    void OnLanguageHover(GameObject item)
    {
        if (item.name != $"Language_{selectedLanguage}")
        {
            Image bg = item.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = hoverColor;
            }

            // Effet de zoom léger
            LeanTween.scale(item, Vector3.one * 1.05f, 0.2f).setEaseOutQuad();

            PlaySound(hoverSound);
        }
    }

    void OnLanguageExit(GameObject item)
    {
        if (item.name != $"Language_{selectedLanguage}")
        {
            Image bg = item.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = normalColor;
            }

            LeanTween.scale(item, Vector3.one, 0.2f).setEaseOutQuad();
        }
    }

    void OnLanguageClicked(string languageCode, GameObject item)
    {
        // Désélectionner tous les items
        foreach (var langItem in languageItems)
        {
            Image bg = langItem.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = normalColor;
            }
            langItem.transform.localScale = Vector3.one;
        }

        // Sélectionner le nouveau
        selectedLanguage = languageCode;

        Image background = item.GetComponent<Image>();
        if (background != null)
        {
            background.color = selectedColor;
        }

        // Effet de sélection
        LeanTween.scale(item, Vector3.one * 1.1f, 0.2f)
            .setEaseOutBack();

        // Mettre à jour la prévisualisation
        UpdatePreview(languageCode);

        // Mettre à jour l'UI avec la nouvelle langue
        currentLanguage = languageCode;
        UpdateUI();

        // Activer le bouton confirmer
        if (confirmButton != null)
        {
            confirmButton.interactable = true;
        }

        PlaySound(selectionSound);

        Debug.Log($"✓ Langue sélectionnée: {languageCode}");

        // Déclencher l'événement
        onLanguageChanged?.Invoke(languageCode);
    }

    void UpdatePreview(string languageCode)
    {
        if (selectedLanguagePanel != null)
        {
            selectedLanguagePanel.SetActive(true);
        }

        var language = availableLanguages.Find(l => l.languageCode == languageCode);
        if (language == null) return;

        // Mettre à jour le drapeau
        if (selectedFlagImage != null && language.flagIcon != null)
        {
            selectedFlagImage.sprite = language.flagIcon;
            selectedFlagImage.gameObject.SetActive(true);
        }

        // Mettre à jour le texte
        if (selectedLanguageText != null)
        {
            selectedLanguageText.text = $"{GetTranslation("selected", languageCode)}: {language.nativeName}";
        }
    }

    void SetupButtons()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
            confirmButton.interactable = !string.IsNullOrEmpty(selectedLanguage);
        }

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnSkipClicked);
        }
    }

    void UpdateUI()
    {
        if (titleText != null)
        {
            titleText.text = GetTranslation("title", currentLanguage);
        }

        if (instructionText != null)
        {
            instructionText.text = GetTranslation("instruction", currentLanguage);
        }

        if (confirmButton != null)
        {
            TextMeshProUGUI btnText = confirmButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = GetTranslation("confirm", currentLanguage);
            }
        }

        if (skipButton != null)
        {
            TextMeshProUGUI btnText = skipButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = GetTranslation("skip", currentLanguage);
            }
        }

        // Mettre à jour les noms des langues si nécessaire
        if (!showNativeNames)
        {
            UpdateLanguageNames();
        }
    }

    void UpdateLanguageNames()
    {
        for (int i = 0; i < languageItems.Count && i < availableLanguages.Count; i++)
        {
            TextMeshProUGUI nameText = languageItems[i].transform.Find("LanguageName")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = GetTranslation($"language_{availableLanguages[i].languageCode}", currentLanguage);
            }
        }
    }

    void OnConfirmClicked()
    {
        if (string.IsNullOrEmpty(selectedLanguage))
        {
            Debug.LogWarning("⚠️ Aucune langue sélectionnée");
            return;
        }

        // Sauvegarder la langue
        PlayerPrefs.SetString("GameLanguage", selectedLanguage);
        PlayerPrefs.SetInt("HasSelectedLanguage", 1);
        PlayerPrefs.Save();

        Debug.Log($"✓ Langue sauvegardée: {selectedLanguage}");

        PlaySound(confirmSound);

        // Charger la scène suivante
        LoadNextScene();
    }

    void OnSkipClicked()
    {
        // Utiliser la langue par défaut (système ou français)
        string defaultLang = DetectSystemLanguage();
        PlayerPrefs.SetString("GameLanguage", defaultLang);
        PlayerPrefs.SetInt("HasSelectedLanguage", 1);
        PlayerPrefs.Save();

        Debug.Log($"✓ Langue par défaut utilisée: {defaultLang}");

        LoadNextScene();
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    string GetTranslation(string key, string languageCode)
    {
        if (translations.ContainsKey(key))
        {
            if (translations[key].ContainsKey(languageCode))
            {
                return translations[key][languageCode];
            }
        }
        return key;
    }

    // ========== MÉTHODES PUBLIQUES ==========

    public string GetCurrentLanguage()
    {
        return currentLanguage;
    }

    public string GetSelectedLanguage()
    {
        return selectedLanguage;
    }

    public void SetLanguage(string languageCode)
    {
        if (availableLanguages.Exists(l => l.languageCode == languageCode))
        {
            var item = languageItems.Find(i => i.name == $"Language_{languageCode}");
            if (item != null)
            {
                OnLanguageClicked(languageCode, item);
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ Langue {languageCode} non disponible");
        }
    }

    public void ClearSelection()
    {
        selectedLanguage = "";

        foreach (var item in languageItems)
        {
            Image bg = item.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = normalColor;
            }
            item.transform.localScale = Vector3.one;
        }

        if (selectedLanguagePanel != null)
        {
            selectedLanguagePanel.SetActive(false);
        }

        if (confirmButton != null)
        {
            confirmButton.interactable = false;
        }
    }

    public List<string> GetAvailableLanguageCodes()
    {
        List<string> codes = new List<string>();
        foreach (var lang in availableLanguages)
        {
            codes.Add(lang.languageCode);
        }
        return codes;
    }

    public string GetLanguageName(string languageCode, bool useNativeName = true)
    {
        var language = availableLanguages.Find(l => l.languageCode == languageCode);
        if (language != null)
        {
            return useNativeName ? language.nativeName : language.languageName;
        }
        return languageCode;
    }

    // Méthode statique pour vérifier si la langue a été sélectionnée
    public static bool HasSelectedLanguage()
    {
        return PlayerPrefs.GetInt("HasSelectedLanguage", 0) == 1;
    }

    // Réinitialiser la sélection (pour développement)
    public static void ResetLanguageSelection()
    {
        PlayerPrefs.DeleteKey("HasSelectedLanguage");
        PlayerPrefs.DeleteKey("GameLanguage");
        PlayerPrefs.Save();
        Debug.Log("✓ Sélection de langue réinitialisée");
    }
}
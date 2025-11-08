using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

/// <summary>
/// Gestionnaire du Menu Principal - VERSION CORRIGÉE
/// Utilise PlayerPrefsManager au lieu de DataService
/// Affiche correctement avatar, drapeau et toutes les données
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Profil du Joueur")]
    public Image playerAvatarImage;
    public Image playerFlagImage;
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI playerLevelText;
    public TextMeshProUGUI playerRankText;
    public TextMeshProUGUI welcomeText;

    [Header("Statistiques Rapides")]
    public TextMeshProUGUI gamesPlayedText;
    public TextMeshProUGUI winRateText;
    public TextMeshProUGUI accuracyText;
    public GameObject statsPanel;

    [Header("Boutons de Navigation")]
    public Button playButton;
    public Button leaderboardButton;
    public Button profileButton;
    public Button settingsButton;
    public Button quitButton;
    public Button tutorialButton;

    [Header("Boutons Secondaires")]
    public Button changeAvatarButton;
    public Button changeCountryButton;
    public Button changeLanguageButton;

    [Header("Panneaux")]
    public GameObject mainPanel;
    public GameObject loadingPanel;
    public GameObject dailyRewardPanel;
    public GameObject newsPanel;

    [Header("Status En Ligne")]
    public GameObject onlineIndicator;
    public TextMeshProUGUI onlineStatusText;
    public Image onlineStatusIcon;
    public Color onlineColor = Color.green;
    public Color offlineColor = Color.red;

    [Header("Animations")]
    public Animator menuAnimator;
    public bool animateOnStart = true;
    public float buttonAnimationDelay = 0.1f;

    [Header("Sons")]
    public AudioClip buttonClickSound;
    public AudioClip menuOpenSound;
    public AudioClip notificationSound;

    [Header("Notifications")]
    public GameObject notificationBadge;
    public TextMeshProUGUI notificationCountText;

    [Header("Version")]
    public TextMeshProUGUI versionText;
    public string gameVersion = "1.0.0";

    [Header("Debug")]
    public bool showDebugLogs = true;

    [Header("Événements")]
    public UnityEngine.Events.UnityEvent onMenuOpened;
    public UnityEngine.Events.UnityEvent<string> onNavigate;

    private AudioSource audioSource;
    private string currentLanguage = "fr";
    private bool isInitialized = false;

    void Start()
    {
        InitializeMenu();
    }

    void InitializeMenu()
    {
        if (showDebugLogs)
        {
            Debug.Log("========================================");
            Debug.Log("🎮 INITIALISATION DU MENU PRINCIPAL");
            Debug.Log("========================================");
        }

        // Setup Audio
        SetupAudio();

        // Charger la langue
        currentLanguage = PlayerPrefs.GetString("GameLanguage", "fr");
        if (showDebugLogs) Debug.Log($"✓ Langue: {currentLanguage}");

        // Afficher l'état des PlayerPrefs
        if (showDebugLogs)
        {
            PlayerPrefsManager.Instance.PrintAllPlayerPrefs();
        }

        // Setup UI
        SetupButtons();
        UpdateUI();
        CheckOnlineStatus();
        ShowVersion();

        // Animations
        if (animateOnStart)
        {
            StartCoroutine(PlayOpeningAnimation());
        }

        // Vérifier les récompenses quotidiennes
        CheckDailyRewards();

        // Vérifier les notifications
        CheckNotifications();

        isInitialized = true;

        if (showDebugLogs)
        {
            Debug.Log("✅ Menu principal initialisé");
            Debug.Log("========================================\n");
        }

        // Déclencher l'événement
        onMenuOpened?.Invoke();

        // Jouer le son d'ouverture
        PlaySound(menuOpenSound);
    }

    void SetupAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    void SetupButtons()
    {
        // Boutons principaux
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
            AddButtonAnimation(playButton.gameObject, 0);
        }

        if (leaderboardButton != null)
        {
            leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
            AddButtonAnimation(leaderboardButton.gameObject, 1);
        }

        if (profileButton != null)
        {
            profileButton.onClick.AddListener(OnProfileClicked);
            AddButtonAnimation(profileButton.gameObject, 2);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
            AddButtonAnimation(settingsButton.gameObject, 3);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
            AddButtonAnimation(quitButton.gameObject, 4);
        }

        if (tutorialButton != null)
        {
            tutorialButton.onClick.AddListener(OnTutorialClicked);
            AddButtonAnimation(tutorialButton.gameObject, 5);
        }

        // Boutons secondaires
        if (changeAvatarButton != null)
            changeAvatarButton.onClick.AddListener(OnChangeAvatarClicked);

        if (changeCountryButton != null)
            changeCountryButton.onClick.AddListener(OnChangeCountryClicked);

        if (changeLanguageButton != null)
            changeLanguageButton.onClick.AddListener(OnChangeLanguageClicked);
    }

    void AddButtonAnimation(GameObject button, int index)
    {
        if (button == null) return;

        // Animation d'apparition avec délai
        button.transform.localScale = Vector3.zero;
        LeanTween.scale(button, Vector3.one, 0.5f)
            .setDelay(index * buttonAnimationDelay)
            .setEaseOutBack();

        // Effet de hover
        Button btn = button.GetComponent<Button>();
        if (btn != null)
        {
            var trigger = button.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
            {
                trigger = button.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }

            var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
            };
            pointerEnter.callback.AddListener((data) => OnButtonHover(button));
            trigger.triggers.Add(pointerEnter);

            var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit
            };
            pointerExit.callback.AddListener((data) => OnButtonExit(button));
            trigger.triggers.Add(pointerExit);
        }
    }

    void OnButtonHover(GameObject button)
    {
        LeanTween.scale(button, Vector3.one * 1.1f, 0.2f).setEaseOutQuad();
    }

    void OnButtonExit(GameObject button)
    {
        LeanTween.scale(button, Vector3.one, 0.2f).setEaseOutQuad();
    }

    void UpdateUI()
    {
        if (showDebugLogs)
        {
            Debug.Log("🔄 Mise à jour de l'UI du menu...");
        }

        // CORRECTION : Utiliser PlayerPrefsManager
        var prefsManager = PlayerPrefsManager.Instance;

        // Récupérer toutes les données
        string playerName = prefsManager.GetPlayerName();
        int avatarId = prefsManager.GetAvatarId();
        int countryId = prefsManager.GetCountryId();
        string countryName = prefsManager.GetCountryName();
        int totalScore = prefsManager.GetTotalScore();
        int highestLevel = prefsManager.GetHighestLevel();
        int gamesPlayed = prefsManager.GetGamesPlayed();
        int gamesWon = prefsManager.GetGamesWon();
        float winRate = prefsManager.GetWinRate();

        if (showDebugLogs)
        {
            Debug.Log($"📊 Données du joueur :");
            Debug.Log($"  • Nom: {playerName}");
            Debug.Log($"  • Avatar ID: {avatarId}");
            Debug.Log($"  • Pays ID: {countryId} ({countryName})");
            Debug.Log($"  • Score: {totalScore}");
            Debug.Log($"  • Niveau: {highestLevel}");
            Debug.Log($"  • Parties: {gamesPlayed}");
            Debug.Log($"  • WinRate: {winRate:F1}%");
        }

        // Nom du joueur
        if (playerNameText != null)
        {
            playerNameText.text = playerName;
        }

        // Message de bienvenue
        if (welcomeText != null)
        {
            welcomeText.text = GetLocalizedText("welcome") + ", " + playerName + "!";
        }

        // Avatar
        UpdateAvatar(avatarId);

        // Drapeau
        UpdateFlag(countryId);

        // Score
        if (playerScoreText != null)
        {
            playerScoreText.text = $"{GetLocalizedText("score")}: {FormatNumber(totalScore)}";
            AnimateNumber(playerScoreText, 0, totalScore, 1.5f);
        }

        // Niveau
        if (playerLevelText != null)
        {
            playerLevelText.text = $"{GetLocalizedText("level")} {highestLevel}";
        }

        // Rang (si vous avez un système de classement local)
        UpdateRank();

        // Statistiques
        UpdateStats(gamesPlayed, winRate);

        // Mettre à jour les textes des boutons
        UpdateButtonTexts();

        if (showDebugLogs)
        {
            Debug.Log("✅ UI mise à jour avec succès");
        }
    }

    void UpdateAvatar(int avatarId)
    {
        if (playerAvatarImage == null) return;

        if (avatarId <= 0)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning($"⚠️ Avatar ID invalide: {avatarId}");
            }
            playerAvatarImage.gameObject.SetActive(false);
            return;
        }

        // Charger l'avatar depuis Resources/Avatars/
        Sprite avatarSprite = Resources.Load<Sprite>($"Avatars/{avatarId}");

        if (avatarSprite != null)
        {
            playerAvatarImage.sprite = avatarSprite;
            playerAvatarImage.gameObject.SetActive(true);

            if (showDebugLogs)
            {
                Debug.Log($"✅ Avatar {avatarId} chargé et affiché");
            }

            // Animation de rotation pour l'avatar
            LeanTween.rotateZ(playerAvatarImage.gameObject, 360f, 20f)
                .setLoopClamp()
                .setEaseInOutSine();
        }
        else
        {
            Debug.LogWarning($"⚠️ Avatar {avatarId} introuvable dans Resources/Avatars/");
            playerAvatarImage.gameObject.SetActive(false);
        }
    }

    void UpdateFlag(int countryId)
    {
        if (playerFlagImage == null) return;

        if (countryId <= 0)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning($"⚠️ Country ID invalide: {countryId}");
            }
            playerFlagImage.gameObject.SetActive(false);
            return;
        }

        // Charger le drapeau depuis Resources/Flags/
        Sprite flagSprite = Resources.Load<Sprite>($"Flags/{countryId}");

        if (flagSprite != null)
        {
            playerFlagImage.sprite = flagSprite;
            playerFlagImage.gameObject.SetActive(true);

            if (showDebugLogs)
            {
                Debug.Log($"✅ Drapeau {countryId} chargé et affiché");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ Drapeau {countryId} introuvable dans Resources/Flags/");
            playerFlagImage.gameObject.SetActive(false);
        }
    }

    void UpdateRank()
    {
        if (playerRankText == null) return;

        // OPTION 1 : Si vous avez un DataService qui calcule le rang
        var dataService = FindObjectOfType<DataService>();
        if (dataService != null)
        {
            int rank = dataService.GetCurrentPlayerRank();
            if (rank > 0)
            {
                playerRankText.text = $"{GetLocalizedText("rank")}: #{rank}";
            }
            else
            {
                playerRankText.text = GetLocalizedText("unranked");
            }
        }
        else
        {
            // OPTION 2 : Afficher un message par défaut
            playerRankText.text = GetLocalizedText("unranked");
        }
    }

    void UpdateStats(int gamesPlayed, float winRate)
    {
        if (statsPanel == null) return;

        statsPanel.SetActive(true);

        // Parties jouées
        if (gamesPlayedText != null)
        {
            gamesPlayedText.text = $"{GetLocalizedText("games_played")}: {gamesPlayed}";
        }

        // Taux de victoire
        if (winRateText != null)
        {
            winRateText.text = $"{GetLocalizedText("win_rate")}: {winRate:F1}%";

            // Couleur selon le taux
            if (winRate >= 70f)
                winRateText.color = Color.green;
            else if (winRate >= 50f)
                winRateText.color = Color.yellow;
            else
                winRateText.color = Color.red;
        }

        // Précision (si vous trackez cette stat)
        if (accuracyText != null)
        {
            // Pour l'instant, on peut utiliser le winRate comme approximation
            // ou récupérer depuis PlayerPrefs si vous avez cette donnée
            float accuracy = winRate; // Temporaire
            accuracyText.text = $"{GetLocalizedText("accuracy")}: {accuracy:F1}%";

            // Couleur selon la précision
            if (accuracy >= 80f)
                accuracyText.color = Color.green;
            else if (accuracy >= 60f)
                accuracyText.color = Color.yellow;
            else
                accuracyText.color = Color.red;
        }
    }

    void UpdateButtonTexts()
    {
        UpdateButtonText(playButton, "play");
        UpdateButtonText(leaderboardButton, "leaderboard");
        UpdateButtonText(profileButton, "profile");
        UpdateButtonText(settingsButton, "settings");
        UpdateButtonText(quitButton, "quit");
        UpdateButtonText(tutorialButton, "tutorial");
    }

    void UpdateButtonText(Button button, string key)
    {
        if (button == null) return;

        TextMeshProUGUI btnText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null)
        {
            btnText.text = GetLocalizedText(key);
        }
    }

    void CheckOnlineStatus()
    {
        var lootlocker = LootLockerService.Instance;
        bool isOnline = lootlocker != null && lootlocker.IsOnline();

        if (onlineIndicator != null)
        {
            onlineIndicator.SetActive(true);
        }

        if (onlineStatusText != null)
        {
            onlineStatusText.text = isOnline ? GetLocalizedText("online") : GetLocalizedText("offline");
            onlineStatusText.color = isOnline ? onlineColor : offlineColor;
        }

        if (onlineStatusIcon != null)
        {
            onlineStatusIcon.color = isOnline ? onlineColor : offlineColor;

            // Animation de pulsation si en ligne
            if (isOnline)
            {
                LeanTween.scale(onlineStatusIcon.gameObject, Vector3.one * 1.2f, 1f)
                    .setLoopPingPong()
                    .setEaseInOutSine();
            }
        }

        if (showDebugLogs)
        {
            Debug.Log($"✓ Status: {(isOnline ? "En ligne ✅" : "Hors ligne ⚠️")}");
        }
    }

    void CheckDailyRewards()
    {
        // Vérifier si une récompense quotidienne est disponible
        DateTime lastClaimed = DateTime.MinValue;

        if (PlayerPrefs.HasKey("LastDailyReward"))
        {
            try
            {
                long ticks = long.Parse(PlayerPrefs.GetString("LastDailyReward"));
                lastClaimed = new DateTime(ticks);
            }
            catch
            {
                lastClaimed = DateTime.MinValue;
            }
        }

        TimeSpan timeSinceLastClaim = DateTime.Now - lastClaimed;

        if (timeSinceLastClaim.TotalHours >= 24)
        {
            if (dailyRewardPanel != null)
            {
                StartCoroutine(ShowDailyRewardPanelDelayed());
            }
        }
    }

    IEnumerator ShowDailyRewardPanelDelayed()
    {
        yield return new WaitForSeconds(2f);

        if (dailyRewardPanel != null)
        {
            dailyRewardPanel.SetActive(true);
            PlaySound(notificationSound);

            // Animation d'apparition
            dailyRewardPanel.transform.localScale = Vector3.zero;
            LeanTween.scale(dailyRewardPanel, Vector3.one, 0.5f).setEaseOutBack();
        }
    }

    void CheckNotifications()
    {
        int notificationCount = 0;

        // Vérifier les nouveaux succès
        // Vérifier les messages
        // Vérifier les mises à jour du classement
        // etc.

        if (notificationCount > 0)
        {
            if (notificationBadge != null)
            {
                notificationBadge.SetActive(true);

                if (notificationCountText != null)
                {
                    notificationCountText.text = notificationCount.ToString();
                }

                // Animation de pulsation
                LeanTween.scale(notificationBadge, Vector3.one * 1.2f, 0.5f)
                    .setLoopPingPong()
                    .setEaseInOutSine();
            }
        }
        else
        {
            if (notificationBadge != null)
            {
                notificationBadge.SetActive(false);
            }
        }
    }

    void ShowVersion()
    {
        if (versionText != null)
        {
            versionText.text = $"v{gameVersion}";
        }
    }

    IEnumerator PlayOpeningAnimation()
    {
        if (menuAnimator != null)
        {
            menuAnimator.SetTrigger("Open");
        }

        // Fade in du panneau principal
        if (mainPanel != null)
        {
            CanvasGroup canvasGroup = mainPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = mainPanel.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 0f;
            LeanTween.alphaCanvas(canvasGroup, 1f, 0.5f);
        }

        yield return null;
    }

    void AnimateNumber(TextMeshProUGUI text, int start, int end, float duration)
    {
        StartCoroutine(AnimateNumberCoroutine(text, start, end, duration));
    }

    IEnumerator AnimateNumberCoroutine(TextMeshProUGUI text, int start, int end, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            int current = (int)Mathf.Lerp(start, end, progress);
            text.text = $"{GetLocalizedText("score")}: {FormatNumber(current)}";
            yield return null;
        }

        text.text = $"{GetLocalizedText("score")}: {FormatNumber(end)}";
    }

    string FormatNumber(int number)
    {
        if (number >= 1000000)
            return $"{number / 1000000f:F1}M";
        if (number >= 1000)
            return $"{number / 1000f:F1}K";
        return number.ToString();
    }

    // ========== NAVIGATION ==========

    void OnPlayClicked()
    {
        PlaySound(buttonClickSound);
        onNavigate?.Invoke("Game");
        LoadScene("Game");
    }

    void OnLeaderboardClicked()
    {
        PlaySound(buttonClickSound);
        onNavigate?.Invoke("Leaderboard");
        LoadScene("Leaderboard");
    }

    void OnProfileClicked()
    {
        PlaySound(buttonClickSound);
        onNavigate?.Invoke("Profile");
        LoadScene("Profile");
    }

    void OnSettingsClicked()
    {
        PlaySound(buttonClickSound);
        onNavigate?.Invoke("Settings");
        LoadScene("Settings");
    }

    void OnTutorialClicked()
    {
        PlaySound(buttonClickSound);
        onNavigate?.Invoke("Tutorial");
        LoadScene("Tutorial");
    }

    void OnQuitClicked()
    {
        PlaySound(buttonClickSound);
        StartCoroutine(QuitGameCoroutine());
    }

    void OnChangeAvatarClicked()
    {
        PlaySound(buttonClickSound);
        LoadScene("AvatarSelection");
    }

    void OnChangeCountryClicked()
    {
        PlaySound(buttonClickSound);
        LoadScene("CountrySelection");
    }

    void OnChangeLanguageClicked()
    {
        PlaySound(buttonClickSound);
        LoadScene("LanguageSelection");
    }

    IEnumerator QuitGameCoroutine()
    {
        // Animation de sortie
        if (mainPanel != null)
        {
            CanvasGroup canvasGroup = mainPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                LeanTween.alphaCanvas(canvasGroup, 0f, 0.5f);
            }
        }

        yield return new WaitForSeconds(0.5f);

        // Sauvegarder avant de quitter (optionnel, PlayerPrefs sauvegarde déjà)
        PlayerPrefs.Save();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void LoadScene(string sceneName)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        // Animation de transition
        if (mainPanel != null)
        {
            CanvasGroup canvasGroup = mainPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                LeanTween.alphaCanvas(canvasGroup, 0f, 0.3f).setOnComplete(() =>
                {
                    SceneManager.LoadScene(sceneName);
                });
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
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
            case "welcome":
                switch (currentLanguage)
                {
                    case "fr": return "Bienvenue";
                    case "en": return "Welcome";
                    case "ru": return "Добро пожаловать";
                    case "es": return "Bienvenido";
                    case "pt": return "Bem-vindo";
                    default: return "Welcome";
                }

            case "score": return currentLanguage == "fr" ? "Score" : "Score";
            case "level": return currentLanguage == "fr" ? "Niveau" : "Level";

            case "rank":
                switch (currentLanguage)
                {
                    case "fr": return "Rang";
                    case "en": return "Rank";
                    case "ru": return "Ранг";
                    case "es": return "Rango";
                    case "pt": return "Classificação";
                    default: return "Rank";
                }

            case "unranked":
                switch (currentLanguage)
                {
                    case "fr": return "Non classé";
                    case "en": return "Unranked";
                    case "ru": return "Без рейтинга";
                    case "es": return "Sin clasificar";
                    case "pt": return "Sem classificação";
                    default: return "Unranked";
                }

            case "games_played":
                switch (currentLanguage)
                {
                    case "fr": return "Parties jouées";
                    case "en": return "Games played";
                    case "ru": return "Игр сыграно";
                    case "es": return "Partidas jugadas";
                    case "pt": return "Jogos jogados";
                    default: return "Games played";
                }

            case "win_rate":
                switch (currentLanguage)
                {
                    case "fr": return "Taux de victoire";
                    case "en": return "Win rate";
                    case "ru": return "Процент побед";
                    case "es": return "Tasa de victoria";
                    case "pt": return "Taxa de vitória";
                    default: return "Win rate";
                }

            case "accuracy":
                switch (currentLanguage)
                {
                    case "fr": return "Précision";
                    case "en": return "Accuracy";
                    case "ru": return "Точность";
                    case "es": return "Precisión";
                    case "pt": return "Precisão";
                    default: return "Accuracy";
                }

            case "play":
                switch (currentLanguage)
                {
                    case "fr": return "Jouer";
                    case "en": return "Play";
                    case "ru": return "Играть";
                    case "es": return "Jugar";
                    case "pt": return "Jogar";
                    default: return "Play";
                }

            case "leaderboard":
                switch (currentLanguage)
                {
                    case "fr": return "Classement";
                    case "en": return "Leaderboard";
                    case "ru": return "Таблица лидеров";
                    case "es": return "Clasificación";
                    case "pt": return "Classificação";
                    default: return "Leaderboard";
                }

            case "profile":
                switch (currentLanguage)
                {
                    case "fr": return "Profil";
                    case "en": return "Profile";
                    case "ru": return "Профиль";
                    case "es": return "Perfil";
                    case "pt": return "Perfil";
                    default: return "Profile";
                }

            case "settings":
                switch (currentLanguage)
                {
                    case "fr": return "Paramètres";
                    case "en": return "Settings";
                    case "ru": return "Настройки";
                    case "es": return "Ajustes";
                    case "pt": return "Configurações";
                    default: return "Settings";
                }

            case "quit":
                switch (currentLanguage)
                {
                    case "fr": return "Quitter";
                    case "en": return "Quit";
                    case "ru": return "Выход";
                    case "es": return "Salir";
                    case "pt": return "Sair";
                    default: return "Quit";
                }

            case "tutorial":
                switch (currentLanguage)
                {
                    case "fr": return "Tutoriel";
                    case "en": return "Tutorial";
                    case "ru": return "Обучение";
                    case "es": return "Tutorial";
                    case "pt": return "Tutorial";
                    default: return "Tutorial";
                }

            case "online":
                switch (currentLanguage)
                {
                    case "fr": return "En ligne";
                    case "en": return "Online";
                    case "ru": return "В сети";
                    case "es": return "En línea";
                    case "pt": return "Online";
                    default: return "Online";
                }

            case "offline":
                switch (currentLanguage)
                {
                    case "fr": return "Hors ligne";
                    case "en": return "Offline";
                    case "ru": return "Не в сети";
                    case "es": return "Sin conexión";
                    case "pt": return "Offline";
                    default: return "Offline";
                }

            default:
                return key;
        }
    }

    // ========== MÉTHODES PUBLIQUES ==========

    public void RefreshUI()
    {
        if (showDebugLogs)
        {
            Debug.Log("🔄 Rafraîchissement de l'UI...");
        }

        UpdateUI();
        CheckOnlineStatus();
    }

    public void ShowNotification(string message)
    {
        Debug.Log($"📢 Notification: {message}");
        PlaySound(notificationSound);

        // TODO: Afficher une notification UI
    }

    public void ClaimDailyReward()
    {
        // Donner la récompense
        int rewardPoints = 100;

        // CORRECTION : Utiliser PlayerPrefsManager au lieu de DataService
        PlayerPrefsManager.Instance.AddScore(rewardPoints);

        // Sauvegarder la date
        PlayerPrefs.SetString("LastDailyReward", DateTime.Now.Ticks.ToString());
        PlayerPrefs.Save();

        // Fermer le panneau
        if (dailyRewardPanel != null)
        {
            LeanTween.scale(dailyRewardPanel, Vector3.zero, 0.3f).setOnComplete(() =>
            {
                dailyRewardPanel.SetActive(false);
            });
        }

        // Rafraîchir l'UI
        RefreshUI();

        if (showDebugLogs)
        {
            Debug.Log($"✅ Récompense quotidienne réclamée: +{rewardPoints} points");
        }
    }

    void OnApplicationPause(bool pause)
    {
        if (!pause && isInitialized)
        {
            // Rafraîchir quand on revient dans le jeu
            RefreshUI();
            CheckDailyRewards();
        }
    }

    // ========== MÉTHODES DE DEBUG (OPTIONNELLES) ==========

    [ContextMenu("Force Refresh UI")]
    public void ForceRefreshUI()
    {
        Debug.Log("🔄 Forcer le rafraîchissement de l'UI...");
        PlayerPrefsManager.Instance.PrintAllPlayerPrefs();
        RefreshUI();
    }

    [ContextMenu("Test Data - Set Avatar 1")]
    public void TestSetAvatar1()
    {
        PlayerPrefsManager.Instance.SetAvatarId(1);
        RefreshUI();
    }

    [ContextMenu("Test Data - Set Country 1")]
    public void TestSetCountry1()
    {
        PlayerPrefsManager.Instance.SetCountryId(1);
        PlayerPrefsManager.Instance.SetCountryName("France");
        RefreshUI();
    }

    [ContextMenu("Test Data - Add 1000 Score")]
    public void TestAdd1000Score()
    {
        PlayerPrefsManager.Instance.AddScore(1000);
        RefreshUI();
    }

    [ContextMenu("Print Current PlayerPrefs")]
    public void PrintPlayerPrefs()
    {
        PlayerPrefsManager.Instance.PrintAllPlayerPrefs();
    }
}
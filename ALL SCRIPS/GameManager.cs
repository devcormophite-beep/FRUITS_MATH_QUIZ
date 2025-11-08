using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelSettings
    {
        public int questionsTotal = 7;
        public int questionsToWinEasy = 5;
        public int questionsToWinMedium = 6;
        public int questionsToWinHard = 7;
        public float timePerQuestion = 30f;
        public int pointsPerCorrectAnswer = 5;
    }

    [System.Serializable]
    public class AdSettings
    {
        [Header("Activation des Publicités")]
        public int startShowingAdsFromLevel = 3;

        [Header("Gestion des Vies")]
        public int maxLives = 3;
        public int livesAfterInterstitial = 3;
        public int livesAfterRewardRefuse = 1;

        [Header("Compteurs de Publicités")]
        public int maxInterstitialsBeforeReward = 2;
    }

    [Header("Configuration")]
    public LevelSettings levelSettings;
    public AdSettings adSettings;

    [Header("Références UI")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI questionCountText;
    public Image[] livesImages;
    public Sprite heartFull;
    public Sprite heartEmpty;

    [Header("Panneaux")]
    public GameObject gamePanel;
    public GameObject correctAnswerPanel;
    public GameObject wrongAnswerPanel;
    public GameObject livesLostPanel;
    public GameObject rewardRequestPanel;
    public GameObject levelCompletedPanel;
    public GameObject levelFailedPanel;
    public GameObject levelTransitionPanel;

    [Header("Boutons")]
    public Button continueAfterCorrectBtn;
    public Button continueAfterWrongBtn;
    public Button collectLivesBtn;
    public Button watchRewardBtn;
    public Button refuseRewardBtn;
    public Button nextLevelBtn;
    public Button retryLevelBtn;

    [Header("Textes Panneaux")]
    public TextMeshProUGUI livesLostMessageText;
    public TextMeshProUGUI rewardRequestMessageText;
    public TextMeshProUGUI levelCompletedText;
    public TextMeshProUGUI levelTransitionText;

    [Header("Références")]
    public QuizGenerator quizGenerator;
    public QuizUIManager quizUIManager;
    public AdmobAdsScript adsManager;

    [Header("LootLocker")]
    public bool submitToLootLocker = true; // Soumettre les scores en ligne

    // Variables d'état
    private int currentLevel = 1;
    private int currentScore = 0;
    private int currentLives = 3;
    private int currentQuestionIndex = 0;
    private int correctAnswersInLevel = 0;
    private int wrongAnswersInLevel = 0;
    private float currentTimer;
    private bool isTimerRunning = false;
    private float totalTimeForLevel = 0f;

    private int interstitialShownCount = 0;
    private bool isInLevel = false;

    private int sessionStartScore = 0;
    private int totalCorrectAnswersSession = 0;
    private int totalWrongAnswersSession = 0;

    public enum Difficulty { Easy, Medium, Hard }
    private Difficulty currentDifficulty = Difficulty.Easy;

    void Start()
    {
        InitializeGame();
        SetupButtonListeners();
        LoadSavedProgress();
    }

    void Update()
    {
        if (isTimerRunning)
        {
            UpdateTimer();
        }
    }

    void InitializeGame()
    {
        currentLives = adSettings.maxLives;
        interstitialShownCount = 0;

        var dataService = DataService.Instance;
        if (dataService != null)
        {
            var player = dataService.GetCurrentPlayer();
            if (player != null)
            {
                currentScore = player.totalScore;
                sessionStartScore = currentScore;
                currentLevel = Mathf.Max(1, player.highestLevel);
            }
        }

        UpdateUI();
        StartNewLevel();
    }

    void LoadSavedProgress()
    {
        var dataService = DataService.Instance;
        if (dataService != null)
        {
            var player = dataService.GetCurrentPlayer();
            if (player != null)
            {
                Debug.Log($"✓ Progression: {player.playerName} - Score: {player.totalScore} - Niv: {player.highestLevel}");
            }
        }
    }

    void SetupButtonListeners()
    {
        continueAfterCorrectBtn.onClick.AddListener(OnContinueAfterCorrect);
        continueAfterWrongBtn.onClick.AddListener(OnContinueAfterWrong);
        collectLivesBtn.onClick.AddListener(OnCollectLives);
        watchRewardBtn.onClick.AddListener(OnWatchReward);
        refuseRewardBtn.onClick.AddListener(OnRefuseReward);
        nextLevelBtn.onClick.AddListener(OnNextLevel);
        retryLevelBtn.onClick.AddListener(OnRetryLevel);
    }

    void StartNewLevel()
    {
        currentQuestionIndex = 0;
        correctAnswersInLevel = 0;
        wrongAnswersInLevel = 0;
        totalTimeForLevel = 0f;
        isInLevel = true;

        if (currentLevel <= 3)
            currentDifficulty = Difficulty.Easy;
        else if (currentLevel <= 6)
            currentDifficulty = Difficulty.Medium;
        else
            currentDifficulty = Difficulty.Hard;

        ShowLevelTransition();
    }

    void ShowLevelTransition()
    {
        HideAllPanels();
        levelTransitionPanel.SetActive(true);
        levelTransitionText.text = $"Niveau {currentLevel}\nDifficulté: {currentDifficulty}\n{GetRequiredCorrectAnswers()}/{levelSettings.questionsTotal} bonnes réponses";

        StartCoroutine(TransitionToNextQuestion(2f));
    }

    IEnumerator TransitionToNextQuestion(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (currentLevel > 1 && ShouldShowAds())
        {
            ShowRewardForLevelTransition();
        }
        else
        {
            LoadNextQuestion();
        }
    }

    void ShowRewardForLevelTransition()
    {
        HideAllPanels();
        levelTransitionPanel.SetActive(true);
        levelTransitionText.text = "Regardez une publicité pour continuer!";

        if (adsManager != null)
        {
            adsManager.LoadRewardedAd();
            StartCoroutine(WaitAndShowReward());
        }
    }

    IEnumerator WaitAndShowReward()
    {
        yield return new WaitForSeconds(1f);
        if (adsManager != null) adsManager.ShowRewardedAd();
        yield return new WaitForSeconds(0.5f);
        LoadNextQuestion();
    }

    void LoadNextQuestion()
    {
        if (currentQuestionIndex >= levelSettings.questionsTotal)
        {
            EndLevel();
            return;
        }

        currentQuestionIndex++;
        quizGenerator.GenerateLevel(currentLevel);
        quizUIManager.DisplayCurrentQuestion();

        StartQuestionTimer();
        ShowGamePanel();
        UpdateUI();
    }

    void StartQuestionTimer()
    {
        currentTimer = levelSettings.timePerQuestion;
        isTimerRunning = true;
    }

    void StopQuestionTimer()
    {
        totalTimeForLevel += (levelSettings.timePerQuestion - currentTimer);
        isTimerRunning = false;
    }

    void UpdateTimer()
    {
        currentTimer -= Time.deltaTime;

        if (currentTimer <= 0)
        {
            currentTimer = 0;
            isTimerRunning = false;
            OnTimeUp();
        }

        UpdateTimerDisplay();
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTimer / 60);
        int seconds = Mathf.FloorToInt(currentTimer % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
        timerText.color = currentTimer <= 10f ? Color.red : Color.black;
    }

    void OnTimeUp()
    {
        Debug.Log("Temps écoulé!");
        OnWrongAnswer();
    }

    public void OnAnswerSubmitted(bool isCorrect)
    {
        StopQuestionTimer();
        if (isCorrect) OnCorrectAnswer();
        else OnWrongAnswer();
    }

    void OnCorrectAnswer()
    {
        correctAnswersInLevel++;
        totalCorrectAnswersSession++;

        int earnedPoints = Mathf.CeilToInt(levelSettings.pointsPerCorrectAnswer * currentTimer);
        currentScore += earnedPoints;

        // Sauvegarder localement
        var dataService = DataService.Instance;
        if (dataService != null)
        {
            dataService.AddScore(earnedPoints);
        }

        // NOUVEAU: Soumettre à LootLocker immédiatement
        if (submitToLootLocker)
        {
            SubmitScoreToLootLocker(currentScore);
        }

        UpdateUI();
        ShowCorrectAnswerPanel(earnedPoints);
    }

    void OnWrongAnswer()
    {
        wrongAnswersInLevel++;
        totalWrongAnswersSession++;

        LoseLife();
        ShowWrongAnswerPanel();
    }

    void ShowCorrectAnswerPanel(int points)
    {
        HideAllPanels();
        correctAnswerPanel.SetActive(true);

        TextMeshProUGUI messageText = correctAnswerPanel.transform.Find("MessageText").GetComponent<TextMeshProUGUI>();
        messageText.text = $"Bravo!\n+{points} points";
    }

    void ShowWrongAnswerPanel()
    {
        HideAllPanels();
        wrongAnswerPanel.SetActive(true);
    }

    void OnContinueAfterCorrect()
    {
        LoadNextQuestion();
    }

    void OnContinueAfterWrong()
    {
        if (currentLives > 0) LoadNextQuestion();
    }

    void LoseLife()
    {
        currentLives--;
        UpdateLivesDisplay();

        if (currentLives <= 0)
        {
            OnAllLivesLost();
        }
    }

    void OnAllLivesLost()
    {
        StopQuestionTimer();

        if (!ShouldShowAds())
        {
            ShowLevelFailedPanel();
            return;
        }

        if (interstitialShownCount < adSettings.maxInterstitialsBeforeReward)
        {
            ShowInterstitialAndRecoverLives();
        }
        else
        {
            ShowRewardRequestPanel();
        }
    }

    void ShowInterstitialAndRecoverLives()
    {
        interstitialShownCount++;

        if (adsManager != null && ShouldShowAds())
        {
            adsManager.LoadInterstitialAd();
            StartCoroutine(WaitAndShowInterstitial());
        }
        else
        {
            ShowLivesRecoveryPanel();
        }
    }

    IEnumerator WaitAndShowInterstitial()
    {
        yield return new WaitForSeconds(1f);
        if (adsManager != null) adsManager.ShowInterstitialAd();
        yield return new WaitForSeconds(0.5f);
        ShowLivesRecoveryPanel();
    }

    void ShowLivesRecoveryPanel()
    {
        HideAllPanels();
        livesLostPanel.SetActive(true);
        livesLostMessageText.text = $"Vous récupérez {adSettings.livesAfterInterstitial} vies!";
    }

    void OnCollectLives()
    {
        currentLives = adSettings.livesAfterInterstitial;
        UpdateLivesDisplay();
        LoadNextQuestion();
    }

    void ShowRewardRequestPanel()
    {
        HideAllPanels();
        rewardRequestPanel.SetActive(true);
        rewardRequestMessageText.text = "Regardez une publicité pour 3 vies\nou refusez et recommencez avec 1 vie";
    }

    void OnWatchReward()
    {
        if (adsManager != null)
        {
            adsManager.LoadRewardedAd();
            StartCoroutine(WaitAndShowRewardForLives());
        }
    }

    IEnumerator WaitAndShowRewardForLives()
    {
        yield return new WaitForSeconds(1f);
        if (adsManager != null) adsManager.ShowRewardedAd();
        yield return new WaitForSeconds(0.5f);

        currentLives = adSettings.livesAfterInterstitial;
        interstitialShownCount = 0;
        UpdateLivesDisplay();
        LoadNextQuestion();
    }

    void OnRefuseReward()
    {
        if (adsManager != null && ShouldShowAds())
        {
            adsManager.LoadInterstitialAd();
            StartCoroutine(WaitAndShowInterstitialAfterRefuse());
        }
        else
        {
            RecoverOneLifeAndRetry();
        }
    }

    IEnumerator WaitAndShowInterstitialAfterRefuse()
    {
        yield return new WaitForSeconds(1f);
        if (adsManager != null) adsManager.ShowInterstitialAd();
        yield return new WaitForSeconds(0.5f);
        RecoverOneLifeAndRetry();
    }

    void RecoverOneLifeAndRetry()
    {
        currentLives = adSettings.livesAfterRewardRefuse;
        interstitialShownCount = 0;
        UpdateLivesDisplay();
        OnRetryLevel();
    }

    void UpdateLivesDisplay()
    {
        for (int i = 0; i < livesImages.Length; i++)
        {
            livesImages[i].sprite = i < currentLives ? heartFull : heartEmpty;
        }
    }

    void EndLevel()
    {
        isInLevel = false;
        int requiredCorrect = GetRequiredCorrectAnswers();
        bool levelWon = correctAnswersInLevel >= requiredCorrect;

        float avgTime = totalTimeForLevel / levelSettings.questionsTotal;

        // Sauvegarder localement
        var dataService = DataService.Instance;
        if (dataService != null)
        {
            dataService.RecordGameResult(levelWon, correctAnswersInLevel, wrongAnswersInLevel, avgTime);

            if (levelWon)
            {
                dataService.UpdateLevel(currentLevel);
            }
        }

        // NOUVEAU: Soumettre le score final à LootLocker
        if (submitToLootLocker && levelWon)
        {
            SubmitScoreToLootLocker(currentScore, showNotification: true);
        }

        if (levelWon)
        {
            ShowLevelCompletedPanel();
        }
        else
        {
            ShowLevelFailedPanel();
        }
    }

    // ========== INTÉGRATION LOOTLOCKER ==========

    void SubmitScoreToLootLocker(int score, bool showNotification = false)
    {
        var lootlocker = LootLockerService.Instance;
        if (lootlocker == null || !lootlocker.IsOnline())
        {
            if (showNotification)
            {
                Debug.Log("📱 Mode hors ligne - score sauvegardé localement");
            }
            return;
        }

        lootlocker.SubmitScore(score, (success) =>
        {
            if (success)
            {
                Debug.Log($"✅ Score {score} envoyé à LootLocker!");

                if (showNotification)
                {
                    // Afficher une notification visuelle
                    ShowOnlineScoreNotification();
                }
            }
            else
            {
                Debug.LogWarning("⚠️ Échec envoi LootLocker, score sauvegardé localement");
            }
        });
    }

    void ShowOnlineScoreNotification()
    {
        // TODO: Afficher un petit popup "Score envoyé en ligne! 🌐"
        Debug.Log("🌐 Score synchronisé avec le leaderboard mondial!");
    }

    // ========== FIN DE NIVEAU ==========

    void ShowLevelCompletedPanel()
    {
        HideAllPanels();
        levelCompletedPanel.SetActive(true);

        var dataService = DataService.Instance;
        string rankInfo = "";

        if (dataService != null)
        {
            var player = dataService.GetCurrentPlayer();
            if (player != null)
            {
                rankInfo = $"\nRang: {player.GetRank()}";
            }
        }

        // Ajouter info si score en ligne
        string onlineInfo = "";
        var lootlocker = LootLockerService.Instance;
        if (lootlocker != null && lootlocker.IsOnline() && submitToLootLocker)
        {
            onlineInfo = "\n🌐 Score envoyé en ligne!";
        }

        levelCompletedText.text = $"Niveau {currentLevel} Complété! 🎉\n" +
                                   $"Score: {currentScore}{rankInfo}{onlineInfo}\n" +
                                   $"Bonnes réponses: {correctAnswersInLevel}/{levelSettings.questionsTotal}";
    }

    void ShowLevelFailedPanel()
    {
        HideAllPanels();
        levelFailedPanel.SetActive(true);

        TextMeshProUGUI failText = levelFailedPanel.transform.Find("MessageText").GetComponent<TextMeshProUGUI>();
        int required = GetRequiredCorrectAnswers();
        failText.text = $"Niveau Échoué 😢\n" +
                       $"Il fallait {required} bonnes réponses\n" +
                       $"Vous avez: {correctAnswersInLevel}/{levelSettings.questionsTotal}";
    }

    void OnNextLevel()
    {
        currentLevel++;
        interstitialShownCount = 0;
        StartNewLevel();
    }

    void OnRetryLevel()
    {
        interstitialShownCount = 0;
        StartNewLevel();
    }

    int GetRequiredCorrectAnswers()
    {
        return currentDifficulty switch
        {
            Difficulty.Easy => levelSettings.questionsToWinEasy,
            Difficulty.Medium => levelSettings.questionsToWinMedium,
            Difficulty.Hard => levelSettings.questionsToWinHard,
            _ => levelSettings.questionsToWinEasy
        };
    }

    bool ShouldShowAds()
    {
        return currentLevel >= adSettings.startShowingAdsFromLevel;
    }

    void UpdateUI()
    {
        levelText.text = $"Niveau {currentLevel}";
        scoreText.text = $"Score: {currentScore}";
        questionCountText.text = $"Question {currentQuestionIndex}/{levelSettings.questionsTotal}";
        UpdateLivesDisplay();
    }

    void ShowGamePanel()
    {
        HideAllPanels();
        gamePanel.SetActive(true);
    }

    void HideAllPanels()
    {
        gamePanel.SetActive(false);
        correctAnswerPanel.SetActive(false);
        wrongAnswerPanel.SetActive(false);
        livesLostPanel.SetActive(false);
        rewardRequestPanel.SetActive(false);
        levelCompletedPanel.SetActive(false);
        levelFailedPanel.SetActive(false);
        levelTransitionPanel.SetActive(false);
    }

    public int GetCurrentLevel() => currentLevel;
    public int GetCurrentScore() => currentScore;
    public float GetCurrentTimer() => currentTimer;
    public bool IsTimerRunning() => isTimerRunning;

    void OnApplicationQuit()
    {
        SaveGameProgress();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause) SaveGameProgress();
    }

    void SaveGameProgress()
    {
        var dataService = DataService.Instance;
        if (dataService != null)
        {
            dataService.SaveCurrentPlayer();
            Debug.Log("✓ Progression sauvegardée");
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Script pour tester l'envoi de scores fictifs à LootLocker
/// Attachez ce script à un GameObject dans votre scène
/// </summary>
public class TestSubmitScores : MonoBehaviour
{
    [Header("UI (Optionnel)")]
    public Button submitSingleScoreBtn;
    public Button submit10ScoresBtn;
    public Button submit50ScoresBtn;
    public TMP_InputField customScoreInput;
    public TextMeshProUGUI statusText;

    [Header("Configuration")]
    public int minScore = 100;
    public int maxScore = 10000;

    void Start()
    {
        // Setup des boutons si présents
        if (submitSingleScoreBtn != null)
            submitSingleScoreBtn.onClick.AddListener(SubmitSingleRandomScore);

        if (submit10ScoresBtn != null)
            submit10ScoresBtn.onClick.AddListener(() => StartCoroutine(SubmitMultipleScores(10)));

        if (submit50ScoresBtn != null)
            submit50ScoresBtn.onClick.AddListener(() => StartCoroutine(SubmitMultipleScores(50)));
    }

    void Update()
    {
        // Raccourcis clavier pour tests rapides
        if (Input.GetKeyDown(KeyCode.T))
        {
            SubmitSingleRandomScore();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            StartCoroutine(SubmitMultipleScores(10));
        }
    }

    // ========== MÉTHODE 1: Score Aléatoire Simple ==========

    [ContextMenu("Envoyer 1 Score Aléatoire")]
    public void SubmitSingleRandomScore()
    {
        int randomScore = Random.Range(minScore, maxScore);
        SubmitTestScore(randomScore);
    }

    // ========== MÉTHODE 2: Score Personnalisé ==========

    public void SubmitCustomScore()
    {
        if (customScoreInput == null) return;

        if (int.TryParse(customScoreInput.text, out int score))
        {
            SubmitTestScore(score);
        }
        else
        {
            UpdateStatus("❌ Score invalide!");
        }
    }

    // ========== MÉTHODE 3: Scores Multiples avec Délai ==========

    [ContextMenu("Envoyer 10 Scores Aléatoires")]
    public void SubmitMultipleRandomScores()
    {
        StartCoroutine(SubmitMultipleScores(10));
    }

    IEnumerator SubmitMultipleScores(int count)
    {
        UpdateStatus($"📤 Envoi de {count} scores...");

        for (int i = 0; i < count; i++)
        {
            int randomScore = Random.Range(minScore, maxScore);
            SubmitTestScore(randomScore, showNotification: false);

            UpdateStatus($"📤 Score {i + 1}/{count} envoyé...");

            // Attendre un peu entre chaque envoi pour éviter le rate limiting
            yield return new WaitForSeconds(0.5f);
        }

        UpdateStatus($"✅ {count} scores envoyés!");
        Debug.Log($"🎉 Terminé! {count} scores envoyés à LootLocker");
    }

    // ========== MÉTHODE 4: Scores avec Profils Complets ==========

    [ContextMenu("Envoyer Score avec Profil Complet")]
    public void SubmitScoreWithFullProfile()
    {
        // Créer un profil fictif
        string randomName = GetRandomName();
        int randomAvatar = Random.Range(1, 15);
        int randomCountry = Random.Range(1, 195);
        int randomLevel = Random.Range(1, 50);
        int randomScore = Random.Range(minScore, maxScore);

        Debug.Log("========================================");
        Debug.Log("👤 PROFIL FICTIF GÉNÉRÉ:");
        Debug.Log($"   Nom: {randomName}");
        Debug.Log($"   Avatar ID: {randomAvatar}");
        Debug.Log($"   Pays ID: {randomCountry}");
        Debug.Log($"   Niveau: {randomLevel}");
        Debug.Log($"   Score: {randomScore}");
        Debug.Log("========================================");

        // Créer les métadonnées
        var metadata = new PlayerMetadata
        {
            avatarId = randomAvatar,
            countryId = randomCountry,
            level = randomLevel,
            gamesWon = Random.Range(10, 100),
            gamesPlayed = Random.Range(20, 150),
            accuracy = Random.Range(50f, 100f)
        };

        string metadataJson = JsonUtility.ToJson(metadata);

        // Soumettre via LootLocker
        SubmitTestScore(randomScore, metadataJson, randomName);
    }

    // ========== MÉTHODE PRINCIPALE D'ENVOI ==========

    void SubmitTestScore(int score, string metadata = null, string playerName = null, bool showNotification = true)
    {
        var lootlocker = LootLockerService.Instance;

        if (lootlocker == null)
        {
            Debug.LogError("❌ LootLockerService introuvable!");
            UpdateStatus("❌ Service introuvable");
            return;
        }

        if (!lootlocker.IsOnline())
        {
            Debug.LogWarning("⚠️ LootLocker non connecté, tentative de connexion...");
            StartCoroutine(lootlocker.AuthenticatePlayer());
            
            // Réessayer après authentification
            StartCoroutine(RetryAfterAuth(score, metadata, playerName));
            return;
        }

        // Si pas de métadonnées fournies, utiliser celles du joueur actuel
        if (string.IsNullOrEmpty(metadata))
        {
            var dataService = DataService.Instance;
            if (dataService != null)
            {
                var player = dataService.GetCurrentPlayer();
                if (player != null)
                {
                    var meta = new PlayerMetadata
                    {
                        avatarId = player.avatarId,
                        countryId = player.countryId,
                        level = player.highestLevel,
                        gamesWon = player.gamesWon,
                        gamesPlayed = player.gamesPlayed,
                        accuracy = player.GetAccuracy()
                    };
                    metadata = JsonUtility.ToJson(meta);
                }
            }
        }

        // Si pas de nom fourni, utiliser le nom actuel
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = lootlocker.currentPlayerName;
        }

        Debug.Log($"📤 Envoi du score {score} à LootLocker...");
        if (showNotification)
        {
            UpdateStatus($"📤 Envoi de {score}...");
        }

        lootlocker.SubmitScore(score, (success) =>
        {
            if (success)
            {
                Debug.Log($"✅ Score {score} envoyé avec succès!");
                if (showNotification)
                {
                    UpdateStatus($"✅ Score {score} envoyé!");
                }
            }
            else
            {
                Debug.LogError($"❌ Échec envoi du score {score}");
                if (showNotification)
                {
                    UpdateStatus($"❌ Échec envoi {score}");
                }
            }
        });
    }

    IEnumerator RetryAfterAuth(int score, string metadata, string playerName)
    {
        yield return new WaitForSeconds(2f);
        SubmitTestScore(score, metadata, playerName);
    }

    // ========== MÉTHODE 5: Créer un Classement Complet ==========

    [ContextMenu("Créer un Classement Complet (100 joueurs)")]
    public void CreateFullLeaderboard()
    {
        StartCoroutine(CreateFullLeaderboardCoroutine());
    }

    IEnumerator CreateFullLeaderboardCoroutine()
    {
        Debug.Log("🏆 Création d'un classement complet...");
        UpdateStatus("🏆 Création du classement...");

        string[] names = GetAllNames();
        
        for (int i = 0; i < 100; i++)
        {
            string randomName = names[Random.Range(0, names.Length)];
            int randomScore = Random.Range(100, 50000);
            int randomAvatar = Random.Range(1, 15);
            int randomCountry = Random.Range(1, 195);
            int randomLevel = Random.Range(1, 50);

            var metadata = new PlayerMetadata
            {
                avatarId = randomAvatar,
                countryId = randomCountry,
                level = randomLevel,
                gamesWon = Random.Range(10, 200),
                gamesPlayed = Random.Range(20, 300),
                accuracy = Random.Range(40f, 100f)
            };

            string metadataJson = JsonUtility.ToJson(metadata);
            SubmitTestScore(randomScore, metadataJson, randomName, false);

            UpdateStatus($"🏆 {i + 1}/100 joueurs créés...");

            // Attendre pour éviter le rate limiting
            yield return new WaitForSeconds(0.3f);
        }

        UpdateStatus("✅ Classement créé!");
        Debug.Log("🎉 Classement de 100 joueurs créé avec succès!");
    }

    // ========== UTILITAIRES ==========

    void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log(message);
    }

    string GetRandomName()
    {
        string[] firstNames = { 
            "Alex", "Sophie", "Mohammed", "Maria", "Chen", "Yuki", 
            "Diego", "Emma", "Ivan", "Fatima", "Lucas", "Aisha",
            "Marco", "Nina", "Omar", "Léa", "Raj", "Ana", "Kim", "Tom",
            "Sarah", "Ahmed", "Olivia", "Carlos", "Mia", "Hassan",
            "Julia", "Pavel", "Elena", "Wei"
        };

        string[] lastNames = {
            "Smith", "Johnson", "Garcia", "Chen", "Patel", "Kim",
            "Rodriguez", "Silva", "Müller", "Ivanov", "Ahmed", "Ali",
            "Martinez", "Lopez", "Wang", "Lee", "Brown", "Wilson",
            "Anderson", "Thomas"
        };

        string firstName = firstNames[Random.Range(0, firstNames.Length)];
        string lastName = lastNames[Random.Range(0, lastNames.Length)];
        int number = Random.Range(10, 99);

        return $"{firstName}{lastName}{number}";
    }

    string[] GetAllNames()
    {
        return new string[]
        {
            "Alex", "Sophie", "Mohammed", "Maria", "Chen", "Yuki",
            "Diego", "Emma", "Ivan", "Fatima", "Lucas", "Aisha",
            "Marco", "Nina", "Omar", "Léa", "Raj", "Ana", "Kim", "Tom",
            "Sarah", "Ahmed", "Olivia", "Carlos", "Mia", "Hassan",
            "Julia", "Pavel", "Elena", "Wei", "John", "Alice", "Bob",
            "Charlie", "David", "Eve", "Frank", "Grace", "Henry", "Iris",
            "Jack", "Kate", "Liam", "Maya", "Noah", "Zoe", "Oscar", "Ruby"
        };
    }

    // ========== MÉTHODE 6: Via Menu Unity (Editor Only) ==========

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/LootLocker Test/Envoyer 1 Score Test")]
    static void MenuItem_SendOneScore()
    {
        var tester = FindObjectOfType<TestSubmitScores>();
        if (tester == null)
        {
            GameObject go = new GameObject("TestSubmitScores");
            tester = go.AddComponent<TestSubmitScores>();
        }
        tester.SubmitSingleRandomScore();
    }

    [UnityEditor.MenuItem("Tools/LootLocker Test/Envoyer 10 Scores Test")]
    static void MenuItem_Send10Scores()
    {
        var tester = FindObjectOfType<TestSubmitScores>();
        if (tester == null)
        {
            GameObject go = new GameObject("TestSubmitScores");
            tester = go.AddComponent<TestSubmitScores>();
        }
        tester.StartCoroutine(tester.SubmitMultipleScores(10));
    }

    [UnityEditor.MenuItem("Tools/LootLocker Test/Créer Classement Complet")]
    static void MenuItem_CreateFullLeaderboard()
    {
        var tester = FindObjectOfType<TestSubmitScores>();
        if (tester == null)
        {
            GameObject go = new GameObject("TestSubmitScores");
            tester = go.AddComponent<TestSubmitScores>();
        }
        tester.CreateFullLeaderboard();
    }
#endif
}

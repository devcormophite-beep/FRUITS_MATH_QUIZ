using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Gestionnaire de l'interface du Quiz
/// Version améliorée avec support du clavier personnalisé
/// </summary>
public class QuizUIManager : MonoBehaviour
{
    [Header("Zone des Équations")]
    public Transform equationsContainer;
    public GameObject equationPrefab;

    [Header("Zone de Réponse")]
    public Transform answerContainer;
    public GameObject answerItemPrefab;

    [Header("Boutons")]
    public Button validateButton;

    [Header("Clavier Personnalisé")]
    public CustomKeyboardManager customKeyboard;
    public bool useCustomKeyboard = true; // Activer/désactiver le clavier personnalisé

    [Header("Paramètres Visuels")]
    public float objectSize = 60f;
    public float answerObjectSize = 80f;
    public float symbolSize = 30f;
    public Sprite plusSymbolSprite;
    public Sprite equalsSymbolSprite;

    [Header("Couleurs de Feedback")]
    public Color correctAnswerColor = Color.green;
    public Color wrongAnswerColor = Color.red;
    public Color normalColor = Color.white;

    [Header("Références")]
    public QuizGenerator quizGenerator;
    public GameManager gameManager;

    [Header("Sons")]
    public AudioClip correctSound;
    public AudioClip incorrectSound;
    public AudioClip buttonClickSound;
    public AudioClip inputFocusSound;

    [Header("Animations")]
    public bool enableAnimations = true;
    public float animationDuration = 0.3f;

    private AudioSource audioSource;
    private Dictionary<string, TMP_InputField> answerInputs = new Dictionary<string, TMP_InputField>();
    private int totalAnswersNeeded = 0;
    private bool isValidating = false;

    void Start()
    {
        // Configurer l'AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Configurer le bouton de validation
        if (validateButton != null)
        {
            validateButton.onClick.AddListener(OnValidateClicked);
            validateButton.interactable = false;
        }
        else
        {
            Debug.LogError("❌ ValidateButton non assigné dans QuizUIManager!");
        }

        // Vérifier les références
        if (quizGenerator == null)
        {
            Debug.LogError("❌ QuizGenerator non assigné!");
        }

        if (gameManager == null)
        {
            Debug.LogError("❌ GameManager non assigné!");
        }

        if (useCustomKeyboard && customKeyboard == null)
        {
            Debug.LogWarning("⚠️ CustomKeyboard activé mais non assigné! Le clavier natif sera utilisé.");
            useCustomKeyboard = false;
        }
    }

    /// <summary>
    /// Affiche la question actuelle du quiz
    /// </summary>
    public void DisplayCurrentQuestion()
    {
        if (quizGenerator == null)
        {
            Debug.LogError("❌ QuizGenerator non initialisé!");
            return;
        }

        Debug.Log("========================================");
        Debug.Log("AFFICHAGE DE LA QUESTION");
        Debug.Log("========================================");

        // Nettoyer l'affichage précédent
        ClearContainers();
        answerInputs.Clear();
        totalAnswersNeeded = 0;
        isValidating = false;

        // Désactiver le bouton de validation
        if (validateButton != null)
        {
            validateButton.interactable = false;
        }

        // Récupérer les équations
        List<QuizGenerator.Equation> equations = quizGenerator.GetEquations();
        HashSet<string> uniqueObjectsInEquations = new HashSet<string>();

        if (equations == null || equations.Count == 0)
        {
            Debug.LogError("❌ Aucune équation générée!");
            return;
        }

        Debug.Log($"Nombre d'équations: {equations.Count}");

        // Afficher les équations
        foreach (var eq in equations)
        {
            // Collecter tous les objets uniques
            foreach (var term in eq.terms)
            {
                uniqueObjectsInEquations.Add(term.objectName);
            }

            CreateEquationDisplay(eq);
        }

        Debug.Log($"Objets uniques dans les équations: {uniqueObjectsInEquations.Count}");

        // Créer les zones de réponse
        Dictionary<string, int> correctValues = quizGenerator.GetObjectValues();

        if (correctValues == null || correctValues.Count == 0)
        {
            Debug.LogError("❌ Aucune valeur d'objet générée!");
            return;
        }

        foreach (var objName in uniqueObjectsInEquations)
        {
            if (correctValues.ContainsKey(objName))
            {
                CreateAnswerItem(objName);
                totalAnswersNeeded++;
            }
            else
            {
                Debug.LogWarning($"⚠️ Objet {objName} présent dans les équations mais pas dans les valeurs!");
            }
        }

        Debug.Log($"Total de réponses attendues: {totalAnswersNeeded}");
        Debug.Log("========================================\n");

        // Animation d'apparition
        if (enableAnimations)
        {
            AnimateQuestionsAppearance();
        }
    }

    /// <summary>
    /// Crée l'affichage d'une équation
    /// </summary>
    void CreateEquationDisplay(QuizGenerator.Equation equation)
    {
        if (equationPrefab == null)
        {
            Debug.LogError("❌ EquationPrefab non assigné!");
            return;
        }

        GameObject equationObj = Instantiate(equationPrefab, equationsContainer);
        Transform contentTransform = equationObj.transform.Find("Content");

        if (contentTransform == null)
        {
            Debug.LogError("❌ Le prefab d'équation doit contenir un objet 'Content'!");
            Destroy(equationObj);
            return;
        }

        // Créer les termes de l'équation
        for (int i = 0; i < equation.terms.Count; i++)
        {
            var term = equation.terms[i];

            // Créer l'image de l'objet
            CreateObjectImage(contentTransform, term, i);

            // Ajouter le symbole + entre les termes
            if (i < equation.terms.Count - 1)
            {
                CreatePlusSymbol(contentTransform, i);
            }
        }

        // Ajouter = et résultat
        AddEqualsAndResult(contentTransform, equation.result);
    }

    /// <summary>
    /// Crée l'image d'un objet dans l'équation
    /// </summary>
    void CreateObjectImage(Transform parent, QuizGenerator.Term term, int index)
    {
        GameObject imgObj = new GameObject($"Object_{index}");
        imgObj.transform.SetParent(parent);
        imgObj.transform.localScale = Vector3.one;
        imgObj.transform.localRotation = Quaternion.identity;

        Image img = imgObj.AddComponent<Image>();
        img.sprite = term.sprite;
        img.preserveAspect = true;

        RectTransform rect = imgObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(objectSize, objectSize);
        rect.localPosition = Vector3.zero;

        if (term.sprite == null)
        {
            Debug.LogWarning($"⚠️ Sprite null pour l'objet {term.objectName}");
        }
    }

    /// <summary>
    /// Crée le symbole + entre les termes
    /// </summary>
    void CreatePlusSymbol(Transform parent, int index)
    {
        GameObject plusObj = new GameObject($"Plus_{index}");
        plusObj.transform.SetParent(parent);
        plusObj.transform.localScale = Vector3.one;
        plusObj.transform.localRotation = Quaternion.identity;

        if (plusSymbolSprite != null)
        {
            // Utiliser le sprite personnalisé
            Image plusImg = plusObj.AddComponent<Image>();
            plusImg.sprite = plusSymbolSprite;
            plusImg.preserveAspect = true;

            RectTransform plusRect = plusObj.GetComponent<RectTransform>();
            plusRect.sizeDelta = new Vector2(symbolSize, symbolSize);
            plusRect.localPosition = Vector3.zero;
        }
        else
        {
            // Utiliser du texte
            TextMeshProUGUI plusText = plusObj.AddComponent<TextMeshProUGUI>();
            plusText.text = "+";
            plusText.fontSize = 150;
            plusText.alignment = TextAlignmentOptions.Center;
            plusText.color = Color.black;

            RectTransform plusRect = plusObj.GetComponent<RectTransform>();
            plusRect.sizeDelta = new Vector2(30, 50);
            plusRect.localPosition = Vector3.zero;
        }
    }

    /// <summary>
    /// Ajoute le symbole = et le résultat
    /// </summary>
    void AddEqualsAndResult(Transform parent, int result)
    {
        // Symbole =
        GameObject equalsObj = new GameObject("Equals");
        equalsObj.transform.SetParent(parent);
        equalsObj.transform.localScale = Vector3.one;
        equalsObj.transform.localRotation = Quaternion.identity;

        if (equalsSymbolSprite != null)
        {
            Image equalsImg = equalsObj.AddComponent<Image>();
            equalsImg.sprite = equalsSymbolSprite;
            equalsImg.preserveAspect = true;

            RectTransform equalsRect = equalsObj.GetComponent<RectTransform>();
            equalsRect.sizeDelta = new Vector2(symbolSize, symbolSize);
            equalsRect.localPosition = Vector3.zero;
        }
        else
        {
            TextMeshProUGUI equalsText = equalsObj.AddComponent<TextMeshProUGUI>();
            equalsText.text = "=";
            equalsText.fontSize = 150;
            equalsText.alignment = TextAlignmentOptions.Center;
            equalsText.color = Color.black;

            RectTransform equalsRect = equalsObj.GetComponent<RectTransform>();
            equalsRect.sizeDelta = new Vector2(30, 50);
            equalsRect.localPosition = Vector3.zero;
        }

        // Résultat
        GameObject resultObj = new GameObject("Result");
        resultObj.transform.SetParent(parent);
        resultObj.transform.localScale = Vector3.one;
        resultObj.transform.localRotation = Quaternion.identity;

        TextMeshProUGUI resultText = resultObj.AddComponent<TextMeshProUGUI>();
        resultText.text = result.ToString();
        resultText.fontSize = 150;
        resultText.alignment = TextAlignmentOptions.Center;
        resultText.color = Color.black;
        resultText.fontStyle = FontStyles.Bold;

        RectTransform resultRect = resultObj.GetComponent<RectTransform>();
        resultRect.sizeDelta = new Vector2(80, 60);
        resultRect.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Crée un item de réponse avec InputField
    /// </summary>
    void CreateAnswerItem(string objectName)
    {
        if (answerItemPrefab == null)
        {
            Debug.LogError("❌ AnswerItemPrefab non assigné!");
            return;
        }

        GameObject answerObj = Instantiate(answerItemPrefab, answerContainer);
        answerObj.name = $"Answer_{objectName}";

        // Récupérer les composants
        Image objectImage = answerObj.transform.Find("ObjectImage")?.GetComponent<Image>();
        TextMeshProUGUI equalText = answerObj.transform.Find("EqualText")?.GetComponent<TextMeshProUGUI>();
        TMP_InputField inputField = answerObj.transform.Find("InputField")?.GetComponent<TMP_InputField>();

        if (objectImage == null || equalText == null || inputField == null)
        {
            Debug.LogError("❌ Le prefab AnswerItem doit contenir : ObjectImage, EqualText, InputField");
            Destroy(answerObj);
            return;
        }

        // Configurer l'image de l'objet
        Sprite sprite = quizGenerator.GetSpriteForObject(objectName);

        if (sprite != null)
        {
            objectImage.sprite = sprite;
            objectImage.preserveAspect = true;
            RectTransform imgRect = objectImage.GetComponent<RectTransform>();
            imgRect.sizeDelta = new Vector2(answerObjectSize, answerObjectSize);
        }
        else
        {
            Debug.LogWarning($"⚠️ Sprite non trouvé pour {objectName}");
            objectImage.gameObject.SetActive(false);
        }

        // Configurer le texte =
        equalText.text = "=";
        equalText.fontSize = 150;
        equalText.alignment = TextAlignmentOptions.Center;
        equalText.color = Color.black;

        // Configurer l'InputField
        ConfigureInputField(inputField, objectName);

        // Stocker la référence
        answerInputs[objectName] = inputField;

        Debug.Log($"✓ Answer item créé pour: {objectName}");
    }

    /// <summary>
    /// Configure un InputField pour les réponses
    /// </summary>
    void ConfigureInputField(TMP_InputField inputField, string objectName)
    {
        inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        inputField.text = "";
        inputField.characterLimit = 3; // Maximum 3 chiffres (0-999)

        // ✅ INTÉGRATION DU CLAVIER PERSONNALISÉ
        if (useCustomKeyboard && customKeyboard != null)
        {
            // Désactiver le clavier natif
            inputField.shouldHideSoftKeyboard = true;
            inputField.shouldHideMobileInput = true;

            // Enregistrer dans le clavier personnalisé
            customKeyboard.RegisterInputField(objectName, inputField);

            // Événement de sélection
            inputField.onSelect.AddListener((text) =>
            {
                OnInputFieldSelected(inputField);
                PlaySound(inputFocusSound);
            });
        }
        else
        {
            // Mode clavier natif (fallback)
            inputField.shouldHideSoftKeyboard = false;
        }

        // Événement de changement de valeur
        inputField.onValueChanged.AddListener((value) =>
        {
            OnInputValueChanged(inputField, objectName, value);
        });

        // Désactiver le caret (curseur clignotant) si clavier personnalisé
        if (useCustomKeyboard)
        {
            inputField.caretWidth = 0;
        }

        Debug.Log($"✓ InputField configuré pour: {objectName}");
    }

    /// <summary>
    /// Événement quand un InputField est sélectionné
    /// </summary>
    void OnInputFieldSelected(TMP_InputField inputField)
    {
        if (useCustomKeyboard && customKeyboard != null)
        {
            customKeyboard.SetCurrentInputField(inputField);
        }

        // Effet visuel de focus
        Image bgImage = inputField.GetComponent<Image>();
        if (bgImage != null)
        {
            bgImage.color = new Color(1f, 1f, 0.8f, 1f); // Jaune clair
        }
    }

    /// <summary>
    /// Événement quand la valeur d'un InputField change
    /// </summary>
    void OnInputValueChanged(TMP_InputField inputField, string objectName, string value)
    {
        // Vérifier si tous les champs sont remplis
        CheckIfAllAnswersFilled();

        // Effet visuel
        Image bgImage = inputField.GetComponent<Image>();
        if (bgImage != null && !string.IsNullOrEmpty(value))
        {
            bgImage.color = new Color(0.9f, 1f, 0.9f, 1f); // Vert clair
        }

        Debug.Log($"Valeur changée pour {objectName}: {value}");
    }

    /// <summary>
    /// Vérifie si tous les champs de réponse sont remplis
    /// </summary>
    void CheckIfAllAnswersFilled()
    {
        if (validateButton == null) return;

        int filledAnswers = 0;

        foreach (var kvp in answerInputs)
        {
            if (kvp.Value != null && !string.IsNullOrEmpty(kvp.Value.text))
            {
                if (int.TryParse(kvp.Value.text, out _))
                {
                    filledAnswers++;
                }
            }
        }

        bool allFilled = (filledAnswers == totalAnswersNeeded);
        validateButton.interactable = allFilled;

        Debug.Log($"Réponses remplies: {filledAnswers}/{totalAnswersNeeded}");

        // Effet visuel sur le bouton
        if (allFilled && enableAnimations)
        {
            AnimateValidateButton();
        }
    }

    /// <summary>
    /// Événement du clic sur le bouton Valider
    /// </summary>
    void OnValidateClicked()
    {
        if (isValidating)
        {
            Debug.LogWarning("⚠️ Validation déjà en cours!");
            return;
        }

        isValidating = true;
        PlaySound(buttonClickSound);

        // Masquer le clavier personnalisé
        if (useCustomKeyboard && customKeyboard != null)
        {
            customKeyboard.HideKeyboard();
        }

        // Valider les réponses
        ValidateAnswers();
    }

    /// <summary>
    /// Valide toutes les réponses
    /// </summary>
    void ValidateAnswers()
    {
        Dictionary<string, int> correctValues = quizGenerator.GetObjectValues();
        bool allCorrect = true;

        Debug.Log("========================================");
        Debug.Log("VALIDATION DES RÉPONSES");
        Debug.Log("========================================");

        foreach (var kvp in answerInputs)
        {
            string objName = kvp.Key;
            TMP_InputField input = kvp.Value;

            if (input == null) continue;

            // Récupérer la réponse de l'utilisateur
            if (string.IsNullOrEmpty(input.text) || !int.TryParse(input.text, out int userAnswer))
            {
                Debug.LogWarning($"⚠️ Réponse invalide pour {objName}");
                allCorrect = false;
                HighlightInputField(input, false);
                continue;
            }

            // Vérifier la réponse
            if (!correctValues.ContainsKey(objName))
            {
                Debug.LogError($"❌ Objet {objName} non trouvé dans les valeurs correctes!");
                allCorrect = false;
                continue;
            }

            int correctAnswer = correctValues[objName];
            bool isCorrect = (userAnswer == correctAnswer);

            Debug.Log($"{objName}: {userAnswer} vs {correctAnswer} → {(isCorrect ? "✓" : "✗")}");

            if (!isCorrect)
            {
                allCorrect = false;
            }

            // Feedback visuel
            HighlightInputField(input, isCorrect);
        }

        Debug.Log($"Résultat final: {(allCorrect ? "CORRECT ✓" : "INCORRECT ✗")}");
        Debug.Log("========================================\n");

        // Jouer le son approprié
        if (allCorrect)
        {
            PlaySound(correctSound);
        }
        else
        {
            PlaySound(incorrectSound);
        }

        // Animation de feedback
        if (enableAnimations)
        {
            AnimateFeedback(allCorrect);
        }

        // Informer le GameManager après un court délai
        Invoke(nameof(NotifyGameManager), allCorrect ? 0.5f : 1f);
    }

    /// <summary>
    /// Notifie le GameManager du résultat
    /// </summary>
    void NotifyGameManager()
    {
        Dictionary<string, int> correctValues = quizGenerator.GetObjectValues();
        bool allCorrect = true;

        foreach (var kvp in answerInputs)
        {
            if (kvp.Value != null && int.TryParse(kvp.Value.text, out int userAnswer))
            {
                if (correctValues.ContainsKey(kvp.Key))
                {
                    if (userAnswer != correctValues[kvp.Key])
                    {
                        allCorrect = false;
                        break;
                    }
                }
            }
        }

        if (gameManager != null)
        {
            gameManager.OnAnswerSubmitted(allCorrect);
        }
        else
        {
            Debug.LogError("❌ GameManager non assigné!");
        }

        isValidating = false;
    }

    /// <summary>
    /// Met en évidence un InputField selon si la réponse est correcte
    /// </summary>
    void HighlightInputField(TMP_InputField inputField, bool isCorrect)
    {
        Image bgImage = inputField.GetComponent<Image>();
        if (bgImage != null)
        {
            Color targetColor = isCorrect ? correctAnswerColor : wrongAnswerColor;

            if (enableAnimations)
            {
                LeanTween.value(bgImage.gameObject, bgImage.color, targetColor, 0.3f)
                    .setOnUpdate((Color c) => { bgImage.color = c; });
            }
            else
            {
                bgImage.color = targetColor;
            }
        }

        // Animation de secousse si incorrect
        if (!isCorrect && enableAnimations)
        {
            ShakeInputField(inputField.gameObject);
        }
    }

    /// <summary>
    /// Animation de secousse pour un InputField incorrect
    /// </summary>
    void ShakeInputField(GameObject obj)
    {
        Vector3 originalPos = obj.transform.localPosition;

        LeanTween.moveLocalX(obj, originalPos.x + 10f, 0.05f).setLoopPingPong(4).setOnComplete(() =>
        {
            obj.transform.localPosition = originalPos;
        });
    }

    /// <summary>
    /// Animation du bouton Valider
    /// </summary>
    void AnimateValidateButton()
    {
        if (validateButton == null) return;

        GameObject btnObj = validateButton.gameObject;

        LeanTween.cancel(btnObj);
        LeanTween.scale(btnObj, Vector3.one * 1.1f, 0.5f)
            .setEaseInOutSine()
            .setLoopPingPong();
    }

    /// <summary>
    /// Animation de feedback général
    /// </summary>
    void AnimateFeedback(bool isCorrect)
    {
        // Animation des conteneurs
        if (isCorrect)
        {
            // Zoom in/out sur les réponses correctes
            foreach (var kvp in answerInputs)
            {
                if (kvp.Value != null)
                {
                    GameObject obj = kvp.Value.gameObject;
                    LeanTween.scale(obj, Vector3.one * 1.2f, 0.3f)
                        .setEaseOutBack()
                        .setLoopPingPong(1);
                }
            }
        }
    }

    /// <summary>
    /// Animation d'apparition des questions
    /// </summary>
    void AnimateQuestionsAppearance()
    {
        int index = 0;

        // Animer les équations
        foreach (Transform child in equationsContainer)
        {
            child.localScale = Vector3.zero;
            LeanTween.scale(child.gameObject, Vector3.one, animationDuration)
                .setDelay(index * 0.1f)
                .setEaseOutBack();
            index++;
        }

        // Animer les réponses
        index = 0;
        foreach (Transform child in answerContainer)
        {
            child.localScale = Vector3.zero;
            LeanTween.scale(child.gameObject, Vector3.one, animationDuration)
                .setDelay(index * 0.15f)
                .setEaseOutBack();
            index++;
        }
    }

    /// <summary>
    /// Nettoie les conteneurs
    /// </summary>
    void ClearContainers()
    {
        // Nettoyer les équations
        foreach (Transform child in equationsContainer)
        {
            Destroy(child.gameObject);
        }

        // Nettoyer les réponses
        foreach (Transform child in answerContainer)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("✓ Conteneurs nettoyés");
    }

    /// <summary>
    /// Joue un son
    /// </summary>
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // ========== MÉTHODES PUBLIQUES UTILES ==========

    /// <summary>
    /// Réinitialise toutes les réponses
    /// </summary>
    public void ClearAllAnswers()
    {
        foreach (var kvp in answerInputs)
        {
            if (kvp.Value != null)
            {
                kvp.Value.text = "";

                Image bgImage = kvp.Value.GetComponent<Image>();
                if (bgImage != null)
                {
                    bgImage.color = normalColor;
                }
            }
        }

        if (validateButton != null)
        {
            validateButton.interactable = false;
        }

        Debug.Log("✓ Toutes les réponses effacées");
    }

    /// <summary>
    /// Active/Désactive le clavier personnalisé
    /// </summary>
    public void SetCustomKeyboardEnabled(bool enabled)
    {
        useCustomKeyboard = enabled && customKeyboard != null;
        Debug.Log($"Clavier personnalisé: {(useCustomKeyboard ? "Activé" : "Désactivé")}");
    }

    /// <summary>
    /// Obtient le nombre de réponses correctes
    /// </summary>
    public int GetCorrectAnswersCount()
    {
        Dictionary<string, int> correctValues = quizGenerator.GetObjectValues();
        int correctCount = 0;

        foreach (var kvp in answerInputs)
        {
            if (kvp.Value != null && int.TryParse(kvp.Value.text, out int userAnswer))
            {
                if (correctValues.ContainsKey(kvp.Key) && userAnswer == correctValues[kvp.Key])
                {
                    correctCount++;
                }
            }
        }

        return correctCount;
    }

    void OnDisable()
    {
        // Nettoyer les animations en cours
        LeanTween.cancelAll();
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Gestionnaire de clavier personnalisé pour remplacer le clavier Android
/// Fonctionne avec le QuizUIManager existant
/// </summary>
public class CustomKeyboardManager : MonoBehaviour
{
    [Header("Panel du Clavier")]
    public GameObject keyboardPanel;
    public Button[] numberButtons; // Boutons 0-9
    public Button deleteButton;
    public Button clearButton;
    public Button validateButton;

    [Header("Références")]
    public QuizUIManager quizUIManager;

    [Header("Configuration")]
    public bool hideKeyboardOnStart = false;
    public int maxDigitsPerAnswer = 3; // Maximum 3 chiffres (0-999)

    [Header("Sons (Optionnel)")]
    public AudioClip numberClickSound;
    public AudioClip deleteSound;
    public AudioClip validateSound;

    private AudioSource audioSource;
    private TMP_InputField currentInputField;
    private Dictionary<string, TMP_InputField> allInputFields = new Dictionary<string, TMP_InputField>();

    void Start()
    {
        SetupAudio();
        SetupKeyboard();

        if (hideKeyboardOnStart)
        {
            HideKeyboard();
        }
    }

    void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void SetupKeyboard()
    {
        // Configurer les boutons numériques (0-9)
        for (int i = 0; i < numberButtons.Length && i < 10; i++)
        {
            int number = i; // Capture locale pour le delegate

            if (numberButtons[i] != null)
            {
                numberButtons[i].onClick.RemoveAllListeners();
                numberButtons[i].onClick.AddListener(() => OnNumberClicked(number));

                // Mettre le texte du bouton
                TextMeshProUGUI btnText = numberButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = number.ToString();
                }
            }
        }

        // Bouton Supprimer (Backspace)
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(OnDeleteClicked);
        }

        // Bouton Effacer Tout (Clear)
        if (clearButton != null)
        {
            clearButton.onClick.RemoveAllListeners();
            clearButton.onClick.AddListener(OnClearClicked);
        }

        // Bouton Valider
        if (validateButton != null)
        {
            validateButton.onClick.RemoveAllListeners();
            validateButton.onClick.AddListener(OnValidateClicked);
        }

        Debug.Log("✓ Clavier personnalisé configuré avec " + numberButtons.Length + " boutons");
    }

    /// <summary>
    /// Enregistrer un InputField pour le gérer avec le clavier personnalisé
    /// </summary>
    public void RegisterInputField(string objectName, TMP_InputField inputField)
    {
        if (inputField == null) return;

        // Désactiver le clavier natif
        inputField.shouldHideSoftKeyboard = true; // ← CLEF POUR DÉSACTIVER LE CLAVIER ANDROID
        inputField.readOnly = false; // Permettre la sélection mais pas la saisie directe

        // Empêcher l'affichage du clavier mobile
        inputField.onSelect.AddListener((text) => OnInputFieldSelected(inputField, objectName));

        // Désactiver le caret (curseur clignotant)
        inputField.caretWidth = 0;

        allInputFields[objectName] = inputField;

        Debug.Log($"✓ InputField '{objectName}' enregistré avec clavier personnalisé");
    }

    void OnInputFieldSelected(TMP_InputField inputField, string objectName)
    {
        currentInputField = inputField;
        ShowKeyboard();

        Debug.Log($"InputField sélectionné : {objectName}");

        // Forcer la fermeture du clavier natif si jamais il apparaît
        TouchScreenKeyboard.hideInput = true;
    }

    /// <summary>
    /// Clic sur un bouton numérique (0-9)
    /// </summary>
    void OnNumberClicked(int number)
    {
        if (currentInputField == null)
        {
            Debug.LogWarning("Aucun InputField sélectionné !");
            return;
        }

        string currentText = currentInputField.text;

        // Limiter le nombre de chiffres
        if (currentText.Length >= maxDigitsPerAnswer)
        {
            Debug.Log($"Maximum {maxDigitsPerAnswer} chiffres atteint");
            PlaySound(deleteSound);
            return;
        }

        // Ajouter le chiffre
        currentInputField.text = currentText + number.ToString();

        // Effet visuel
        AnimateButton(numberButtons[number]);
        PlaySound(numberClickSound);

        // Notifier le QuizUIManager si nécessaire
        if (quizUIManager != null)
        {
            // Le QuizUIManager vérifie automatiquement via onValueChanged
        }

        Debug.Log($"Chiffre ajouté : {number} → Texte : {currentInputField.text}");
    }

    /// <summary>
    /// Clic sur le bouton Supprimer (Backspace)
    /// </summary>
    void OnDeleteClicked()
    {
        if (currentInputField == null) return;

        string currentText = currentInputField.text;

        if (currentText.Length > 0)
        {
            // Supprimer le dernier caractère
            currentInputField.text = currentText.Substring(0, currentText.Length - 1);

            AnimateButton(deleteButton);
            PlaySound(deleteSound);

            Debug.Log($"Caractère supprimé → Texte : {currentInputField.text}");
        }
    }

    /// <summary>
    /// Clic sur le bouton Effacer Tout
    /// </summary>
    void OnClearClicked()
    {
        if (currentInputField == null) return;

        currentInputField.text = "";

        AnimateButton(clearButton);
        PlaySound(deleteSound);

        Debug.Log("Texte effacé");
    }

    /// <summary>
    /// Clic sur le bouton Valider
    /// </summary>
    void OnValidateClicked()
    {
        if (quizUIManager != null)
        {
            // Appeler la méthode de validation du QuizUIManager
            quizUIManager.validateButton.onClick.Invoke();

            PlaySound(validateSound);
            HideKeyboard();

            Debug.Log("Validation déclenchée");
        }
        else
        {
            Debug.LogError("QuizUIManager non assigné !");
        }
    }

    /// <summary>
    /// Afficher le clavier
    /// </summary>
    public void ShowKeyboard()
    {
        if (keyboardPanel != null)
        {
            keyboardPanel.SetActive(true);

            // Animation optionnelle
            keyboardPanel.transform.localScale = new Vector3(1f, 0f, 1f);
            LeanTween.scaleY(keyboardPanel, 1f, 0.3f).setEaseOutBack();
        }
    }

    /// <summary>
    /// Masquer le clavier
    /// </summary>
    public void HideKeyboard()
    {
        if (keyboardPanel != null)
        {
            LeanTween.scaleY(keyboardPanel, 0f, 0.2f).setEaseInBack().setOnComplete(() =>
            {
                keyboardPanel.SetActive(false);
            });
        }

        currentInputField = null;
    }

    /// <summary>
    /// Animation de bouton
    /// </summary>
    void AnimateButton(Button button)
    {
        if (button == null) return;

        GameObject btnObj = button.gameObject;

        // Reset scale si animation en cours
        LeanTween.cancel(btnObj);

        // Animation de pression
        btnObj.transform.localScale = Vector3.one;
        LeanTween.scale(btnObj, Vector3.one * 0.9f, 0.1f).setEaseInOutQuad().setLoopPingPong(1);
    }

    /// <summary>
    /// Jouer un son
    /// </summary>
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Effacer tous les InputFields
    /// </summary>
    public void ClearAllInputs()
    {
        foreach (var kvp in allInputFields)
        {
            if (kvp.Value != null)
            {
                kvp.Value.text = "";
            }
        }

        Debug.Log("Tous les InputFields effacés");
    }

    /// <summary>
    /// Désélectionner le champ actuel
    /// </summary>
    public void DeselectCurrentInput()
    {
        if (currentInputField != null)
        {
            currentInputField.DeactivateInputField();
            currentInputField = null;
        }

        HideKeyboard();
    }

    // ========== MÉTHODES PUBLIQUES UTILES ==========

    /// <summary>
    /// Obtenir le texte du champ actuel
    /// </summary>
    public string GetCurrentInputText()
    {
        return currentInputField != null ? currentInputField.text : "";
    }

    /// <summary>
    /// Définir le champ actif
    /// </summary>
    public void SetCurrentInputField(TMP_InputField inputField)
    {
        currentInputField = inputField;
        ShowKeyboard();
    }

    /// <summary>
    /// Vérifier si tous les champs sont remplis
    /// </summary>
    public bool AreAllInputsFilled()
    {
        foreach (var kvp in allInputFields)
        {
            if (kvp.Value != null)
            {
                if (string.IsNullOrEmpty(kvp.Value.text))
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Obtenir le nombre de champs remplis
    /// </summary>
    public int GetFilledInputsCount()
    {
        int count = 0;
        foreach (var kvp in allInputFields)
        {
            if (kvp.Value != null && !string.IsNullOrEmpty(kvp.Value.text))
            {
                count++;
            }
        }
        return count;
    }

    // ========== SUPPORT CLAVIER PHYSIQUE (PC) ==========

    void Update()
    {
        if (currentInputField == null) return;

        // Support du clavier physique pour tests sur PC
#if UNITY_EDITOR || UNITY_STANDALONE

        // Chiffres du clavier principal
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                OnNumberClicked(i);
            }
        }

        // Chiffres du pavé numérique
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
                OnNumberClicked(i);
            }
        }

        // Backspace
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            OnDeleteClicked();
        }

        // Escape pour masquer
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideKeyboard();
        }

        // Enter pour valider
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnValidateClicked();
        }

#endif
    }

    // ========== DÉSACTIVER LE CLAVIER NATIF (IMPORTANT!) ==========

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            // Forcer la fermeture du clavier natif quand on revient dans l'app
            TouchScreenKeyboard.hideInput = true;
        }
    }

    void OnEnable()
    {
        // S'assurer que le clavier natif est désactivé
        TouchScreenKeyboard.hideInput = true;
    }
}
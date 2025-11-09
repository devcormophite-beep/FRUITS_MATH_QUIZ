using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement; // ✅ AJOUTÉ

public class CountrySelector : MonoBehaviour
{
    [Header("Références UI")]
    public Transform countriesContainer;
    public GameObject countryItemPrefab;
    public ScrollRect scrollRect;
    public TMP_InputField searchField;
    public TextMeshProUGUI selectedCountryText;
    public Button confirmButton;

    [Header("Paramètres")]
    public float itemHeight = 80f;
    public Color selectedColor = Color.green;
    public Color normalColor = Color.white;
    public string nextSceneName = "MainMenu"; // ✅ AJOUTÉ

    [Header("Langue")]
    public string currentLanguage = "fr";
    public Button btnFrench;
    public Button btnEnglish;
    public Button btnRussian;
    public Button btnSpanish;
    public Button btnPortuguese;

    [Header("Événements")]
    public UnityEngine.Events.UnityEvent<string> onCountrySelected;

    private List<CountryData> allCountries = new List<CountryData>();
    private List<GameObject> countryItems = new List<GameObject>();
    private int selectedCountryId = -1;
    private string selectedCountryName = "";

    void Start()
    {
        if (PlayerPrefs.HasKey("GameLanguage"))
        {
            currentLanguage = PlayerPrefs.GetString("GameLanguage", "fr");
        }

        LoadCountriesData();
        CreateCountryItems();

        if (searchField != null)
        {
            searchField.onValueChanged.AddListener(OnSearchChanged);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
            confirmButton.interactable = false;
        }

        SetupLanguageButtons();
        LoadSavedCountry();
    }

    void SetupLanguageButtons()
    {
        if (btnFrench != null)
            btnFrench.onClick.AddListener(() => ChangeLanguage("fr"));

        if (btnEnglish != null)
            btnEnglish.onClick.AddListener(() => ChangeLanguage("en"));

        if (btnRussian != null)
            btnRussian.onClick.AddListener(() => ChangeLanguage("ru"));

        if (btnSpanish != null)
            btnSpanish.onClick.AddListener(() => ChangeLanguage("es"));

        if (btnPortuguese != null)
            btnPortuguese.onClick.AddListener(() => ChangeLanguage("pt"));
    }

    void ChangeLanguage(string langCode)
    {
        currentLanguage = langCode;
        PlayerPrefs.SetString("GameLanguage", langCode);
        PlayerPrefs.Save();

        Debug.Log($"✓ Langue changée: {langCode}");

        RefreshCountryItems();
    }

    void RefreshCountryItems()
    {
        int previousSelection = selectedCountryId;

        foreach (var item in countryItems)
        {
            Destroy(item);
        }
        countryItems.Clear();

        CreateCountryItems();

        if (previousSelection != -1)
        {
            var country = allCountries.Find(c => c.id == previousSelection);
            if (country != null)
            {
                var item = countryItems.Find(i => i.name.Contains($"Country_{country.id}_"));
                if (item != null)
                {
                    OnCountryClicked(country, item);
                }
            }
        }
    }

    void LoadCountriesData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Pays");

        if (jsonFile == null)
        {
            Debug.LogError("❌ Fichier Pays.json non trouvé dans Resources!");
            return;
        }

        try
        {
            CountryListWrapper wrapper = JsonUtility.FromJson<CountryListWrapper>(jsonFile.text);

            if (wrapper != null && wrapper.countries != null)
            {
                allCountries = wrapper.countries;
                allCountries = allCountries.OrderBy(c => c.GetName(currentLanguage)).ToList();

                Debug.Log($"✓ {allCountries.Count} pays chargés avec succès");

                if (allCountries.Count > 0)
                {
                    Debug.Log($"Premier pays: {allCountries[0].GetName(currentLanguage)} (ID: {allCountries[0].id})");
                }
            }
            else
            {
                Debug.LogError("❌ Erreur de parsing du JSON");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erreur lors du chargement du JSON: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }

    void CreateCountryItems()
    {
        if (allCountries.Count == 0)
        {
            Debug.LogWarning("⚠️ Aucun pays à afficher");
            return;
        }

        foreach (var country in allCountries)
        {
            CreateCountryItem(country);
        }
    }

    void CreateCountryItem(CountryData country)
    {
        GameObject item = Instantiate(countryItemPrefab, countriesContainer);

        Sprite flag = Resources.Load<Sprite>($"Flags/{country.id}");

        if (flag == null)
        {
            Debug.LogWarning($"⚠️ Drapeau non trouvé pour {country.GetName(currentLanguage)} (ID: {country.id})");
        }

        Image flagImage = item.transform.Find("FlagImage")?.GetComponent<Image>();
        TextMeshProUGUI nameText = item.transform.Find("CountryName")?.GetComponent<TextMeshProUGUI>();
        Button button = item.GetComponent<Button>();
        Image background = item.GetComponent<Image>();

        if (flagImage != null && flag != null)
        {
            flagImage.sprite = flag;
            flagImage.preserveAspect = true;
        }

        if (nameText != null)
        {
            nameText.text = country.GetName(currentLanguage);
        }

        if (button != null)
        {
            button.onClick.AddListener(() => OnCountryClicked(country, item));
        }

        item.name = $"Country_{country.id}";

        countryItems.Add(item);
    }

    void OnCountryClicked(CountryData country, GameObject item)
    {
        if (selectedCountryId != -1)
        {
            foreach (var countryItem in countryItems)
            {
                Image bg = countryItem.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = normalColor;
                }
            }
        }

        selectedCountryId = country.id;
        selectedCountryName = country.GetName(currentLanguage);

        Image background = item.GetComponent<Image>();
        if (background != null)
        {
            background.color = selectedColor;
        }

        if (selectedCountryText != null)
        {
            selectedCountryText.text = $"{GetLocalizedText("selected")}: {selectedCountryName}";
        }

        if (confirmButton != null)
        {
            confirmButton.interactable = true;
        }

        Debug.Log($"✓ Pays sélectionné: {selectedCountryName} (ID: {country.id})");
    }

    string GetLocalizedText(string key)
    {
        switch (key)
        {
            case "selected":
                switch (currentLanguage)
                {
                    case "fr": return "Pays sélectionné";
                    case "en": return "Selected country";
                    case "ru": return "Выбранная страна";
                    case "es": return "País seleccionado";
                    case "pt": return "País selecionado";
                    default: return "Selected country";
                }
            default:
                return key;
        }
    }

    void OnSearchChanged(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            foreach (var item in countryItems)
            {
                item.SetActive(true);
            }
            return;
        }

        string search = searchText.ToLower();

        for (int i = 0; i < countryItems.Count; i++)
        {
            var item = countryItems[i];
            var country = allCountries[i];

            bool matches = country.fr.ToLower().Contains(search) ||
                          country.en.ToLower().Contains(search) ||
                          country.ru.ToLower().Contains(search) ||
                          country.es.ToLower().Contains(search) ||
                          country.pt.ToLower().Contains(search);

            item.SetActive(matches);
        }
    }

    void OnConfirmClicked()
    {
        if (selectedCountryId == -1)
        {
            Debug.LogWarning("⚠️ Aucun pays sélectionné");
            return;
        }

        // Sauvegarder via PlayerPrefsManager
        PlayerPrefsManager.Instance.SetCountryId(selectedCountryId);
        PlayerPrefsManager.Instance.SetCountryName(selectedCountryName);

        Debug.Log($"✅ Pays confirmé et sauvegardé: {selectedCountryName}");
        Debug.Log($"→ Chargement de la scène: {nextSceneName}");

        // Déclencher l'événement
        onCountrySelected?.Invoke(selectedCountryName);

        // ✅ AJOUTÉ: Charger la scène suivante
        SceneManager.LoadScene(nextSceneName);
    }

    void LoadSavedCountry()
    {
        if (PlayerPrefs.HasKey("SelectedCountryId"))
        {
            int savedId = PlayerPrefs.GetInt("SelectedCountryId");
            string savedName = PlayerPrefs.GetString("SelectedCountryName", "");

            Debug.Log($"✓ Pays sauvegardé trouvé: {savedName} (ID: {savedId})");

            var savedCountry = allCountries.Find(c => c.id == savedId);
            if (savedCountry != null)
            {
                var item = countryItems.Find(i => i.name == $"Country_{savedId}");
                if (item != null)
                {
                    OnCountryClicked(savedCountry, item);
                }
            }
        }
    }

    // Méthodes publiques utiles
    public string GetSelectedCountryName()
    {
        return selectedCountryName;
    }

    public string GetSelectedCountryNameInLanguage(string langCode)
    {
        if (selectedCountryId == -1)
            return "";

        var country = allCountries.Find(c => c.id == selectedCountryId);
        return country != null ? country.GetName(langCode) : "";
    }

    public int GetSelectedCountryId()
    {
        return selectedCountryId;
    }

    public string GetCurrentLanguage()
    {
        return currentLanguage;
    }

    public void ClearSelection()
    {
        selectedCountryId = -1;
        selectedCountryName = "";

        foreach (var item in countryItems)
        {
            Image bg = item.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = normalColor;
            }
        }

        if (selectedCountryText != null)
        {
            selectedCountryText.text = "Aucun pays sélectionné";
        }

        if (confirmButton != null)
        {
            confirmButton.interactable = false;
        }
    }
}

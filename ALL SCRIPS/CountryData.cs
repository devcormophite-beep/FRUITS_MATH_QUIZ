using System;
using System.Collections.Generic;

[Serializable]
public class CountryData
{
    public int id;
    public string fr;  // Français
    public string en;  // Anglais
    public string ru;  // Russe
    public string es;  // Espagnol
    public string pt;  // Portugais

    public string GetName(string languageCode)
    {
        switch (languageCode.ToLower())
        {
            case "fr": return fr;
            case "en": return en;
            case "ru": return ru;
            case "es": return es;
            case "pt": return pt;
            default: return en; // Par défaut anglais
        }
    }
}

[Serializable]
public class CountryListWrapper
{
    public List<CountryData> countries;
}
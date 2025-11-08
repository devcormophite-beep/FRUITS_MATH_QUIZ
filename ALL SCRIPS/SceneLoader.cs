using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private float delaiEnSecondes = 3f;
    [SerializeField] private string nomDeLaScene = "NomDeLaScene";

    void Start()
    {
        // Lance le chargement de la scène après le délai spécifié
        Invoke("ChargerScene", delaiEnSecondes);
    }

    void ChargerScene()
    {
        SceneManager.LoadScene(nomDeLaScene);
    }
}
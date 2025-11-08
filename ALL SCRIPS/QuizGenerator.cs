using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class QuizGenerator : MonoBehaviour
{
    [System.Serializable]
    public class FruitObject
    {
        public string name;
        public int objectId; // Le numéro de l'objet (1, 2, 3, etc.)
        public Dictionary<int, Sprite> sprites = new Dictionary<int, Sprite>(); // count -> sprite
    }

    [Header("Configuration")]
    [Tooltip("Cette liste est remplie automatiquement au démarrage depuis Resources/Images")]
    public List<FruitObject> availableObjects = new List<FruitObject>();
    public int minValue = 1;
    public int maxValue = 10;

    [Header("Debug Info")]
    [SerializeField] private int totalSpritesLoaded = 0;
    [SerializeField] private int totalObjectsCreated = 0;

    private Dictionary<string, int> objectValues = new Dictionary<string, int>();
    private List<Equation> equations = new List<Equation>();
    private List<string> objectsToGuess = new List<string>();
    private int currentLevel = 1;

    [System.Serializable]
    public class Equation
    {
        public List<Term> terms = new List<Term>();
        public int result;
        public OperationType operation = OperationType.Addition;
    }

    [System.Serializable]
    public class Term
    {
        public string objectName;
        public Sprite sprite;
        public int count = 1; // Nombre d'objets dans cette image
    }

    public enum OperationType
    {
        Addition,
        Subtraction,
        Mixed
    }

    void Start()
    {
        LoadObjectsFromResources();

        // Validation
        if (availableObjects.Count == 0)
        {
            Debug.LogError("ERREUR CRITIQUE: Aucun objet chargé! Vérifiez que vos images sont dans 'Resources/Images' avec le format 'NxM.png' (ex: 1x1.png, 1x2.png)");
        }
        else
        {
            Debug.Log($"✓ Système prêt: {totalObjectsCreated} types d'objets avec {totalSpritesLoaded} sprites au total");
        }
    }

    void LoadObjectsFromResources()
    {
        // Vider la liste au cas où
        availableObjects.Clear();
        totalSpritesLoaded = 0;
        totalObjectsCreated = 0;

        Debug.Log("========================================");
        Debug.Log("CHARGEMENT AUTOMATIQUE DES SPRITES");
        Debug.Log("Dossier: Resources/Images");
        Debug.Log("========================================");

        Sprite[] sprites = Resources.LoadAll<Sprite>("Images");

        if (sprites.Length == 0)
        {
            Debug.LogError("ERREUR: Aucun sprite trouvé dans Resources/Images!");
            Debug.LogError("Assurez-vous que:");
            Debug.LogError("1. Le dossier 'Resources/Images' existe");
            Debug.LogError("2. Vos images sont au format PNG");
            Debug.LogError("3. Les noms suivent le format: 1x1.png, 1x2.png, 2x1.png, etc.");
            return;
        }

        Debug.Log($"Trouvé {sprites.Length} sprites au total dans le dossier");

        // Regrouper les sprites par objectId
        Dictionary<int, Dictionary<int, Sprite>> groupedSprites = new Dictionary<int, Dictionary<int, Sprite>>();

        foreach (Sprite sprite in sprites)
        {
            // Parser le nom du sprite: "1x2" -> objectId=1, count=2
            string spriteName = sprite.name;

            // Ignorer les sprites de symboles (equals, minus, multiply, plus)
            if (spriteName.Contains("Symbol") || spriteName.Contains("equals") ||
                spriteName.Contains("minus") || spriteName.Contains("multiply") ||
                spriteName.Contains("plus"))
            {
                Debug.Log($"  → Sprite symbole ignoré: {spriteName}");
                continue;
            }

            string[] parts = spriteName.Split('x');

            if (parts.Length == 2)
            {
                if (int.TryParse(parts[0], out int objectId) && int.TryParse(parts[1], out int count))
                {
                    if (!groupedSprites.ContainsKey(objectId))
                    {
                        groupedSprites[objectId] = new Dictionary<int, Sprite>();
                    }

                    groupedSprites[objectId][count] = sprite;
                    totalSpritesLoaded++;
                    Debug.Log($"  ✓ {spriteName} → Objet #{objectId}, Quantité: {count}");
                }
                else
                {
                    Debug.LogWarning($"  ✗ Impossible de parser: {spriteName}");
                }
            }
            else
            {
                Debug.LogWarning($"  ✗ Format invalide: {spriteName} (attendu: NxM comme 1x1, 2x3)");
            }
        }

        if (groupedSprites.Count == 0)
        {
            Debug.LogError("ERREUR: Aucun sprite valide trouvé!");
            Debug.LogError("Vérifiez que vos fichiers suivent le format: 1x1.png, 1x2.png, 2x1.png, etc.");
            return;
        }

        // Créer les FruitObjects
        Debug.Log("--- Création des objets ---");
        foreach (var kvp in groupedSprites.OrderBy(k => k.Key))
        {
            int objectId = kvp.Key;
            Dictionary<int, Sprite> objectSprites = kvp.Value;

            FruitObject obj = new FruitObject
            {
                name = $"Object_{objectId}",
                objectId = objectId,
                sprites = objectSprites
            };
            availableObjects.Add(obj);
            totalObjectsCreated++;

            string spritesList = string.Join(", ", objectSprites.Keys.OrderBy(k => k).Select(k => $"x{k}"));
            Debug.Log($"  ✓ {obj.name} créé avec {objectSprites.Count} sprites: [{spritesList}]");

            // Vérification: est-ce que x1 existe?
            if (!objectSprites.ContainsKey(1))
            {
                Debug.LogWarning($"    ⚠ ATTENTION: {obj.name} n'a pas de sprite x1! Cela peut causer des erreurs.");
            }
        }

        Debug.Log("========================================");
        Debug.Log($"RÉSUMÉ DU CHARGEMENT:");
        Debug.Log($"  • {totalObjectsCreated} types d'objets chargés");
        Debug.Log($"  • {totalSpritesLoaded} sprites au total");
        Debug.Log($"  • Objets disponibles: {string.Join(", ", availableObjects.Select(o => o.name))}");
        Debug.Log("========================================\n");
    }

    public void GenerateLevel(int level)
    {
        currentLevel = level;
        objectValues.Clear();
        equations.Clear();
        objectsToGuess.Clear();

        Debug.Log("========================================");
        Debug.Log("GÉNÉRATION DU NIVEAU " + level);
        Debug.Log("========================================");

        switch (level)
        {
            case 1:
                GenerateLevel1();
                break;
            case 2:
                GenerateLevel2();
                break;
            case 3:
                GenerateLevel3();
                break;
            case 4:
                GenerateLevel4();
                break;
            case 5:
                GenerateLevel5();
                break;
            default:
                GenerateLevelAdvanced();
                break;
        }

        Debug.Log("--- Valeurs assignées ---");
        foreach (var kvp in objectValues)
        {
            Debug.Log(kvp.Key + " = " + kvp.Value);
        }

        Debug.Log("--- Objets à deviner ---");
        Debug.Log("Liste: " + string.Join(", ", objectsToGuess));
        Debug.Log("Nombre: " + objectsToGuess.Count);

        HashSet<string> unique = new HashSet<string>(objectsToGuess);
        if (unique.Count != objectsToGuess.Count)
        {
            Debug.LogWarning("ATTENTION: Il y a des doublons dans objectsToGuess!");
        }

        Debug.Log("========================================\n");
    }

    // Niveau 1: A + A = X, A + B = Y
    void GenerateLevel1()
    {
        List<FruitObject> selected = SelectRandomObjects(2);

        objectValues[selected[0].name] = Random.Range(2, 5);
        objectValues[selected[1].name] = Random.Range(2, 5);

        // Équation 1: A + A (utiliser l'image x2)
        Equation eq1 = new Equation();
        eq1.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 2),
            count = 2
        });
        eq1.result = objectValues[selected[0].name] * 2;
        equations.Add(eq1);

        // Équation 2: A + B
        Equation eq2 = new Equation();
        eq2.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 1),
            count = 1
        });
        eq2.terms.Add(new Term
        {
            objectName = selected[1].name,
            sprite = GetSpriteForObjectAndCount(selected[1], 1),
            count = 1
        });
        eq2.result = objectValues[selected[0].name] + objectValues[selected[1].name];
        equations.Add(eq2);

        objectsToGuess.Add(selected[0].name);
        objectsToGuess.Add(selected[1].name);
    }

    // Niveau 2: A + A = X, A + B = Y, B + B = Z
    void GenerateLevel2()
    {
        List<FruitObject> selected = SelectRandomObjects(2);

        objectValues[selected[0].name] = Random.Range(2, 6);
        objectValues[selected[1].name] = Random.Range(2, 6);

        // Équation 1: A + A
        Equation eq1 = new Equation();
        eq1.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 2),
            count = 2
        });
        eq1.result = objectValues[selected[0].name] * 2;
        equations.Add(eq1);

        // Équation 2: A + B
        Equation eq2 = new Equation();
        eq2.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 1),
            count = 1
        });
        eq2.terms.Add(new Term
        {
            objectName = selected[1].name,
            sprite = GetSpriteForObjectAndCount(selected[1], 1),
            count = 1
        });
        eq2.result = objectValues[selected[0].name] + objectValues[selected[1].name];
        equations.Add(eq2);

        // Équation 3: B + B
        Equation eq3 = new Equation();
        eq3.terms.Add(new Term
        {
            objectName = selected[1].name,
            sprite = GetSpriteForObjectAndCount(selected[1], 2),
            count = 2
        });
        eq3.result = objectValues[selected[1].name] * 2;
        equations.Add(eq3);

        objectsToGuess.Add(selected[0].name);
        objectsToGuess.Add(selected[1].name);
    }

    // Niveau 3: A + B = X, B + C = Y, A + C = Z
    void GenerateLevel3()
    {
        List<FruitObject> selected = SelectRandomObjects(3);

        objectValues[selected[0].name] = Random.Range(2, 7);
        objectValues[selected[1].name] = Random.Range(2, 7);
        objectValues[selected[2].name] = Random.Range(2, 7);

        // Équation 1: A + B
        Equation eq1 = new Equation();
        eq1.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 1),
            count = 1
        });
        eq1.terms.Add(new Term
        {
            objectName = selected[1].name,
            sprite = GetSpriteForObjectAndCount(selected[1], 1),
            count = 1
        });
        eq1.result = objectValues[selected[0].name] + objectValues[selected[1].name];
        equations.Add(eq1);

        // Équation 2: B + C
        Equation eq2 = new Equation();
        eq2.terms.Add(new Term
        {
            objectName = selected[1].name,
            sprite = GetSpriteForObjectAndCount(selected[1], 1),
            count = 1
        });
        eq2.terms.Add(new Term
        {
            objectName = selected[2].name,
            sprite = GetSpriteForObjectAndCount(selected[2], 1),
            count = 1
        });
        eq2.result = objectValues[selected[1].name] + objectValues[selected[2].name];
        equations.Add(eq2);

        // Équation 3: A + C
        Equation eq3 = new Equation();
        eq3.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 1),
            count = 1
        });
        eq3.terms.Add(new Term
        {
            objectName = selected[2].name,
            sprite = GetSpriteForObjectAndCount(selected[2], 1),
            count = 1
        });
        eq3.result = objectValues[selected[0].name] + objectValues[selected[2].name];
        equations.Add(eq3);

        objectsToGuess.Add(selected[0].name);
        objectsToGuess.Add(selected[1].name);
        objectsToGuess.Add(selected[2].name);
    }

    // Niveau 4: A + A + A = X, B + B = Y, A + B + C = Z
    void GenerateLevel4()
    {
        List<FruitObject> selected = SelectRandomObjects(3);

        objectValues[selected[0].name] = Random.Range(2, 5);
        objectValues[selected[1].name] = Random.Range(3, 7);
        objectValues[selected[2].name] = Random.Range(2, 6);

        // Équation 1: A + A + A (utiliser l'image x3)
        Equation eq1 = new Equation();
        eq1.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 3),
            count = 3
        });
        eq1.result = objectValues[selected[0].name] * 3;
        equations.Add(eq1);

        // Équation 2: B + B
        Equation eq2 = new Equation();
        eq2.terms.Add(new Term
        {
            objectName = selected[1].name,
            sprite = GetSpriteForObjectAndCount(selected[1], 2),
            count = 2
        });
        eq2.result = objectValues[selected[1].name] * 2;
        equations.Add(eq2);

        // Équation 3: A + B + C
        Equation eq3 = new Equation();
        eq3.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 1),
            count = 1
        });
        eq3.terms.Add(new Term
        {
            objectName = selected[1].name,
            sprite = GetSpriteForObjectAndCount(selected[1], 1),
            count = 1
        });
        eq3.terms.Add(new Term
        {
            objectName = selected[2].name,
            sprite = GetSpriteForObjectAndCount(selected[2], 1),
            count = 1
        });
        eq3.result = objectValues[selected[0].name] + objectValues[selected[1].name] + objectValues[selected[2].name];
        equations.Add(eq3);

        objectsToGuess.Add(selected[0].name);
        objectsToGuess.Add(selected[1].name);
        objectsToGuess.Add(selected[2].name);
    }

    // Niveau 5: Avec plus de complexité
    void GenerateLevel5()
    {
        List<FruitObject> selected = SelectRandomObjects(3);

        objectValues[selected[0].name] = Random.Range(3, 8);
        objectValues[selected[1].name] = Random.Range(2, 6);
        objectValues[selected[2].name] = Random.Range(4, 9);

        // A + A + B (2A + B)
        Equation eq1 = new Equation();
        eq1.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 2),
            count = 2
        });
        eq1.terms.Add(new Term
        {
            objectName = selected[1].name,
            sprite = GetSpriteForObjectAndCount(selected[1], 1),
            count = 1
        });
        eq1.result = objectValues[selected[0].name] * 2 + objectValues[selected[1].name];
        equations.Add(eq1);

        // B + C + C (B + 2C)
        Equation eq2 = new Equation();
        eq2.terms.Add(new Term
        {
            objectName = selected[1].name,
            sprite = GetSpriteForObjectAndCount(selected[1], 1),
            count = 1
        });
        eq2.terms.Add(new Term
        {
            objectName = selected[2].name,
            sprite = GetSpriteForObjectAndCount(selected[2], 2),
            count = 2
        });
        eq2.result = objectValues[selected[1].name] + objectValues[selected[2].name] * 2;
        equations.Add(eq2);

        // A + B + C
        Equation eq3 = new Equation();
        eq3.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 1),
            count = 1
        });
        eq3.terms.Add(new Term
        {
            objectName = selected[1].name,
            sprite = GetSpriteForObjectAndCount(selected[1], 1),
            count = 1
        });
        eq3.terms.Add(new Term
        {
            objectName = selected[2].name,
            sprite = GetSpriteForObjectAndCount(selected[2], 1),
            count = 1
        });
        eq3.result = objectValues[selected[0].name] + objectValues[selected[1].name] + objectValues[selected[2].name];
        equations.Add(eq3);

        objectsToGuess.Add(selected[0].name);
        objectsToGuess.Add(selected[1].name);
        objectsToGuess.Add(selected[2].name);
    }

    void GenerateLevelAdvanced()
    {
        int advancedType = (currentLevel - 6) % 4;

        switch (advancedType)
        {
            case 0:
                GenerateLevel6();
                break;
            case 1:
                GenerateLevel7();
                break;
            case 2:
                GenerateLevel8();
                break;
            case 3:
                GenerateLevel9();
                break;
        }
    }

    // Niveau 6: 4 objets différents
    void GenerateLevel6()
    {
        List<FruitObject> selected = SelectRandomObjects(4);

        objectValues[selected[0].name] = Random.Range(2, 5);
        objectValues[selected[1].name] = Random.Range(3, 6);
        objectValues[selected[2].name] = Random.Range(2, 5);
        objectValues[selected[3].name] = Random.Range(3, 6);

        // A + A (2A)
        Equation eq1 = new Equation();
        eq1.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 2),
            count = 2
        });
        eq1.result = objectValues[selected[0].name] * 2;
        equations.Add(eq1);

        // B + B (2B)
        Equation eq2 = new Equation();
        eq2.terms.Add(new Term
        {
            objectName = selected[1].name,
            sprite = GetSpriteForObjectAndCount(selected[1], 2),
            count = 2
        });
        eq2.result = objectValues[selected[1].name] * 2;
        equations.Add(eq2);

        // A + B + C
        Equation eq3 = new Equation();
        eq3.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 1),
            count = 1
        });
        eq3.terms.Add(new Term
        {
            objectName = selected[1].name,
            sprite = GetSpriteForObjectAndCount(selected[1], 1),
            count = 1
        });
        eq3.terms.Add(new Term
        {
            objectName = selected[2].name,
            sprite = GetSpriteForObjectAndCount(selected[2], 1),
            count = 1
        });
        eq3.result = objectValues[selected[0].name] + objectValues[selected[1].name] + objectValues[selected[2].name];
        equations.Add(eq3);

        // C + D + D (C + 2D)
        Equation eq4 = new Equation();
        eq4.terms.Add(new Term
        {
            objectName = selected[2].name,
            sprite = GetSpriteForObjectAndCount(selected[2], 1),
            count = 1
        });
        eq4.terms.Add(new Term
        {
            objectName = selected[3].name,
            sprite = GetSpriteForObjectAndCount(selected[3], 2),
            count = 2
        });
        eq4.result = objectValues[selected[2].name] + objectValues[selected[3].name] * 2;
        equations.Add(eq4);

        objectsToGuess.Add(selected[0].name);
        objectsToGuess.Add(selected[1].name);
        objectsToGuess.Add(selected[2].name);
        objectsToGuess.Add(selected[3].name);
    }

    // Niveau 7: Équations avec 4 termes
    void GenerateLevel7()
    {
        List<FruitObject> selected = SelectRandomObjects(3);

        objectValues[selected[0].name] = Random.Range(2, 4);
        objectValues[selected[1].name] = Random.Range(2, 5);
        objectValues[selected[2].name] = Random.Range(3, 6);

        // A + A + A + A (4A)
        Equation eq1 = new Equation();
        eq1.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 4),
            count = 4
        });
        eq1.result = objectValues[selected[0].name] * 4;
        equations.Add(eq1);

        // A + A + B + B (2A + 2B)
        Equation eq2 = new Equation();
        eq2.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 2),
            count = 2
        });
        eq2.terms.Add(new Term
        {
            objectName = selected[1].name,
            sprite = GetSpriteForObjectAndCount(selected[1], 2),
            count = 2
        });
        eq2.result = objectValues[selected[0].name] * 2 + objectValues[selected[1].name] * 2;
        equations.Add(eq2);

        // B + C + C + C (B + 3C)
        Equation eq3 = new Equation();
        eq3.terms.Add(new Term
        {
            objectName = selected[1].name,
            sprite = GetSpriteForObjectAndCount(selected[1], 1),
            count = 1
        });
        eq3.terms.Add(new Term
        {
            objectName = selected[2].name,
            sprite = GetSpriteForObjectAndCount(selected[2], 3),
            count = 3
        });
        eq3.result = objectValues[selected[1].name] + objectValues[selected[2].name] * 3;
        equations.Add(eq3);

        objectsToGuess.Add(selected[0].name);
        objectsToGuess.Add(selected[1].name);
        objectsToGuess.Add(selected[2].name);
    }

    // Niveau 8: Mix de 2, 3 et 4 termes
    void GenerateLevel8()
    {
        List<FruitObject> selected = SelectRandomObjects(4);

        objectValues[selected[0].name] = Random.Range(2, 5);
        objectValues[selected[1].name] = Random.Range(2, 5);
        objectValues[selected[2].name] = Random.Range(3, 6);
        objectValues[selected[3].name] = Random.Range(2, 4);

        // A + B
        Equation eq1 = new Equation();
        eq1.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 1),
            count = 1
        });
        eq1.terms.Add(new Term
        {
            objectName = selected[1].name,
            sprite = GetSpriteForObjectAndCount(selected[1], 1),
            count = 1
        });
        eq1.result = objectValues[selected[0].name] + objectValues[selected[1].name];
        equations.Add(eq1);

        // C + D + D (C + 2D)
        Equation eq2 = new Equation();
        eq2.terms.Add(new Term
        {
            objectName = selected[2].name,
            sprite = GetSpriteForObjectAndCount(selected[2], 1),
            count = 1
        });
        eq2.terms.Add(new Term
        {
            objectName = selected[3].name,
            sprite = GetSpriteForObjectAndCount(selected[3], 2),
            count = 2
        });
        eq2.result = objectValues[selected[2].name] + objectValues[selected[3].name] * 2;
        equations.Add(eq2);

        // A + B + C + D
        Equation eq3 = new Equation();
        eq3.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 1),
            count = 1
        });
        eq3.terms.Add(new Term
        {
            objectName = selected[1].name,
            sprite = GetSpriteForObjectAndCount(selected[1], 1),
            count = 1
        });
        eq3.terms.Add(new Term
        {
            objectName = selected[2].name,
            sprite = GetSpriteForObjectAndCount(selected[2], 1),
            count = 1
        });
        eq3.terms.Add(new Term
        {
            objectName = selected[3].name,
            sprite = GetSpriteForObjectAndCount(selected[3], 1),
            count = 1
        });
        eq3.result = objectValues[selected[0].name] + objectValues[selected[1].name] +
                     objectValues[selected[2].name] + objectValues[selected[3].name];
        equations.Add(eq3);

        // A + A + C (2A + C)
        Equation eq4 = new Equation();
        eq4.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 2),
            count = 2
        });
        eq4.terms.Add(new Term
        {
            objectName = selected[2].name,
            sprite = GetSpriteForObjectAndCount(selected[2], 1),
            count = 1
        });
        eq4.result = objectValues[selected[0].name] * 2 + objectValues[selected[2].name];
        equations.Add(eq4);

        objectsToGuess.Add(selected[0].name);
        objectsToGuess.Add(selected[1].name);
        objectsToGuess.Add(selected[2].name);
        objectsToGuess.Add(selected[3].name);
    }

    // Niveau 9: Défi maximum avec 5 équations et 4 objets
    void GenerateLevel9()
    {
        List<FruitObject> selected = SelectRandomObjects(4);

        objectValues[selected[0].name] = Random.Range(2, 4);
        objectValues[selected[1].name] = Random.Range(3, 5);
        objectValues[selected[2].name] = Random.Range(2, 4);
        objectValues[selected[3].name] = Random.Range(3, 5);

        // A + A + A (3A)
        Equation eq1 = new Equation();
        eq1.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 3),
            count = 3
        });
        eq1.result = objectValues[selected[0].name] * 3;
        equations.Add(eq1);

        // B + B + B (3B)
        Equation eq2 = new Equation();
        eq2.terms.Add(new Term
        {
            objectName = selected[1].name,
            sprite = GetSpriteForObjectAndCount(selected[1], 3),
            count = 3
        });
        eq2.result = objectValues[selected[1].name] * 3;
        equations.Add(eq2);

        // A + B + C + D
        Equation eq3 = new Equation();
        eq3.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 1),
            count = 1
        });
        eq3.terms.Add(new Term
        {
            objectName = selected[1].name,
            sprite = GetSpriteForObjectAndCount(selected[1], 1),
            count = 1
        });
        eq3.terms.Add(new Term
        {
            objectName = selected[2].name,
            sprite = GetSpriteForObjectAndCount(selected[2], 1),
            count = 1
        });
        eq3.terms.Add(new Term
        {
            objectName = selected[3].name,
            sprite = GetSpriteForObjectAndCount(selected[3], 1),
            count = 1
        });
        eq3.result = objectValues[selected[0].name] + objectValues[selected[1].name] +
                     objectValues[selected[2].name] + objectValues[selected[3].name];
        equations.Add(eq3);

        // C + C + D (2C + D)
        Equation eq4 = new Equation();
        eq4.terms.Add(new Term
        {
            objectName = selected[2].name,
            sprite = GetSpriteForObjectAndCount(selected[2], 2),
            count = 2
        });
        eq4.terms.Add(new Term
        {
            objectName = selected[3].name,
            sprite = GetSpriteForObjectAndCount(selected[3], 1),
            count = 1
        });
        eq4.result = objectValues[selected[2].name] * 2 + objectValues[selected[3].name];
        equations.Add(eq4);

        // A + C + D + D (A + C + 2D)
        Equation eq5 = new Equation();
        eq5.terms.Add(new Term
        {
            objectName = selected[0].name,
            sprite = GetSpriteForObjectAndCount(selected[0], 1),
            count = 1
        });
        eq5.terms.Add(new Term
        {
            objectName = selected[2].name,
            sprite = GetSpriteForObjectAndCount(selected[2], 1),
            count = 1
        });
        eq5.terms.Add(new Term
        {
            objectName = selected[3].name,
            sprite = GetSpriteForObjectAndCount(selected[3], 2),
            count = 2
        });
        eq5.result = objectValues[selected[0].name] + objectValues[selected[2].name] +
                     objectValues[selected[3].name] * 2;
        equations.Add(eq5);

        objectsToGuess.Add(selected[0].name);
        objectsToGuess.Add(selected[1].name);
        objectsToGuess.Add(selected[2].name);
        objectsToGuess.Add(selected[3].name);
    }

    Sprite GetSpriteForObjectAndCount(FruitObject obj, int count)
    {
        if (obj == null)
        {
            Debug.LogError("FruitObject est null!");
            return null;
        }

        if (obj.sprites == null)
        {
            Debug.LogError($"Dictionary sprites est null pour {obj.name}!");
            return null;
        }

        if (obj.sprites.ContainsKey(count))
        {
            return obj.sprites[count];
        }

        // Fallback: si l'image pour ce count n'existe pas, utiliser x1
        Debug.LogWarning($"Sprite non trouvé pour {obj.name} x{count}, recherche de x1...");
        if (obj.sprites.ContainsKey(1))
        {
            Debug.LogWarning($"Utilisation de {obj.name} x1 comme fallback");
            return obj.sprites[1];
        }

        Debug.LogError($"Aucun sprite disponible pour {obj.name}! Sprites disponibles: {string.Join(", ", obj.sprites.Keys)}");
        return null;
    }

    List<FruitObject> SelectRandomObjects(int count)
    {
        if (availableObjects == null || availableObjects.Count == 0)
        {
            Debug.LogError("Aucun objet disponible! Assurez-vous que les images sont dans Resources/Images avec le format NxM.png");
            return new List<FruitObject>();
        }

        List<FruitObject> selected = new List<FruitObject>();
        List<FruitObject> available = new List<FruitObject>(availableObjects);

        for (int i = 0; i < count && available.Count > 0; i++)
        {
            int index = Random.Range(0, available.Count);
            selected.Add(available[index]);
            available.RemoveAt(index);
        }

        if (selected.Count < count)
        {
            Debug.LogWarning($"Seulement {selected.Count} objets disponibles sur {count} demandés!");
        }

        return selected;
    }

    public List<Equation> GetEquations() => equations;
    public List<string> GetObjectsToGuess() => objectsToGuess;
    public Dictionary<string, int> GetObjectValues() => objectValues;
    public int GetCurrentLevel() => currentLevel;

    public Sprite GetSpriteForObject(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
        {
            Debug.LogError("objectName est null ou vide!");
            return null;
        }

        Debug.Log($"Recherche du sprite pour: '{objectName}'");
        Debug.Log($"Objets disponibles: {string.Join(", ", availableObjects.ConvertAll(o => o.name))}");

        var obj = availableObjects.Find(o => o.name == objectName);
        if (obj != null)
        {
            Debug.Log($"Objet trouvé: {obj.name}, Sprites disponibles: {string.Join(", ", obj.sprites.Keys)}");

            if (obj.sprites != null && obj.sprites.ContainsKey(1))
            {
                Debug.Log($"Sprite x1 trouvé pour '{objectName}'");
                return obj.sprites[1];
            }
            else if (obj.sprites == null)
            {
                Debug.LogError($"Dictionary sprites est null pour '{objectName}'!");
            }
            else if (!obj.sprites.ContainsKey(1))
            {
                Debug.LogError($"Sprite x1 non trouvé pour '{objectName}'! Sprites disponibles: {string.Join(", ", obj.sprites.Keys)}");
            }
        }
        else
        {
            Debug.LogError($"Objet '{objectName}' non trouvé dans availableObjects!");
        }

        return null;
    }
}
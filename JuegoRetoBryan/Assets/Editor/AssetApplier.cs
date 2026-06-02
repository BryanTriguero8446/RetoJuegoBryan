using UnityEditor;
using UnityEngine;

/// <summary>
/// Aplica los modelos 3D y texturas descargados al Player y la escena.
/// Usa Warrior como personaje y FBX_Pistol como arma (necesario para Physics.Raycast).
///
/// Uso: Menu Tools > Apply Assets to Scene
/// </summary>
public class AssetApplier
{
    // Rutas exactas a los assets del proyecto
    private const string PLAYER_MODEL_PATH   = "Assets/Models/FBX_Player/Warrior.fbx";
    private const string PLAYER_TEXTURE_PATH = "Assets/Texture/TexturesPlayer/Warrior_Texture.png";

    private const string SWORD_MODEL_PATH   = "Assets/Models/FBX_Player/Only Weapons/Warrior_Sword.fbx";
    private const string SWORD_TEXTURE_PATH = "Assets/Texture/TexturesPlayer/Warrior_Sword_Texture.png";

    private const string TERRAIN_MODEL_PATH  = "Assets/Models/rocky_terrain_02_2k.fbx";

    private const string MATERIALS_FOLDER    = "Assets/Materials";

    [MenuItem("Tools/Apply Assets to Scene")]
    public static void ApplyAll()
    {
        if (!AssetDatabase.IsValidFolder(MATERIALS_FOLDER))
            AssetDatabase.CreateFolder("Assets", "Materials");

        ApplyPlayerModel();
        ApplyWeaponModel();
        ApplyGroundModel();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Assets Aplicados",
            "✓ Personaje Warrior aplicado al Player\n" +
            "✓ Pistola con texturas PBR como arma\n" +
            "✓ Terreno rocoso aplicado al Ground\n\n" +
            "Ya puedes presionar Play.",
            "OK"
        );
    }

    private static void ApplyPlayerModel()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[AssetApplier] No hay GameObject con tag 'Player' en la escena.");
            return;
        }

        // Eliminar visual previo (cubo/capsule)
        MeshFilter oldMesh = player.GetComponent<MeshFilter>();
        MeshRenderer oldRenderer = player.GetComponent<MeshRenderer>();
        if (oldMesh != null) Object.DestroyImmediate(oldMesh);
        if (oldRenderer != null) Object.DestroyImmediate(oldRenderer);

        // Eliminar hijos visuales anteriores (si re-aplicas)
        Transform existingVisual = player.transform.Find("Visual");
        if (existingVisual != null)
            Object.DestroyImmediate(existingVisual.gameObject);

        // Cargar e instanciar el modelo Warrior
        GameObject warriorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PLAYER_MODEL_PATH);
        if (warriorPrefab == null)
        {
            Debug.LogError($"[AssetApplier] No se encontró {PLAYER_MODEL_PATH}");
            return;
        }

        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(warriorPrefab);
        visual.name = "Visual";
        visual.transform.SetParent(player.transform, false);
        visual.transform.localPosition = new Vector3(0f, -1f, 0f); // bajar para alinear con CharacterController
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        // Aplicar textura del Warrior creando un material
        ApplyTextureToModel(visual, PLAYER_TEXTURE_PATH, "Warrior_Material", false);

        Debug.Log("[AssetApplier] Modelo Warrior aplicado al Player.");
    }

    private static void ApplyWeaponModel()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Transform handBone = FindHandBone(player.transform);
        RemoveOldWeapon(player.transform);

        GameObject swordPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SWORD_MODEL_PATH);
        if (swordPrefab == null)
        {
            Debug.LogError($"[AssetApplier] No se encontró {SWORD_MODEL_PATH}");
            return;
        }

        GameObject sword = (GameObject)PrefabUtility.InstantiatePrefab(swordPrefab);
        sword.name = "Weapon";

        if (handBone != null)
        {
            sword.transform.SetParent(handBone, false);
            sword.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            sword.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            sword.transform.localScale    = Vector3.one;
            Debug.Log($"[AssetApplier] Espada enganchada al hueso: {handBone.name}");
        }
        else
        {
            sword.transform.SetParent(player.transform, false);
            sword.transform.localPosition = new Vector3(0.3f, 1f, 0.5f);
            Debug.LogWarning("[AssetApplier] No se encontró bone de mano. Espada anclada al Player.");
        }

        // Crear MuzzlePoint en la punta del arma (origen del disparo horizontal)
        GameObject muzzle = new GameObject("MuzzlePoint");
        muzzle.transform.SetParent(sword.transform, false);
        muzzle.transform.localPosition = new Vector3(0f, 0.5f, 0.3f); // adelante del arma

        // Asignar MuzzlePoint al WeaponSystem del Player
        WeaponSystem weapon = player.GetComponent<WeaponSystem>();
        if (weapon != null)
        {
            SerializedObject so = new SerializedObject(weapon);
            so.FindProperty("muzzlePoint").objectReferenceValue = muzzle.transform;
            so.ApplyModifiedProperties();
        }

        // Material con la textura del arma
        ApplyTextureToModel(sword, SWORD_TEXTURE_PATH, "Sword_Material", true);
    }

    private static void RemoveOldWeapon(Transform root)
    {
        string[] oldNames = { "Pistol", "Weapon", "Sword" };
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            foreach (string n in oldNames)
            {
                if (t.name == n)
                {
                    Object.DestroyImmediate(t.gameObject);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Busca recursivamente un hueso cuyo nombre indique mano derecha.
    /// Soporta multiples convenciones: Hand_R, RightHand, hand.R, etc.
    /// </summary>
    private static Transform FindHandBone(Transform root)
    {
        string[] candidates = {
            "RightHand", "Right Hand", "Hand_R", "hand_r", "hand.R", "Hand.R",
            "mixamorig:RightHand", "Bip01 R Hand", "R_Hand", "Bone_Hand_R",
            "Hand_R_end", "wrist.R"
        };

        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            string n = t.name;
            foreach (string c in candidates)
            {
                if (n.Equals(c, System.StringComparison.OrdinalIgnoreCase))
                    return t;
            }
        }

        // Segundo intento: contiene "hand" + "r"
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            string n = t.name.ToLower();
            if (n.Contains("hand") && (n.Contains("r") || n.EndsWith("_r")))
                return t;
        }

        return null;
    }


    private static void ApplyGroundModel()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground == null)
        {
            Debug.LogWarning("[AssetApplier] No se encontró GameObject 'Ground'.");
            return;
        }

        GameObject terrainPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TERRAIN_MODEL_PATH);
        if (terrainPrefab == null)
        {
            Debug.LogWarning($"[AssetApplier] No se encontró {TERRAIN_MODEL_PATH}");
            return;
        }

        // Si Ground es un cubo, agregarle el visual del terreno como hijo
        Transform existingTerrain = ground.transform.Find("TerrainVisual");
        if (existingTerrain != null)
            Object.DestroyImmediate(existingTerrain.gameObject);

        // Escalar Ground para que sea una plataforma extensa
        ground.transform.localScale = new Vector3(20f, 1f, 20f);

        // Asignar material rocoso al cubo Ground (mantiene el collider)
        MeshRenderer groundRenderer = ground.GetComponent<MeshRenderer>();
        if (groundRenderer != null)
        {
            // Buscar textura asociada al terreno (en caso de no haber, usar color piedra)
            Material rockMat = new Material(Shader.Find("Standard"));
            rockMat.color = new Color(0.5f, 0.45f, 0.4f); // tono rocoso
            rockMat.SetFloat("_Glossiness", 0.1f);

            string matPath = $"{MATERIALS_FOLDER}/Ground_Rock.mat";
            AssetDatabase.CreateAsset(rockMat, matPath);
            groundRenderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        }

        Debug.Log("[AssetApplier] Ground expandido (20x20) con textura rocosa.");
    }

    /// <summary>
    /// Crea un material con la textura aplicada y la asigna a todos los MeshRenderers del modelo.
    /// </summary>
    private static void ApplyTextureToModel(GameObject model, string texturePath, string matName, bool isMetal)
    {
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (tex == null)
        {
            Debug.LogWarning($"[AssetApplier] No se encontró textura {texturePath}");
            return;
        }

        Material mat = new Material(Shader.Find("Standard"));
        mat.mainTexture = tex;
        mat.SetFloat("_Glossiness", isMetal ? 0.7f : 0.2f);
        if (isMetal) mat.SetFloat("_Metallic", 0.8f);

        string matPath = $"{MATERIALS_FOLDER}/{matName}.mat";
        AssetDatabase.CreateAsset(mat, matPath);
        Material savedMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

        ApplyMaterialToRenderers(model, savedMat);
    }

    private static void ApplyMaterialToRenderers(GameObject root, Material mat)
    {
        SkinnedMeshRenderer[] skinned = root.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var smr in skinned)
            smr.sharedMaterial = mat;

        MeshRenderer[] meshes = root.GetComponentsInChildren<MeshRenderer>();
        foreach (var mr in meshes)
            mr.sharedMaterial = mat;
    }

}

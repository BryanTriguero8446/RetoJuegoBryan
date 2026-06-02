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

    private const string PISTOL_MODEL_PATH   = "Assets/Models/FBX_Pistol.fbx";
    private const string PISTOL_DIFF_PATH    = "Assets/Texture/TexturesPistol/service_pistol_diff_2k.jpg";
    private const string PISTOL_METAL_PATH   = "Assets/Texture/TexturesPistol/service_pistol_metal_2k.exr";
    private const string PISTOL_NORMAL_PATH  = "Assets/Texture/TexturesPistol/service_pistol_nor_gl_2k.exr";
    private const string PISTOL_ROUGH_PATH   = "Assets/Texture/TexturesPistol/service_pistol_rough_2k.exr";

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

        // Buscar la mano derecha del Warrior dentro de la jerarquia de huesos
        Transform handBone = FindHandBone(player.transform);

        // Eliminar pistola anterior (este hijo puede estar en el Player o en la mano)
        RemoveOldPistol(player.transform);

        GameObject pistolPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PISTOL_MODEL_PATH);
        if (pistolPrefab == null)
        {
            Debug.LogError($"[AssetApplier] No se encontró {PISTOL_MODEL_PATH}");
            return;
        }

        GameObject pistol = (GameObject)PrefabUtility.InstantiatePrefab(pistolPrefab);
        pistol.name = "Pistol";

        if (handBone != null)
        {
            // El arma se hace hija de la mano: sigue la animación automáticamente
            pistol.transform.SetParent(handBone, false);
            pistol.transform.localPosition = new Vector3(0f, 0.05f, 0.05f);
            pistol.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            pistol.transform.localScale    = Vector3.one * 0.5f;
            Debug.Log($"[AssetApplier] Pistola enganchada al hueso: {handBone.name}");
        }
        else
        {
            // Fallback si no se encuentra el bone
            pistol.transform.SetParent(player.transform, false);
            pistol.transform.localPosition = new Vector3(0.3f, 0.5f, 0.5f);
            pistol.transform.localScale    = Vector3.one * 0.3f;
            Debug.LogWarning("[AssetApplier] No se encontró bone de mano. Pistola anclada al Player.");
        }

        Material pistolMat = CreatePistolMaterial();
        ApplyMaterialToRenderers(pistol, pistolMat);
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

    private static void RemoveOldPistol(Transform root)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "Pistol")
            {
                Object.DestroyImmediate(t.gameObject);
                return;
            }
        }
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

    /// <summary>
    /// Crea material PBR completo para la pistola con sus 4 texturas (Albedo + Metallic + Normal + Roughness).
    /// </summary>
    private static Material CreatePistolMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));

        Texture2D diff   = AssetDatabase.LoadAssetAtPath<Texture2D>(PISTOL_DIFF_PATH);
        Texture2D metal  = AssetDatabase.LoadAssetAtPath<Texture2D>(PISTOL_METAL_PATH);
        Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(PISTOL_NORMAL_PATH);

        if (diff != null)   mat.SetTexture("_MainTex", diff);
        if (metal != null)  mat.SetTexture("_MetallicGlossMap", metal);

        // El normal map debe marcarse como tal antes de asignarse
        if (normal != null)
        {
            MarkAsNormalMap(PISTOL_NORMAL_PATH);
            mat.SetTexture("_BumpMap", normal);
            mat.EnableKeyword("_NORMALMAP");
        }

        mat.SetFloat("_Metallic", 0.85f);
        mat.SetFloat("_Glossiness", 0.6f);

        string matPath = $"{MATERIALS_FOLDER}/Pistol_PBR.mat";
        AssetDatabase.CreateAsset(mat, matPath);
        return AssetDatabase.LoadAssetAtPath<Material>(matPath);
    }

    private static void MarkAsNormalMap(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.NormalMap)
        {
            importer.textureType = TextureImporterType.NormalMap;
            importer.SaveAndReimport();
        }
    }
}

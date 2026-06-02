using UnityEditor;
using UnityEngine;

/// <summary>
/// Auto-configuración completa de Etapa 2 en un solo click.
/// Uso: Click derecho en el Player → Complete Stage 2 Setup
/// </summary>
public class Stage2AutoSetup
{
    [MenuItem("GameObject/Complete Stage 2 Setup")]
    public static void CompleteSetup()
    {
        GameObject player = Selection.activeGameObject;
        if (player == null)
        {
            EditorUtility.DisplayDialog("Error", "Selecciona el Player en la Hierarchy.", "OK");
            return;
        }

        // Paso 1: Validar CharacterController
        if (player.GetComponent<CharacterController>() == null)
        {
            EditorUtility.DisplayDialog("Error", "El Player necesita CharacterController.", "OK");
            return;
        }

        // Paso 2: Agregar Animator si no existe
        Animator animator = player.GetComponent<Animator>();
        if (animator == null)
        {
            animator = player.AddComponent<Animator>();
            Debug.Log("[Stage2Setup] Animator agregado.");
        }

        // Paso 3: Crear y asignar Animator Controller
        SetupAnimatorController(player, animator);

        // Paso 4: Agregar componentes si no existen
        if (player.GetComponent<PlayerAnimator>() == null)
            player.AddComponent<PlayerAnimator>();

        if (player.GetComponent<WeaponSystem>() == null)
            player.AddComponent<WeaponSystem>();

        if (player.GetComponent<PlayerHealth>() == null)
            player.AddComponent<PlayerHealth>();

        if (player.GetComponent<PlayerInitializer>() == null)
            player.AddComponent<PlayerInitializer>();

        // Paso 5: Crear Hit Effect Prefab
        CreateHitEffectAndAssign(player);

        // Paso 6: Asignar Tag "Player"
        if (player.tag != "Player")
        {
            if (!TagExists("Player"))
                CreateTag("Player");
            player.tag = "Player";
            Debug.Log("[Stage2Setup] Tag 'Player' asignado.");
        }

        EditorUtility.DisplayDialog(
            "✓ Etapa 2 Configurada",
            "Todo está listo para Play:\n\n" +
            "✓ Animator Controller creado\n" +
            "✓ Componentes agregados\n" +
            "✓ Hit Effect Prefab creado\n" +
            "✓ Tag 'Player' asignado\n\n" +
            "Presiona Play para probar.",
            "OK"
        );

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private static void SetupAnimatorController(GameObject player, Animator animator)
    {
        string controllerPath = "Assets/Animation/PlayerAnimator.controller";

        if (!AssetDatabase.IsValidFolder("Assets/Animation"))
            AssetDatabase.CreateFolder("Assets", "Animation");

        var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
        if (controller == null)
        {
            // Crear nuevo controller
            AnimatorSetup.SetupAnimator();
        }

        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
        Debug.Log("[Stage2Setup] Animator Controller asignado.");
    }

    private static void CreateHitEffectAndAssign(GameObject player)
    {
        string prefabPath = "Assets/Prefabs/HitEffect.prefab";

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        GameObject hitEffectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (hitEffectPrefab == null)
        {
            // Crear Hit Effect temporalmente
            GameObject temp = new GameObject("HitEffect");
            ParticleSystem ps = temp.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 0.5f;
            main.startSpeed = 5f;

            var emission = ps.emission;
            emission.rateOverTime = 10f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            ParticleSystemRenderer renderer = temp.GetComponent<ParticleSystemRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 0.8f, 0f, 1f);
            renderer.material = mat;

            PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
            hitEffectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            DestroyImmediate(temp);

            Debug.Log("[Stage2Setup] Hit Effect Prefab creado.");
        }

        // Asignar al WeaponSystem
        WeaponSystem weapon = player.GetComponent<WeaponSystem>();
        if (weapon != null)
        {
            SerializedObject so = new SerializedObject(weapon);
            so.FindProperty("hitEffectPrefab").objectReferenceValue = hitEffectPrefab;
            so.ApplyModifiedProperties();
            Debug.Log("[Stage2Setup] Hit Effect asignado a WeaponSystem.");
        }
    }

    private static bool TagExists(string tag)
    {
        foreach (string t in UnityEditorInternal.InternalEditorUtility.tags)
        {
            if (t == tag) return true;
        }
        return false;
    }

    private static void CreateTag(string tag)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = tagManager.FindProperty("tags");
        tags.arraySize++;
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
        Debug.Log($"[Stage2Setup] Tag '{tag}' creado.");
    }

    [MenuItem("GameObject/Complete Stage 2 Setup", validate = true)]
    private static bool ValidateSetup()
    {
        return Selection.activeGameObject != null &&
               Selection.activeGameObject.GetComponent<CharacterController>() != null;
    }
}

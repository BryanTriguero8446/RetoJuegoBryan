using UnityEditor;
using UnityEngine;

/// <summary>
/// Crea un segundo personaje (Cleric) en la escena como objetivo para los disparos.
/// Configurado con CapsuleCollider + TargetDummy (IDamageable).
///
/// Uso: Menu Tools > Spawn Target Dummy
/// </summary>
public class TargetSpawner
{
    private const string TARGET_MODEL_PATH   = "Assets/Models/FBX_Player/Cleric.fbx";
    private const string TARGET_TEXTURE_PATH = "Assets/Texture/TexturesPlayer/Cleric_Texture.png";

    [MenuItem("Tools/Spawn Target Dummy")]
    public static void SpawnTarget()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TARGET_MODEL_PATH);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Error", $"No se encontro {TARGET_MODEL_PATH}", "OK");
            return;
        }

        // GameObject contenedor con fisica y dano
        GameObject target = new GameObject("TargetDummy_Cleric");

        // Posicionar 5m al frente del jugador (o del origen si no existe)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target.transform.position = player.transform.position + player.transform.forward * 5f;
            target.transform.rotation = Quaternion.LookRotation(-player.transform.forward); // mira al jugador
        }
        else
        {
            target.transform.position = new Vector3(0f, 1f, 5f);
        }

        // Instanciar el modelo visual como hijo
        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        visual.name = "Visual";
        visual.transform.SetParent(target.transform, false);
        visual.transform.localPosition = new Vector3(0f, -1f, 0f);

        // Aplicar textura
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(TARGET_TEXTURE_PATH);
        if (tex != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.mainTexture = tex;

            string matPath = "Assets/Materials/Cleric_Material.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(matPath) == null)
                AssetDatabase.CreateAsset(mat, matPath);

            Material savedMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            foreach (var smr in visual.GetComponentsInChildren<SkinnedMeshRenderer>())
                smr.sharedMaterial = savedMat;
            foreach (var mr in visual.GetComponentsInChildren<MeshRenderer>())
                mr.sharedMaterial = savedMat;
        }

        // Collider que la bala pueda detectar (CapsuleCollider del tamano del personaje)
        CapsuleCollider col = target.AddComponent<CapsuleCollider>();
        col.height = 2f;
        col.radius = 0.5f;
        col.center = new Vector3(0f, 1f, 0f);

        // Componente de dano (IDamageable)
        target.AddComponent<TargetDummy>();

        // Tag para que la bala lo identifique como objetivo (no es Player)
        // Si quieres usar EnemyAI, asigna tag "Enemy" en su lugar
        if (!TagExists("Target"))
            CreateTag("Target");
        target.tag = "Target";

        Selection.activeGameObject = target;
        EditorGUIUtility.PingObject(target);

        EditorUtility.DisplayDialog(
            "Target Spawneado",
            $"Cleric agregado a la escena en posicion: {target.transform.position}\n\n" +
            "Tag: Target\n" +
            "Vida: 100 HP\n" +
            "Auto-respawn: 3s\n\n" +
            "Dispara hacia el!",
            "OK"
        );
    }

    private static bool TagExists(string tag)
    {
        foreach (string t in UnityEditorInternal.InternalEditorUtility.tags)
            if (t == tag) return true;
        return false;
    }

    private static void CreateTag(string tag)
    {
        var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (asset == null || asset.Length == 0) return;

        var tagManager = new SerializedObject(asset[0]);
        var tags = tagManager.FindProperty("tags");
        tags.arraySize++;
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
}

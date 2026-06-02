using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;

/// <summary>
/// Crea un Hit Effect Prefab funcional para el WeaponSystem.
/// Uso: Click derecho en Hierarchy → Create Hit Effect Prefab
/// </summary>
public class HitEffectSetup
{
    [MenuItem("GameObject/Create Hit Effect Prefab")]
    public static void CreateHitEffectPrefab()
    {
        // Crear GameObject raíz
        GameObject hitEffect = new GameObject("HitEffect");

        // Agregar ParticleSystem
        ParticleSystem ps = hitEffect.AddComponent<ParticleSystem>();

        // Configurar ParticleSystem para que sea visible (sin texturas)
        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = 0.5f;
        main.startSpeed = 5f;
        main.startSize = 0.1f;

        // Emission
        var emission = ps.emission;
        emission.rateOverTime = 10f;

        // Shape
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        // Renderer (se muestra con material por defecto)
        ParticleSystemRenderer renderer = hitEffect.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        // Asignar material estándar
        Material defaultMaterial = new Material(Shader.Find("Standard"));
        defaultMaterial.color = new Color(1f, 0.8f, 0f, 1f); // Amarillo (para impactos)
        renderer.material = defaultMaterial;

        // Guardar como Prefab
        string prefabPath = "Assets/Prefabs/HitEffect.prefab";

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        PrefabUtility.SaveAsPrefabAsset(hitEffect, prefabPath);

        // Asignar a WeaponSystem si existe
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.TryGetComponent<WeaponSystem>(out var weapon))
        {
            GameObject hitEffectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            SerializedObject so = new SerializedObject(weapon);
            so.FindProperty("hitEffectPrefab").objectReferenceValue = hitEffectPrefab;
            so.ApplyModifiedProperties();
        }

        Object.DestroyImmediate(hitEffect);

        EditorUtility.DisplayDialog("Éxito", "Hit Effect Prefab creado en Assets/Prefabs/\n\nYa está asignado a WeaponSystem.", "OK");
    }
}

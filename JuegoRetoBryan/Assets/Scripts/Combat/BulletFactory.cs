using UnityEngine;

/// <summary>
/// Fabrica balas en runtime sin necesidad de un Prefab externo.
/// Placeholder visual: esfera amarilla pequena. Sustituir mas adelante
/// cuando se tenga un asset (sprite/modelo) definitivo.
/// </summary>
public static class BulletFactory
{
    private static Material _cachedMat;

    public static GameObject Create(Vector3 origin, Vector3 direction, float damage, float life)
    {
        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.name = "Bullet";
        bullet.transform.position = origin;
        bullet.transform.localScale = Vector3.one * 0.15f;

        // Material amarillo brillante como placeholder
        if (_cachedMat == null)
        {
            _cachedMat = new Material(Shader.Find("Standard"));
            _cachedMat.color = Color.yellow;
            _cachedMat.EnableKeyword("_EMISSION");
            _cachedMat.SetColor("_EmissionColor", Color.yellow * 2f);
        }
        bullet.GetComponent<MeshRenderer>().sharedMaterial = _cachedMat;

        // El collider de la esfera ya viene; lo hacemos trigger para no rebotar
        SphereCollider col = bullet.GetComponent<SphereCollider>();
        col.isTrigger = true;

        // Rigidbody para que la bala viaje por fisica
        Rigidbody rb = bullet.AddComponent<Rigidbody>();
        rb.useGravity = false;        // proyectil recto
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Logica de dano
        Projectile p = bullet.AddComponent<Projectile>();
        p.Damage   = damage;
        p.Lifetime = life;

        return bullet;
    }
}

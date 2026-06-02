using UnityEngine;

/// <summary>
/// Fabrica balas y efectos de impacto en runtime, sin necesidad de Prefabs.
/// Placeholder visual: esfera amarilla emisiva + burst de particulas en hit.
/// Sustituir mas adelante por assets definitivos.
/// </summary>
public static class BulletFactory
{
    private static Material _cachedMat;
    private static Material _cachedHitMat;

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

    /// <summary>
    /// Crea un VFX de impacto procedural en la posicion dada.
    /// Burst de particulas naranjas con luz puntual breve.
    /// </summary>
    public static void SpawnHitVFX(Vector3 position, Vector3 normal)
    {
        // Si la normal viene en cero, usar arriba como fallback
        if (normal == Vector3.zero)
            normal = Vector3.up;

        GameObject fx = new GameObject("HitVFX");
        fx.transform.position = position;
        fx.transform.rotation = Quaternion.LookRotation(normal);

        ParticleSystem ps = fx.AddComponent<ParticleSystem>();
        // Detener el sistema antes de modificarlo (AddComponent lo arranca automaticamente)
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Main
        var main = ps.main;
        main.playOnAwake = false;
        main.duration         = 0.3f;
        main.loop             = false;
        main.startLifetime    = 0.4f;
        main.startSpeed       = 4f;
        main.startSize        = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startColor       = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.5f, 0f), new Color(1f, 0.9f, 0.2f));
        main.gravityModifier  = 0.5f;

        // Emission: burst de 25 particulas instantaneas
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 25)
        });

        // Shape: cono que apunta hacia la normal de la superficie
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle     = 35f;
        shape.radius    = 0.05f;

        // Color over lifetime: fade out
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.7f, 0.1f), 0f),
                    new GradientColorKey(new Color(1f, 0.3f, 0f),   1f) },
            new[] { new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

        // Renderer con material emisivo
        ParticleSystemRenderer r = fx.GetComponent<ParticleSystemRenderer>();
        if (_cachedHitMat == null)
        {
            _cachedHitMat = new Material(Shader.Find("Particles/Standard Unlit"));
            _cachedHitMat.color = new Color(1f, 0.6f, 0f);
        }
        r.material = _cachedHitMat;

        // Luz puntual breve para destacar el impacto
        GameObject lightGO = new GameObject("HitLight");
        lightGO.transform.SetParent(fx.transform, false);
        Light light = lightGO.AddComponent<Light>();
        light.color     = new Color(1f, 0.6f, 0f);
        light.intensity = 3f;
        light.range     = 2f;

        ps.Play();
        Object.Destroy(fx, 1f);
    }
}

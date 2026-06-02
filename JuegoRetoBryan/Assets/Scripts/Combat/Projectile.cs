using UnityEngine;

/// <summary>
/// Proyectil fisico (bala). Aplica dano a cualquier IDamageable con que colisione,
/// spawnea VFX de impacto en el punto exacto de colision y se auto-destruye.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    public float Damage   { get; set; } = 25f;
    public float Lifetime { get; set; } = 3f;

    public string ShooterTag { get; set; } = "Player";

    private Vector3 _lastPosition;

    private void Start()
    {
        Destroy(gameObject, Lifetime);
    }

    private void FixedUpdate()
    {
        // Guarda posicion previa para fallback de impacto cuando es trigger
        _lastPosition = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.GetContact(0);
        HandleHit(collision.collider, contact.point, contact.normal);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Para triggers no hay punto de contacto exacto: aproximar
        Vector3 hitPoint  = transform.position;
        Vector3 hitNormal = (_lastPosition - transform.position).normalized;
        if (hitNormal == Vector3.zero) hitNormal = -transform.forward;

        HandleHit(other, hitPoint, hitNormal);
    }

    private void HandleHit(Collider other, Vector3 hitPoint, Vector3 hitNormal)
    {
        // Ignora al jugador que la disparo
        if (!string.IsNullOrEmpty(ShooterTag) && other.CompareTag(ShooterTag))
            return;

        // Aplica dano si el objeto es IDamageable
        if (other.TryGetComponent<IDamageable>(out var target))
            target.TakeDamage(Damage);

        // VFX de impacto en el punto exacto de colision
        BulletFactory.SpawnHitVFX(hitPoint, hitNormal);

        Destroy(gameObject);
    }
}

using UnityEngine;

/// <summary>
/// Proyectil fisico (bala). Aplica dano a cualquier IDamageable con que colisione
/// y se auto-destruye al chocar o cuando su vida util acaba.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    public float Damage   { get; set; } = 25f;
    public float Lifetime { get; set; } = 3f;

    public string ShooterTag { get; set; } = "Player"; // para no auto-impactarse

    private void Start()
    {
        Destroy(gameObject, Lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    private void HandleHit(Collider other)
    {
        // Ignora al jugador que la disparo
        if (!string.IsNullOrEmpty(ShooterTag) && other.CompareTag(ShooterTag))
            return;

        if (other.TryGetComponent<IDamageable>(out var target))
            target.TakeDamage(Damage);

        Destroy(gameObject);
    }
}

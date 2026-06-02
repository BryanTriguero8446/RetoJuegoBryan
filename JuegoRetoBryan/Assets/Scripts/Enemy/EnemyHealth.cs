using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Salud del enemigo. Implementa IDamageable igual que PlayerHealth,
/// demostrando la reutilizacion del sistema de dano a traves de la interfaz.
/// </summary>
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 50f;

    public float CurrentHealth { get; private set; }
    public float MaxHealth     => maxHealth;

    public event System.Action<float, float> OnHealthChanged;
    public event System.Action               OnDeath;

    private bool _isDead;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (_isDead)
            return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0f)
            Die();
    }

    public void Die()
    {
        if (_isDead)
            return;

        _isDead = true;
        OnDeath?.Invoke();

        // Desactiva la IA y el agente de navegacion antes de destruir
        if (TryGetComponent<EnemyAI>(out var ai))
            ai.enabled = false;

        if (TryGetComponent<NavMeshAgent>(out var agent))
            agent.enabled = false;

        Destroy(gameObject, 2f);
    }
}

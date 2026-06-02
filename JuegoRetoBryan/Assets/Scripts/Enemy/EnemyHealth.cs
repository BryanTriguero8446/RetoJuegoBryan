using UnityEngine;

/// <summary>
/// Salud del enemigo. Implementa IDamageable para reutilizar el sistema
/// de dano a traves de la interfaz, igual que PlayerHealth y TargetDummy.
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

        Destroy(gameObject, 2f);
    }
}

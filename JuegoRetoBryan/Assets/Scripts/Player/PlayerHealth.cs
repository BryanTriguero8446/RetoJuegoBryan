using UnityEngine;

/// <summary>
/// Salud del jugador. Implementa IDamageable para recibir dano desde cualquier fuente
/// (enemigos cuerpo a cuerpo, proyectiles, trampas, etc.) sin acoplar las clases.
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;

    public float CurrentHealth { get; private set; }
    public float MaxHealth     => maxHealth;

    // Eventos para la UI (barra de salud) sin referencia directa
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
        Debug.Log("[PlayerHealth] El jugador ha muerto.");
        // Aqui se puede llamar a GameManager para mostrar Game Over
    }
}

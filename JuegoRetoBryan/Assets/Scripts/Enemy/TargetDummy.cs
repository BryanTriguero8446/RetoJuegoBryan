using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Objetivo simple para practicar disparos. Implementa IDamageable
/// y muestra feedback visual al recibir impacto (parpadeo + knockback opcional).
/// Se reinicia automaticamente despues de "morir".
/// </summary>
public class TargetDummy : MonoBehaviour, IDamageable
{
    [Header("Salud")]
    [SerializeField] private float maxHealth   = 100f;
    [SerializeField] private bool  autoRespawn = true;
    [SerializeField] private float respawnTime = 3f;

    [Header("Feedback Visual")]
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float flashDuration = 0.15f;

    public float CurrentHealth { get; private set; }
    public float MaxHealth     => maxHealth;

    public event System.Action<float, float> OnHealthChanged;
    public event System.Action               OnDeath;

    private List<Renderer> _renderers = new();
    private List<Color[]>  _originalColors = new();
    private bool   _isDead;
    private Vector3 _startPosition;

    private void Awake()
    {
        CurrentHealth   = maxHealth;
        _startPosition  = transform.position;

        // Cachear todos los renderers para el efecto flash
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            _renderers.Add(r);
            Color[] colors = new Color[r.materials.Length];
            for (int i = 0; i < r.materials.Length; i++)
                colors[i] = r.materials[i].color;
            _originalColors.Add(colors);
        }
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        StartCoroutine(FlashRoutine());

        if (CurrentHealth <= 0f)
            Die();
    }

    public void Die()
    {
        if (_isDead) return;
        _isDead = true;
        OnDeath?.Invoke();

        Debug.Log($"[TargetDummy] {name} derribado.");

        if (autoRespawn)
            StartCoroutine(RespawnRoutine());
        else
            Destroy(gameObject, 1f);
    }

    private IEnumerator FlashRoutine()
    {
        // Pinta de rojo
        for (int i = 0; i < _renderers.Count; i++)
        {
            foreach (var mat in _renderers[i].materials)
                mat.color = hitFlashColor;
        }

        yield return new WaitForSeconds(flashDuration);

        // Restaura color original
        for (int i = 0; i < _renderers.Count; i++)
        {
            for (int j = 0; j < _renderers[i].materials.Length; j++)
                _renderers[i].materials[j].color = _originalColors[i][j];
        }
    }

    private IEnumerator RespawnRoutine()
    {
        // Cae rotando
        transform.Rotate(0f, 0f, 90f);

        yield return new WaitForSeconds(respawnTime);

        // Resetea
        transform.position = _startPosition;
        transform.rotation = Quaternion.identity;
        CurrentHealth = maxHealth;
        _isDead = false;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}

using UnityEngine;

/// <summary>
/// Auto-inicializa y valida todos los componentes del Player para Etapa 2.
/// Se ejecuta una vez en Awake y asegura que todo esté conectado.
/// </summary>
public class PlayerInitializer : MonoBehaviour
{
    private void Awake()
    {
        InitializePlayer();
    }

    private void InitializePlayer()
    {
        // Validar y obtener componentes
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement == null)
        {
            Debug.LogError("[PlayerInitializer] PlayerMovement no encontrado.");
            return;
        }

        // Validar Animator
        Animator animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("[PlayerInitializer] Animator no encontrado. Agrega el componente.");
            return;
        }

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("[PlayerInitializer] Animator Controller no asignado.");
            return;
        }

        // Validar PlayerAnimator
        PlayerAnimator playerAnimator = GetComponent<PlayerAnimator>();
        if (playerAnimator == null)
        {
            Debug.LogWarning("[PlayerInitializer] PlayerAnimator no encontrado. Agregándolo...");
            gameObject.AddComponent<PlayerAnimator>();
        }

        // Validar WeaponSystem
        WeaponSystem weapon = GetComponent<WeaponSystem>();
        if (weapon == null)
        {
            Debug.LogWarning("[PlayerInitializer] WeaponSystem no encontrado. Agregándolo...");
            gameObject.AddComponent<WeaponSystem>();
        }

        // Validar PlayerHealth
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health == null)
        {
            Debug.LogWarning("[PlayerInitializer] PlayerHealth no encontrado. Agregándolo...");
            gameObject.AddComponent<PlayerHealth>();
        }

    }
}

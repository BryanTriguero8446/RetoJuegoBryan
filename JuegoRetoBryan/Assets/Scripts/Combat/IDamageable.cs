/// <summary>
/// Interfaz que deben implementar todos los objetos que puedan recibir dano.
/// Desacopla el sistema de combate de los tipos concretos (jugador, enemigo, destruible).
/// </summary>
public interface IDamageable
{
    void TakeDamage(float amount);
    void Die();
}

using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// IA basica del enemigo usando NavMeshAgent.
/// Estados: Idle -> Chase (detecta al jugador) -> Attack (rango de ataque).
/// Requiere que el jugador tenga el tag "Player" y el componente IDamageable.
/// El GameObject debe estar sobre una superficie con NavMesh horneado.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, Chase, Attack }

    [Header("Deteccion")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange    = 2f;

    [Header("Ataque")]
    [SerializeField] private float attackDamage   = 10f;
    [SerializeField] private float attackInterval = 1f;

    [Header("Animacion (opcional)")]
    [SerializeField] private Animator animator;

    private static readonly int SpeedHash  = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    public EnemyState CurrentState { get; private set; } = EnemyState.Idle;

    private NavMeshAgent _agent;
    private Transform    _player;
    private float        _attackTimer;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            TryGetComponent(out animator);
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
        else
            Debug.LogWarning("[EnemyAI] No se encontro un GameObject con tag 'Player'.");
    }

    private void Update()
    {
        if (_player == null)
            return;

        float dist = Vector3.Distance(transform.position, _player.position);
        UpdateState(dist);
        ExecuteState();
        UpdateAnimator();
    }

    private void UpdateState(float dist)
    {
        if (dist <= attackRange)
            CurrentState = EnemyState.Attack;
        else if (dist <= detectionRange)
            CurrentState = EnemyState.Chase;
        else
            CurrentState = EnemyState.Idle;
    }

    private void ExecuteState()
    {
        switch (CurrentState)
        {
            case EnemyState.Idle:
                _agent.ResetPath();
                break;

            case EnemyState.Chase:
                _agent.SetDestination(_player.position);
                break;

            case EnemyState.Attack:
                _agent.ResetPath();
                FacePlayer();
                HandleAttack();
                break;
        }
    }

    private void FacePlayer()
    {
        Vector3 dir = (_player.position - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(
                transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
    }

    private void HandleAttack()
    {
        _attackTimer += Time.deltaTime;
        if (_attackTimer < attackInterval)
            return;

        _attackTimer = 0f;

        if (animator != null)
            animator.SetTrigger(AttackHash);

        if (_player.TryGetComponent<IDamageable>(out var target))
            target.TakeDamage(attackDamage);
    }

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        float speed = _agent.velocity.magnitude / _agent.speed;
        animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
    }

    // Muestra los rangos en el editor para facilitar el ajuste
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

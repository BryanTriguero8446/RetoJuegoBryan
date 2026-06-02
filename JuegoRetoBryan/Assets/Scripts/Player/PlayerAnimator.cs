using UnityEngine;

/// <summary>
/// Puente entre PlayerMovement y el Animator Controller.
/// Parametros requeridos en el Animator:
///   - Speed (Float)  -> usado en el Blend Tree Idle/Run
///   - IsGrounded (Bool)
///   - Jump (Trigger)
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int SpeedHash      = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash       = Animator.StringToHash("Jump");

    private Animator _animator;
    private PlayerMovement _movement;
    private bool _wasGrounded;

    private void Awake()
    {
        _animator  = GetComponent<Animator>();
        _movement  = GetComponent<PlayerMovement>();

        if (_animator == null || _movement == null)
            enabled = false;
    }

    private void Start()
    {
        // Start() corre despues de todos los Awake(), asi que _controller ya existe
        if (_movement != null)
            _wasGrounded = _movement.IsGrounded;
    }

    private void Update()
    {
        if (_animator == null || _movement == null)
            return;

        // Blend Tree: interpola suavemente entre Idle y Run
        _animator.SetFloat(SpeedHash, _movement.Speed, 0.1f, Time.deltaTime);
        _animator.SetBool(IsGroundedHash, _movement.IsGrounded);

        // Dispara Jump solo en el frame en que el jugador deja el suelo
        if (_wasGrounded && !_movement.IsGrounded)
            _animator.SetTrigger(JumpHash);

        _wasGrounded = _movement.IsGrounded;
    }
}

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public enum PlayerState { Idle, Run, Jump }

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Salto y Gravedad")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Camara")]
    [SerializeField] private Transform cameraTransform;

    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;
    public float Speed { get; private set; }
    public bool IsGrounded => _controller != null && _controller.isGrounded;

    private CharacterController _controller;
    private Vector3 _velocity;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        HandleGround();
        HandleMovement();
        HandleJump();
        ApplyGravity();
        UpdateState();
    }

    private void HandleGround()
    {
        if (_controller.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0f, v).normalized;
        Speed = input.magnitude;

        if (input.magnitude < 0.1f || cameraTransform == null)
            return;

        float targetAngle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg
                            + cameraTransform.eulerAngles.y;

        float smoothAngle = Mathf.LerpAngle(
            transform.eulerAngles.y, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        _controller.Move(moveDir * moveSpeed * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && _controller.isGrounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void ApplyGravity()
    {
        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void UpdateState()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool moving = new Vector2(h, v).magnitude > 0.1f;

        if (!_controller.isGrounded)
            CurrentState = PlayerState.Jump;
        else if (moving)
            CurrentState = PlayerState.Run;
        else
            CurrentState = PlayerState.Idle;
    }
}

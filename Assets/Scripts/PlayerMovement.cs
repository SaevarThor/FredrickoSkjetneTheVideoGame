using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 9f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float verticalLookClamp = 85f;

    [Header("References")]
    [SerializeField] private Transform cameraHolder; // Assign the Camera (or its parent) in the Inspector

    // Private state
    private CharacterController _controller;
    private Vector3 _velocity;         // Tracks vertical velocity (gravity + jump)
    private float _verticalRotation;   // Current camera pitch

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        // Lock and hide the cursor for FPS gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        var referance = ReferenceManager.Instance; 
        referance.PlayerTransform = this.transform; 
        referance.gameManager.ApplyActiveUpgrades();

    }

    private void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    public void ApplySpeedUpgrade(float speedIncrease)
    {
        walkSpeed = walkSpeed * (1f + speedIncrease);
        sprintSpeed = sprintSpeed * (1f + speedIncrease);
    }

    // -------------------------------------------------------------------------
    // Mouse Look
    // Rotates the player body horizontally and the camera vertically.
    // -------------------------------------------------------------------------
    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        // Rotate the player body left/right
        transform.Rotate(Vector3.up * mouseX);

        // Rotate the camera up/down, clamped so the player can't flip over
        _verticalRotation -= mouseY;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -verticalLookClamp, verticalLookClamp);
        cameraHolder.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
    }

    // -------------------------------------------------------------------------
    // Movement
    // Handles WASD, sprinting, jumping, and gravity.
    // -------------------------------------------------------------------------
    private void HandleMovement()
    {
        bool isGrounded = _controller.isGrounded;

        // Bleed off accumulated downward velocity when grounded
        if (isGrounded && _velocity.y < 0f)
            _velocity.y = -2f; // Small negative keeps isGrounded reliable

        // --- Horizontal movement ---
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D
        float vertical   = Input.GetAxisRaw("Vertical");   // W/S

        Vector3 moveDir = transform.right * horizontal + transform.forward * vertical;
        moveDir.Normalize(); // Prevent faster diagonal movement

        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && vertical > 0f;
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        _controller.Move(moveDir * currentSpeed * Time.deltaTime);

        // --- Jumping ---
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // v = sqrt(jumpHeight * -2 * gravity)  →  reaches exactly jumpHeight
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // --- Gravity ---
        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }
}
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Snog.InteractionSystem.Demo
{
    /// <summary>
    /// First-person character controller for the Interaction System demo scene.
    /// Handles walking, sprinting, jumping, gravity, and mouse-look.
    ///
    /// Requires:
    /// <list type="bullet">
    ///   <item>A <see cref="CharacterController"/> on the same GameObject.</item>
    ///   <item>A Camera on a child GameObject assigned to <see cref="playerCamera"/>.</item>
    /// </list>
    ///
    /// Input:
    /// <list type="bullet">
    ///   <item>Move  — WASD / left stick</item>
    ///   <item>Look  — Mouse / right stick</item>
    ///   <item>Jump  — Space / South button</item>
    ///   <item>Sprint — Left Shift / left stick click</item>
    ///   <item>Cursor toggle — I (re-locks cursor after UI interaction)</item>
    /// </list>
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Snog/Demo/First Person Controller")]
    [DisallowMultipleComponent]
    public class FirstPersonController : MonoBehaviour
    {
        // ── References ────────────────────────────────────────────────────────────

        [Header("References")]
        [Tooltip("Camera child used for mouse-look. Assign the Camera GameObject directly.")]
        [SerializeField] private Camera playerCamera;

        // ── Movement ──────────────────────────────────────────────────────────────

        [Header("Movement")]
        [Tooltip("Base walking speed in metres per second.")]
        [SerializeField] private float walkSpeed   = 4f;

        [Tooltip("Speed while holding Sprint.")]
        [SerializeField] private float sprintSpeed = 8f;

        [Tooltip("How quickly the character accelerates and decelerates. Higher = snappier.")]
        [Min(1f)]
        [SerializeField] private float acceleration = 12f;

        // ── Jump & Gravity ────────────────────────────────────────────────────────

        [Header("Jump & Gravity")]
        [Tooltip("Vertical velocity applied on jump.")]
        [SerializeField] private float jumpHeight  = 1.2f;

        [Tooltip("Gravity multiplier. Unity's default gravity is −9.81 m/s².")]
        [SerializeField] private float gravityScale = 2.4f;

        // ── Mouse Look ────────────────────────────────────────────────────────────

        [Header("Mouse Look")]
        [Tooltip("Mouse / stick sensitivity. X = yaw, Y = pitch.")]
        [SerializeField] private Vector2 sensitivity = new Vector2(0.12f, 0.12f);

        [Tooltip("Maximum downward pitch angle in degrees.")]
        [Range(10f, 90f)]
        [SerializeField] private float pitchMin = 80f;

        [Tooltip("Maximum upward pitch angle in degrees.")]
        [Range(10f, 90f)]
        [SerializeField] private float pitchMax = 80f;

        [Tooltip("If true, look input is smoothed to reduce jitter.")]
        [SerializeField] private bool smoothLook = false;

        [Tooltip("Look smoothing factor. Only used when Smooth Look is enabled.")]
        [Range(1f, 30f)]
        [SerializeField] private float lookSmoothing = 15f;

        // ── Head Bob ──────────────────────────────────────────────────────────────

        [Header("Head Bob  (optional)")]
        [Tooltip("Enable a subtle camera bob while walking.")]
        [SerializeField] private bool enableHeadBob = true;

        [Tooltip("Frequency of the head bob cycle.")]
        [SerializeField] private float bobFrequency = 2.0f;

        [Tooltip("Vertical amplitude of the camera bob.")]
        [SerializeField] private float bobAmplitude = 0.04f;

        // ── Runtime state ─────────────────────────────────────────────────────────

        private CharacterController cc;
        private Vector3             velocity;
        private float               pitch;
        private float               yaw;
        private Vector2             smoothedLookDelta;
        private float               bobTimer;
        private Vector3             cameraRestPosition;
        private bool                cursorLocked = true;
        private bool                disableControls = false;

        // ── Properties ────────────────────────────────────────────────────────────

        /// <summary>True while the character is on the ground.</summary>
        public bool IsGrounded => cc != null && cc.isGrounded;

        /// <summary>True while sprint input is held and the character is moving.</summary>
        public bool IsSprinting { get; private set; }

        /// <summary>The CharacterController component attached to this GameObject.</summary>
        public CharacterController Controller => cc;

        // ─────────────────────────────────────────────────────────────────────────

        private void Reset()
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        private void Awake()
        {
            cc = GetComponent<CharacterController>();

            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();

            if (playerCamera != null)
                cameraRestPosition = playerCamera.transform.localPosition;

            // Initialise yaw from current transform so the camera matches the spawn rotation.
            yaw   = transform.eulerAngles.y;
            pitch = playerCamera != null ? playerCamera.transform.localEulerAngles.x : 0f;
            if (pitch > 180f) pitch -= 360f; // normalise from Unity's 0-360 range
        }

        private void OnEnable()
        {
            LockCursor(true);

#if ENABLE_INPUT_SYSTEM
            // Enable action maps if using the New Input System with an InputActionAsset.
            // Individual InputActionReferences are enabled automatically.
#endif
        }

        private void OnDisable()
        {
            LockCursor(false);
        }

        private void Update()
        {
            HandleCursorToggle();

            if (!cursorLocked) return; // don't consume look/move when cursor is free

            ApplyGravity();
            HandleMovement();
            HandleJump();
            HandleLook();
            HandleHeadBob();
        }

        // ── Gravity ───────────────────────────────────────────────────────────────

        private void ApplyGravity()
        {
            if (cc.isGrounded && velocity.y < 0f)
                velocity.y = -2f; // keep grounded — small negative to re-detect ground next frame
            else
                velocity.y += Physics.gravity.y * gravityScale * Time.deltaTime;
        }

        // ── Movement ──────────────────────────────────────────────────────────────

        private void HandleMovement()
        {
            Vector2 input   = GetMoveInput();
            IsSprinting     = GetSprintHeld() && input.magnitude > 0.1f;
            float   speed   = IsSprinting ? sprintSpeed : walkSpeed;

            // World-space target velocity aligned to look direction
            Vector3 wishDir = transform.right * input.x + transform.forward * input.y;
            wishDir         = Vector3.ClampMagnitude(wishDir, 1f); // prevent faster diagonal movement

            Vector3 targetHorizontal = wishDir * speed;
            Vector3 currentHorizontal = new Vector3(velocity.x, 0f, velocity.z);

            // Smooth acceleration / deceleration
            Vector3 smoothed = Vector3.MoveTowards(
                currentHorizontal,
                targetHorizontal,
                acceleration * speed * Time.deltaTime);

            velocity.x = smoothed.x;
            velocity.z = smoothed.z;

            cc.Move(velocity * Time.deltaTime);
        }

        // ── Jump ──────────────────────────────────────────────────────────────────

        private void HandleJump()
        {
            if (cc.isGrounded && GetJumpPressed())
            {
                // v² = 2gh  →  v = √(2 * |g| * h)
                velocity.y = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * gravityScale * jumpHeight);
            }
        }

        // ── Look ──────────────────────────────────────────────────────────────────

        private void HandleLook()
        {
            Vector2 lookDelta = GetLookInput();

            if (smoothLook)
            {
                smoothedLookDelta = Vector2.Lerp(
                    smoothedLookDelta, lookDelta, lookSmoothing * Time.deltaTime);
                lookDelta = smoothedLookDelta;
            }

            yaw   += lookDelta.x * sensitivity.x;
            pitch -= lookDelta.y * sensitivity.y; // subtract so mouse-up looks up
            pitch  = Mathf.Clamp(pitch, -pitchMax, pitchMin);

            transform.rotation              = Quaternion.Euler(0f, yaw, 0f);

            if (playerCamera != null)
                playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        // ── Head Bob ──────────────────────────────────────────────────────────────

        private void HandleHeadBob()
        {
            if (!enableHeadBob || playerCamera == null) return;

            bool isMovingOnGround = cc.isGrounded &&
                                    new Vector2(velocity.x, velocity.z).magnitude > 0.5f;

            if (isMovingOnGround)
            {
                float speedFactor = IsSprinting ? 1.6f : 1f;
                bobTimer += Time.deltaTime * bobFrequency * speedFactor * Mathf.PI * 2f;

                float bobOffset = Mathf.Sin(bobTimer) * bobAmplitude;
                playerCamera.transform.localPosition = Vector3.Lerp(
                    playerCamera.transform.localPosition,
                    cameraRestPosition + new Vector3(0f, bobOffset, 0f),
                    Time.deltaTime * 12f);
            }
            else
            {
                // Smoothly return to rest position when not moving
                bobTimer = 0f;
                playerCamera.transform.localPosition = Vector3.Lerp(
                    playerCamera.transform.localPosition,
                    cameraRestPosition,
                    Time.deltaTime * 8f);
            }
        }

        // ── Cursor and Movement toggle ──────────────────────────────────────────────

        private void HandleCursorToggle()
        {
#if ENABLE_INPUT_SYSTEM
            bool iPressed = Keyboard.current != null &&
                            Keyboard.current.iKey.wasPressedThisFrame;
#else
            bool iPressed = Input.GetKeyDown(KeyCode.I);
#endif
            if (iPressed)
                LockCursor(!cursorLocked);
        }

        private void LockCursor(bool locked)
        {
            cursorLocked              = locked;
            Cursor.lockState          = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible            = !locked;
        }

        private void LockMovement(bool locked)
        {
            disableControls = locked;
        }

        public void ToggleCursorAndMovement()
        {
            bool newState = !cursorLocked;
            LockCursor(newState);
            LockMovement(!newState);
        }

        // ── Input abstraction ─────────────────────────────────────────────────────

        private Vector2 GetMoveInput()
        {
            if(disableControls) return Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                float x = (Keyboard.current.dKey.isPressed  ? 1f : 0f)
                        - (Keyboard.current.aKey.isPressed  ? 1f : 0f);
                float y = (Keyboard.current.wKey.isPressed  ? 1f : 0f)
                        - (Keyboard.current.sKey.isPressed  ? 1f : 0f);
                if (x != 0f || y != 0f) return new Vector2(x, y);
            }
            if (Gamepad.current != null)
                return Gamepad.current.leftStick.ReadValue();
            return Vector2.zero;
#else
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
        }

        private Vector2 GetLookInput()
        {
            if(disableControls) return Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                Vector2 delta = Mouse.current.delta.ReadValue();
                // Mouse delta is in pixels/frame — scale down to match legacy Input sensitivity
                return delta * 0.05f;
            }
            if (Gamepad.current != null)
                return Gamepad.current.rightStick.ReadValue() * 4f;
            return Vector2.zero;
#else
            return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
#endif
        }

        private bool GetJumpPressed()
        {
            if(disableControls) return false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) return true;
            if (Gamepad.current  != null && Gamepad.current.buttonSouth.wasPressedThisFrame) return true;
            return false;
#else
            return Input.GetButtonDown("Jump");
#endif
        }

        private bool GetSprintHeld()
        {
            if(disableControls) return false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) return true;
            if (Gamepad.current  != null && Gamepad.current.leftStickButton.isPressed) return true;
            return false;
#else
            return Input.GetKey(KeyCode.LeftShift);
#endif
        }
    }
}
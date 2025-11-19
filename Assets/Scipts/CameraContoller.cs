using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Positioning")]
    public Vector3 offset = new Vector3(0f, 1.5f, -5f);
    public LayerMask collisionMask = ~0;
    public float collisionOffset = 0.2f;

    [Header("Look")]
    public float sensitivityX = 250f;
    public float sensitivityY = 250f;
    public float verticalClamp = 80f;
    public bool invertY = false;

    [Header("Smoothing")]
    public float smoothSpeed = 10f;

    [Header("Cursor")]
    public bool lockCursor = true;

    private float yaw = 0f;
    private float pitch = 10f;
    private Vector2 lookInput = Vector2.zero;

    void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Input System callback (if you have a "Look" action mapped)
    void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    void Update()
    {
        // Fallback: if Input System doesn't provide input, try reading mouse delta
        if (lookInput == Vector2.zero)
        {
            if (Mouse.current != null)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                if (mouseDelta != Vector2.zero) lookInput = mouseDelta;
            }
            else
            {
                float mx = Input.GetAxis("Mouse X");
                float my = Input.GetAxis("Mouse Y");
                Vector2 legacy = new Vector2(mx, my);
                if (legacy != Vector2.zero) lookInput = legacy;
            }
        }

        // Apply look input to yaw/pitch
        yaw += lookInput.x * sensitivityX * Time.deltaTime;
        float y = lookInput.y * sensitivityY * Time.deltaTime;
        pitch += (invertY ? y : -y);
        pitch = Mathf.Clamp(pitch, -verticalClamp, verticalClamp);

        // clear lookInput when using legacy polling to avoid accumulation
        if (Mouse.current == null)
            lookInput = Vector2.zero;
    }

    void LateUpdate()
    {
        if (player == null) return;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = player.position + rotation * offset;

        // Collision check: cast from player to desired camera position
        Vector3 dir = desiredPosition - player.position;
        float dist = dir.magnitude;
        if (dist > 0.001f)
        {
            if (Physics.Raycast(player.position, dir.normalized, out RaycastHit hit, dist + collisionOffset, collisionMask))
            {
                desiredPosition = hit.point - dir.normalized * collisionOffset;
            }
        }

        // Smooth position and rotation
        transform.position = Vector3.Slerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
        Quaternion lookRotation = Quaternion.LookRotation(player.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * smoothSpeed);
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Utility to toggle cursor lock at runtime
    public void ToggleCursorLock()
    {
        lockCursor = !lockCursor;
        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockCursor;
    }
}

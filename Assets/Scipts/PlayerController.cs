using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    public float jumpForce = 12f;
    
    [Header("Ground Check")]
    public LayerMask groundLayer;
    
    [Header("Camera")]
    public Transform cameraTransform;
    public float cameraDistance = 10f;
    public float cameraHeight = 5f;
    public float mouseSensitivity = 2f;
    public bool useMouseLook = true;
    
    [Header("Game UI")]
    public TextMeshProUGUI countText;
    public GameObject winTextObject;
    
    private Rigidbody rb;
    private int count;
    private float movementX;
    private float movementY;
    private bool isGrounded;
    private bool wasGrounded;
    private float cameraRotationX = 0f;
    private float cameraRotationY = 0f;
    private Vector2 lookInput;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Initialize UI
        count = 0;
        SetCountText();
        
        if (winTextObject != null)
        {
            winTextObject.SetActive(false);
        }
        
        // Setup rigidbody for ball physics
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        
        // Setup camera if not assigned
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }
    
    void Update()
    {
        // Check if grounded
        wasGrounded = isGrounded;
        
        // Mouse camera control
        if (useMouseLook)
        {
            cameraRotationY += lookInput.x * mouseSensitivity;
            cameraRotationX -= lookInput.y * mouseSensitivity;
            cameraRotationX = Mathf.Clamp(cameraRotationX, -45f, 60f);
        }
        
        // Update camera position
        UpdateCamera();
    }
    
    void FixedUpdate()
    {
        // Calculate movement relative to camera direction
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        // Apply movement force relative to camera (ball rolling physics)
        Vector3 movement = (cameraForward * movementY + cameraRight * movementX);
        rb.AddForce(movement * speed);
    }
    
    void UpdateCamera()
    {
        if (cameraTransform != null)
        {
            // Position camera with rotation around the ball
            Quaternion rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0);
            Vector3 offset = rotation * new Vector3(0, cameraHeight, -cameraDistance);
            cameraTransform.position = transform.position + offset;
            cameraTransform.LookAt(transform.position);
        }
    }
    
    // New Input System callbacks
    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementY = movementVector.y;
    }
    
    void OnLook(InputValue lookValue)
    {
        lookInput = lookValue.Get<Vector2>();
    }
    
    void OnJump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }
    
    void SetCountText()
    {
        if (countText != null)
        {
            countText.text = "Count: " + count.ToString();
        }
        
        // Win condition
        if (count >= 16)
        {
            if (winTextObject != null)
            {
                winTextObject.SetActive(true);
            }
            
            GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Collect pickups
        if (other.gameObject.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
            count++;
            SetCountText();
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Check for ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        
        // Check for enemy - lose condition
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
            
            if (winTextObject != null)
            {
                winTextObject.SetActive(true);
                winTextObject.GetComponent<TextMeshProUGUI>().text = "You Lose!";
            }
        }
    }
    
    void OnCollisionExit(Collision collision)
    {
        // No longer grounded when leaving ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
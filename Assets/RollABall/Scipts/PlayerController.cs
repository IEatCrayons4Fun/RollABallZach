using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public enum PlayerType { Sphere, Capsule }
    
    public PlayerType playerType = PlayerType.Sphere;

    
    private int health;

    
    public float boostMultiplier = 2f;      // how much faster
    public float boostDuration = 5f;        // how long it lasts
    private bool boostActive = false;
    private float boostEndTime;
    private float baseSpeed;                // store original speed
    private float baseMaxSpeed;             // store original max speed

    
    public float speed = 10f;
    public float maxSpeed = 20f; // NEW: Maximum speed cap
    public float acceleration = 50f;
    public float airControl = 0.3f;
    public float jumpForce = 12f;
    public float brakeForce = 10f;
    public float coyoteTime = 0.15f; // NEW: Grace period for jumping after leaving ground

    [Header("Ground Detection")] // NEW: Better ground detection
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;

    [Header("Camera")]
    public Transform cameraTransform;
    public float cameraDistance = 10f;
    public float cameraHeight = 5f;
    public float mouseSensitivity = 2f;
    public bool useMouseLook = true;

    [Header("UI")]
    public TextMeshProUGUI countText;
    public GameObject winTextObject;
    [SerializeField] private float textDisplayDuration = 2f;
    [SerializeField] private float fadeDuration = 1.5f;
    private TextMeshProUGUI healthText;

    private Rigidbody rb;
    private int count;
    private int maxCount = 105;
    private float movementX;
    private float movementY;
    private bool isGrounded;
    private float lastGroundedTime; // NEW: For coyote time
    private float cameraRotationX = 0f;
    private float cameraRotationY = 0f;
    private Vector2 lookInput;
    public InputAction unlockCursorAction;
    private string[] tagsToDestroy = {"PickUp", "BigPickUp", "InstaWin", "HidingDoor", "Enemy"};
    private bool shouldBrake = false;

    void OnEnable()
    {
        unlockCursorAction.Enable();
    }
    
    void OnDisable()
    {
        unlockCursorAction.Disable();
    }
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        
        baseSpeed = speed;
        baseMaxSpeed = maxSpeed;

        if (playerType == PlayerType.Capsule)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            healthText = GameObject.Find("HealthText")?.GetComponent<TextMeshProUGUI>(); // IMPROVED: Null safety
            health = 100;
            SetHealthText();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        count = 0;
        SetCountText();
        if (winTextObject != null) winTextObject.SetActive(false);
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
        
        // Set default ground layer if not set
        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Default");
        }
    }

    void Update()
    {
        if (boostActive && Time.time > boostEndTime)
        {
            speed = baseSpeed;
            maxSpeed = baseMaxSpeed;
            boostActive = false;
        }
        if (useMouseLook)
        {
            cameraRotationY += lookInput.x * mouseSensitivity;
            cameraRotationX -= lookInput.y * mouseSensitivity;
            cameraRotationX = Mathf.Clamp(cameraRotationX, -45f, 60f);
        }
        
        if (unlockCursorAction.triggered)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        shouldBrake = Keyboard.current.leftShiftKey.isPressed;

        UpdateCamera();
        CheckGrounded(); // NEW: Better ground detection
    }

    void FixedUpdate()
    {
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 movement = (cameraForward * movementY + cameraRight * movementX);

        if (playerType == PlayerType.Sphere)
        {
            Vector3 torque = (cameraTransform.right * movementY - cameraTransform.forward * movementX) * speed;
            
            // Only limit speed when accelerating in the direction you're already moving
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            float currentSpeed = horizontalVelocity.magnitude;
            
            if (currentSpeed < maxSpeed)
            {
                // Under max speed, apply full torque
                rb.AddTorque(torque);
            }
            else
            {
                // At max speed, only allow torque perpendicular to velocity (steering)
                Vector3 velocityDir = horizontalVelocity.normalized;
                Vector3 torqueDir = torque.normalized;
                float alignment = Vector3.Dot(velocityDir, Vector3.Cross(torqueDir, Vector3.up));
                
                // If trying to accelerate forward, limit it. If turning, allow it.
                if (Mathf.Abs(alignment) > 0.5f)
                {
                    rb.AddTorque(torque * 0.3f); // Allow some steering
                }
            }
            
            // Apply brake to angular velocity too
            if (shouldBrake)
            {
                ApplyBrake();
                rb.angularVelocity *= 0.95f; // Slow down rotation
            }
        }
        else if (playerType == PlayerType.Capsule)
        {
            float controlMultiplier = isGrounded ? 1f : airControl;
            Vector3 targetVelocity = movement * speed * controlMultiplier;
            Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            
            // Check speed limit
            if (currentVelocity.magnitude < maxSpeed || Vector3.Dot(targetVelocity.normalized, currentVelocity.normalized) < 0)
            {
                Vector3 velocityChange = (targetVelocity - currentVelocity);
                velocityChange = Vector3.ClampMagnitude(velocityChange, acceleration * Time.fixedDeltaTime);
                rb.AddForce(new Vector3(velocityChange.x, 0, velocityChange.z), ForceMode.VelocityChange);
            }
            
            if (shouldBrake)
            {
                ApplyBrake();
            }
        }
    }

    // NEW: Improved ground detection using raycast
    void CheckGrounded()
    {
        float checkDistance = groundCheckDistance;
        if (playerType == PlayerType.Sphere)
        {
            checkDistance += GetComponent<SphereCollider>() ? GetComponent<SphereCollider>().radius : 0.5f;
        }
        else
        {
            checkDistance += GetComponent<CapsuleCollider>() ? GetComponent<CapsuleCollider>().height * 0.5f : 0.5f;
        }

        bool wasGrounded = isGrounded;
        
        // Update coyote time
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
        }
        
        // Debug visualization - ENABLE THIS to see if raycast is hitting
        Debug.DrawRay(transform.position, Vector3.down * checkDistance, isGrounded ? Color.green : Color.red);
    }

    void UpdateCamera()
    {
        Quaternion rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0);
        Vector3 offset = rotation * new Vector3(0, cameraHeight, -cameraDistance);
        cameraTransform.position = transform.position + offset;
        cameraTransform.LookAt(transform.position);
    }

    public void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    public void OnLook(InputValue lookValue)
    {
        lookInput = lookValue.Get<Vector2>();
    }

    public void OnJump()
    {
        
        if (isGrounded || Time.time - lastGroundedTime < coyoteTime)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            lastGroundedTime = 0;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Death();
        }
        
        if(playerType == PlayerType.Capsule && collision.gameObject.CompareTag("Hazard"))
        {
            health -= 20;
            SetHealthText();
            if (health <= 0)
            {
                Death();
            }
        }
        
        if(playerType == PlayerType.Capsule && collision.gameObject.CompareTag("Lava"))
        {
            health -= 100;
            SetHealthText();
            if (health <= 0)
            {
                Death();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
            count++;
        }
        else if (other.CompareTag("BigPickUp"))
        {
            other.gameObject.SetActive(false);
            count += 5;
        }
        else if (other.CompareTag("InstaWin"))
        {
            other.gameObject.SetActive(false);
            count += 105;
        }
        else if (other.CompareTag("SpeedPickUp")){

            other.gameObject.SetActive(false);
            ActiateSpeedBoost();
        }
        SetCountText();
    }

    void SetCountText()
    {
        count = Mathf.Clamp(count, 0, maxCount);

        if (countText != null) 
            countText.text = "Count: " + count.ToString();

        if (count == maxCount && winTextObject != null)
        {
            TriggerWin();
        }
    }
    
    void SetHealthText()
    {
        if (healthText != null) healthText.text = "Health: " + health.ToString();
    }
    
    void ApplyBrake()
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        
        if (horizontalVelocity.magnitude > 0.1f)
        {
            Vector3 brakeForceVector = -horizontalVelocity.normalized * brakeForce;

            if (horizontalVelocity.magnitude < brakeForce * Time.fixedDeltaTime)
            {
                brakeForceVector = -horizontalVelocity / Time.fixedDeltaTime;
            }

            rb.AddForce(brakeForceVector, ForceMode.Acceleration);
        }
    }

    void ActiateSpeedBoost()
    {
        speed = baseSpeed * boostMultiplier;
        maxSpeed = baseMaxSpeed * boostMultiplier;
        boostEndTime = Time.time + boostDuration;
        boostActive = true;
    }
    
    public void TriggerWin()
    {
        // FIXED: Removed duplicate destroy logic
        foreach (string tag in tagsToDestroy)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objects)
            {
                Destroy(obj);
            }
        }
        
        if (winTextObject != null)
        {
            winTextObject.SetActive(true);
            TextMeshProUGUI tmp = winTextObject.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = "Find the Door to the Next Level!";
                StartCoroutine(FadeOutText(tmp));
            }
        }
        GameObject torch = GameObject.FindWithTag("Torch");
        if (torch != null) torch.SetActive(true);
    }
    
    private IEnumerator FadeOutText(TextMeshProUGUI tmp)
    {
        yield return new WaitForSeconds(textDisplayDuration);

        float elapsed = 0f;
        Color originalColor = tmp.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            tmp.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        winTextObject.SetActive(false);
    }

    public void Death()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("DeathScreen");
    }

    public int GetCount()
    {
        return count;
    }

    public void TakeDamage(int damageAmount)
    {
        if (playerType == PlayerType.Capsule)
        {
            health -= damageAmount;
            SetHealthText();
            
            if (health <= 0)
            {
                Death();
            }
        }
    }
}

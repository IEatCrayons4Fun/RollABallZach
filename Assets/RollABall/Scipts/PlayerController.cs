using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public enum PlayerType { Sphere, Capsule }
    
    public PlayerType playerType = PlayerType.Sphere;

    private int health = 100;


    [Header("Movement")]
    public float speed = 10f;
    public float maxSpeed = 20f;
    public float acceleration = 50f;
    public float airControl = 0.3f;
    public float jumpForce = 12f;
    public float brakeForce = 10f;
    public float steeringMultiplier = 0.3f;
    public float angularDamping = 0.95f;



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
    private float cameraRotationX = 0f;
    private float cameraRotationY = 0f;
    private Vector2 lookInput;
    public InputAction unlockCursorAction;
    private string[] tagsToDestroy = {"PickUp", "BigPickUp", "InstaWin", "HidingDoor", "Enemy", "SpeedPickUp"};
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

        if (playerType == PlayerType.Capsule)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            GameObject healthTextObj = GameObject.Find("HealthText");
            if (healthTextObj != null)
            {
                healthText = healthTextObj.GetComponent<TextMeshProUGUI>();
            }
            SetHealthText();
            GameObject countTextObj = GameObject.Find("CountText");
            if (countTextObj != null) Destroy(countTextObj);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        count = 0;
        SetCountText();
        if (winTextObject != null) winTextObject.SetActive(false);
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        
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
            
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            float currentSpeed = horizontalVelocity.magnitude;
            
            if (currentSpeed < maxSpeed)
            {
                rb.AddTorque(torque);
            }
            else
            {
                Vector3 velocityDir = horizontalVelocity.normalized;
                Vector3 torqueDir = torque.normalized;
                float alignment = Vector3.Dot(velocityDir, Vector3.Cross(torqueDir, Vector3.up));
                
                if (Mathf.Abs(alignment) > 0.5f)
                {
                    rb.AddTorque(torque * steeringMultiplier);
                }
            }
            
            if (shouldBrake)
            {
                ApplyBrake();
                rb.angularVelocity *= angularDamping;
            }
        }
        else if (playerType == PlayerType.Capsule)
        {
            float controlMultiplier = isGrounded ? 1f : airControl;
            Vector3 targetVelocity = movement * speed * controlMultiplier;
            Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            
            if (currentVelocity.magnitude < maxSpeed || Vector3.Dot(targetVelocity.normalized, currentVelocity.normalized) < 0)
            {
                Vector3 velocityChange = (targetVelocity - currentVelocity);
                velocityChange = Vector3.ClampMagnitude(velocityChange, acceleration * Time.fixedDeltaTime);
                rb.AddForce(new Vector3(velocityChange.x, 0, velocityChange.z), ForceMode.VelocityChange);
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
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
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == null) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Death();
        }
        
        if (playerType == PlayerType.Capsule && collision.gameObject.CompareTag("Hazard"))
        {
            health -= 20;
            SetHealthText();
            if (health <= 0)
            {
                Death();
            }
        }
        
        if (playerType == PlayerType.Capsule && collision.gameObject.CompareTag("Lava"))
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
        health = Mathf.Clamp(health, 0, 100);
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

    
    public void TriggerWin()
    {
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
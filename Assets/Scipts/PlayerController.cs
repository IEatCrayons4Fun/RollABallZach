using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public enum PlayerType { Sphere, Capsule }
    [Header("Player Type")]
    public PlayerType playerType = PlayerType.Sphere;

    [Header("Movement")]
    public float speed = 10f;
    public float jumpForce = 12f;

    [Header("Camera")]
    public Transform cameraTransform;
    public float cameraDistance = 10f;
    public float cameraHeight = 5f;
    public float mouseSensitivity = 2f;
    public bool useMouseLook = true;

    [Header("UI")]
    public TextMeshProUGUI countText;
    public GameObject winTextObject;

    private Rigidbody rb;
    private int count;
    private float movementX;
    private float movementY;
    private bool isGrounded;
    private float cameraRotationX = 0f;
    private float cameraRotationY = 0f;
    private Vector2 lookInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (playerType == PlayerType.Capsule)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
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
            rb.AddTorque(torque);
        }
        else if (playerType == PlayerType.Capsule)
        {
            Vector3 velocity = movement * speed;
            rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
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
            isGrounded = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) isGrounded = true;

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
        if (collision.gameObject.CompareTag("Ground")) isGrounded = false;
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
        if (countText != null) countText.text = "Count: " + count.ToString();

        if (count >= 105 && winTextObject != null)
        {
            winTextObject.SetActive(true);
            GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
            if (enemy != null) Destroy(enemy);
        }
    }
}

using UnityEngine;

// Attach this to your pickup prefab
public class EnemyTeleportPickup : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Vector3 teleportPosition = Vector3.zero; // Center of the world (0, 0, 0)
    public bool useRelativeToPlayer = false; // If true, teleport relative to player instead
    public Vector3 playerOffset = new Vector3(0, 0, -5); // Offset from player if using relative mode
    public string enemyTag = "Enemy"; // Tag for enemies
    
    [Header("Visual Settings")]
    public bool rotatePickup = true;
    public float rotationSpeed = 100f;
    public bool bobUpDown = true;
    public float bobSpeed = 2f;
    public float bobHeight = 0.5f;
    
    [Header("Pickup Behavior")]
    public bool destroyAfterUse = true; // If true, pickup disappears after one use
    public float cooldown = 1f; // Cooldown between uses if not destroyed
    
    [Header("Audio (Optional)")]
    public AudioClip teleportSound;
    
    private Vector3 startPosition;
    private float cooldownTimer = 0f;
    private bool onCooldown = false;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Handle cooldown
        if (onCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                onCooldown = false;
            }
        }
        
        // Rotate the pickup
        if (rotatePickup)
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
        
        // Bob up and down
        if (bobUpDown)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if it's an enemy and not on cooldown
        if (other.CompareTag(enemyTag) && !onCooldown)
        {
            TeleportEnemy(other.gameObject);
            
            // Play sound if available
            if (teleportSound != null)
            {
                AudioSource.PlayClipAtPoint(teleportSound, transform.position);
            }
            
            if (destroyAfterUse)
            {
                Destroy(gameObject);
            }
            else
            {
                // Start cooldown
                onCooldown = true;
                cooldownTimer = cooldown;
            }
        }
    }

    void TeleportEnemy(GameObject enemy)
    {
        Debug.Log("Teleporting " + enemy.name + " to center!");
        
        // Teleport enemy to center
        enemy.transform.position = teleportPosition;
        
        // Optional: Reset enemy velocity if it has a Rigidbody
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
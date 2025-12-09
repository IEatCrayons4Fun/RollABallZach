using UnityEngine;

// Attach this to your speed pickup prefab
public class SpeedPickup : MonoBehaviour
{
    [Header("Speed Boost Settings")]
    public float speedIncrease = 5f; // How much to add to speed
    public bool isMultiplier = false; // If true, multiplies speed instead of adding
    public float multiplier = 1.5f; // Use this if isMultiplier is true
    
    [Header("Duration Settings")]
    public bool isPermanent = false;
    public float boostDuration = 10f; // How long boost lasts (if not permanent)
    
    [Header("Visual Settings")]
    public bool rotatePickup = true;
    public float rotationSpeed = 100f;
    public bool bobUpDown = true;
    public float bobSpeed = 2f;
    public float bobHeight = 0.5f;
    
    [Header("Audio (Optional)")]
    public AudioClip pickupSound;
    
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
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
        if (other.CompareTag("Player"))
        {
            // Try to find the PlayerController script (adjust name if different)
            PlayerController player = other.GetComponent<PlayerController>();
            
            if (player == null)
            {
                // Try alternate common names
                player = other.GetComponent<PlayerController>();
            }
            
            if (player != null)
            {
                ApplySpeedBoost(player);
                
                // Play sound if available
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }
                
                // Destroy the pickup
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("PlayerController script not found on player!");
            }
        }
    }

    void ApplySpeedBoost(PlayerController player)
    {
        if (isMultiplier)
        {
            // Multiply the speed
            if (isPermanent)
            {
                player.speed *= multiplier;
                Debug.Log("Speed permanently increased to: " + player.speed);
            }
            else
            {
                StartCoroutine(TemporaryMultiplierBoost(player));
            }
        }
        else
        {
            // Add to the speed
            if (isPermanent)
            {
                player.speed += speedIncrease;
                Debug.Log("Speed permanently increased to: " + player.speed);
            }
            else
            {
                StartCoroutine(TemporaryAdditiveBoost(player));
            }
        }
    }

    System.Collections.IEnumerator TemporaryAdditiveBoost(PlayerController player)
    {
        float originalSpeed = player.speed;
        player.speed += speedIncrease;
        
        Debug.Log("Speed boosted to: " + player.speed + " for " + boostDuration + " seconds");
        
        yield return new WaitForSeconds(boostDuration);
        
        player.speed = originalSpeed;
        Debug.Log("Speed returned to: " + player.speed);
    }

    System.Collections.IEnumerator TemporaryMultiplierBoost(PlayerController player)
    {
        float originalSpeed = player.speed;
        player.speed *= multiplier;
        
        Debug.Log("Speed boosted to: " + player.speed + " for " + boostDuration + " seconds");
        
        yield return new WaitForSeconds(boostDuration);
        
        player.speed = originalSpeed;
        Debug.Log("Speed returned to: " + player.speed);
    }
}
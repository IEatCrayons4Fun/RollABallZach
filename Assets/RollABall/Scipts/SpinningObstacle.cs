using UnityEngine;

// Attach this to the PARENT spinning object
public class SpinningHazard : MonoBehaviour
{
    [Header("Spinning Settings")]
    public float spinSpeed = 100f;
    public Vector3 spinAxis = Vector3.up; // Which axis to spin on (up = Y-axis)
    
    [Header("Flash Settings")]
    public float flashSpeed = 0.3f; // How fast it flashes (lower = faster)
    public Color normalColor = Color.white;
    public Color dangerColor = Color.red;
    
    [Header("Dangerous Parts")]
    public string dangerousPartTag = "Capsule"; // Tag for dangerous parts (or leave empty to use name)
    public bool useName = true; // If true, looks for GameObjects with "Capsule" in name
    
    private DangerousPart[] dangerousParts;
    private float flashTimer = 0f;
    private bool isRed = false;

    void Start()
    {
        // Find all capsule children
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        System.Collections.Generic.List<DangerousPart> partsList = new System.Collections.Generic.List<DangerousPart>();
        
        foreach (Transform child in allChildren)
        {
            bool isCapsule = false;
            
            if (useName)
            {
                // Check if name contains "Capsule"
                isCapsule = child.name.ToLower().Contains("capsule");
            }
            else
            {
                // Check by tag
                isCapsule = child.CompareTag(dangerousPartTag);
            }
            
            if (isCapsule)
            {
                Renderer renderer = child.GetComponent<Renderer>();
                Collider collider = child.GetComponent<Collider>();
                
                if (renderer != null && renderer.material != null)
                {
                    DangerousPart part = child.gameObject.AddComponent<DangerousPart>();
                    part.Initialize(this, renderer.material, normalColor);
                    partsList.Add(part);
                }
            }
        }
        
        dangerousParts = partsList.ToArray();
    }

    void Update()
    {
        // Spin the object
        transform.Rotate(spinAxis * spinSpeed * Time.deltaTime);
        
        // Handle rapid flashing
        flashTimer += Time.deltaTime;
        
        if (flashTimer >= flashSpeed)
        {
            // Toggle between colors rapidly
            isRed = !isRed;
            Color targetColor = isRed ? dangerColor : normalColor;
            
            foreach (DangerousPart part in dangerousParts)
            {
                if (part != null)
                {
                    part.SetColor(targetColor);
                }
            }
            
            flashTimer = 0f;
        }
    }

    public void KillPlayer(GameObject player)
    {
        Debug.Log("Player killed by spinning hazard!");
        
        // Get the PlayerController and call Death function
        PlayerController playerController = player.GetComponent<PlayerController>();
        
        if (playerController != null)
        {
            playerController.Death();
        }
        else
        {
            Debug.LogError("PlayerController not found on player!");
        }
    }
}

// This component is automatically added to each dangerous part
public class DangerousPart : MonoBehaviour
{
    private SpinningHazard parentHazard;
    private Material partMaterial;
    private Color originalColor;

    public void Initialize(SpinningHazard parent, Material material, Color original)
    {
        parentHazard = parent;
        partMaterial = material;
        originalColor = original;
    }

    public void SetColor(Color color)
    {
        if (partMaterial != null)
        {
            partMaterial.color = color;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // ALWAYS kills player on contact
        if (collision.gameObject.CompareTag("Player"))
        {
            parentHazard.KillPlayer(collision.gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // ALWAYS kills player on contact
        if (other.CompareTag("Player"))
        {
            parentHazard.KillPlayer(other.gameObject);
        }
    }
}
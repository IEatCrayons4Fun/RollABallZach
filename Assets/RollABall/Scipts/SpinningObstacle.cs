using UnityEngine;

public class BouncyObstacle : MonoBehaviour
{
    [Header("Sphere Settings")]
    public float bounceForce = 15f;
    
    [Header("Capsule Settings")]
    public int damageAmount = 20;

    [Header("Rotation Settings")]
    public float rotationSpeed = 50f; // Degrees per second
    public Vector3 rotationAxis = Vector3.up; // Which axis to spin around (up = Y-axis)
    
    void Update()
    {
        // Rotate around the specified axis
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
        
        if (pc != null)
        {
            if (pc.playerType == PlayerController.PlayerType.Sphere)
            {
                // Bounce the sphere
                Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 bounceDirection = collision.contacts[0].normal;
                    rb.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);
                }
            }
            else // Capsule
            {
                // Damage the capsule using the new TakeDamage method
                pc.TakeDamage(damageAmount);
            }
        }

        GameObject enemy = GameObject.FindWithTag("Enemy");
        if (enemy != null)
        {
           Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 bounceDirection = collision.contacts[0].normal;
                    rb.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);
                } 
        }
    }
}
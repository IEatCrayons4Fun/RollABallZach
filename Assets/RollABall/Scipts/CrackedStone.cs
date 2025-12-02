using UnityEngine;

public class CrackedStone : MonoBehaviour
{   
    public float disappearDelay = 3f;
    public AudioClip disappearSound;
    public AudioSource audioSource;
    private bool triggered = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter(Collision collision)
    {
        if (!triggered && collision.gameObject.CompareTag("Player"))
        {
            triggered = true;
            Invoke(nameof(Disappear), disappearDelay);
        }
    }
    void Disappear()
    {
        if (disappearSound != null)
        {
            audioSource.PlayOneShot(disappearSound);
        }
        Destroy(gameObject, disappearSound != null ? disappearSound.length : 0f);
    }
}

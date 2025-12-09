using UnityEngine;

public class DeathBox : MonoBehaviour
{
    [SerializeField] private PlayerController player;

    void OnTriggerEnter(Collider other)
    {
        player.Death();
    }
}

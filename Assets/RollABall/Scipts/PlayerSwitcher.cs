using UnityEngine;

public class PlayerSwitcher : MonoBehaviour
{
    public GameObject capsulePrefab; // assign in Inspector
    private bool switched = false;

    void OnTriggerEnter(Collider other)
    {
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        if (enemy != null) Destroy(enemy);
        if (!switched && other.CompareTag("Player"))
        {
            // Save position of ball before destroying
            Vector3 pos = other.transform.position;

            // Use upright rotation for capsule
            Quaternion uprightRotation = Quaternion.Euler(0, other.transform.eulerAngles.y, 0);

            // Destroy ball player (and its camera)
            Destroy(other.gameObject);

            // Spawn capsule player at same spot
            Instantiate(capsulePrefab, pos, uprightRotation);

            switched = true;
        }
        
        GameObject.Find("Arrow System").SetActive(false);
    }
}

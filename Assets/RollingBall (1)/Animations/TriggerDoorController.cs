using UnityEngine;

public class TriggerDoorController : MonoBehaviour
{
    [SerializeField] private Animator LeftDoorAnimator;
    [SerializeField] private Animator RightDoorAnimator;

    [SerializeField] private bool openTrigger = false;
    [SerializeField] private bool closeTrigger = false;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (openTrigger){
                LeftDoorAnimator.Play("Left Door Open", 0, 0.0f);
                RightDoorAnimator.Play("Right Door Open", 0, 0.0f);
            }
            else if (closeTrigger){
                LeftDoorAnimator.Play("Left Door Close", 0, 0.0f);
                RightDoorAnimator.Play("Right Door Close", 0, 0.0f);
            }
        }
    }
}

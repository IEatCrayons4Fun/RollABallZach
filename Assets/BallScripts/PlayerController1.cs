using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController1 : MonoBehaviour
{


    private Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponet<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    void OnMove(InputValue movementValue){
        Vector2 movementVector = movementValue.Get<Vector2>();
        
    }
        
    private void FixedUpdate(){
        rb.AddForce(movement);
    }
}


using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float speed = 0;
    public float jumpForce = 5;
    public TextMeshProUGUI countText;
    public GameObject winTextObject;

    private Rigidbody rb;
    private int count;
    private float movementX;
    private float movementY;
    private bool isGrounded;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        count = 0;
        SetCountText();
        rb = GetComponent<Rigidbody>();
        winTextObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    void OnMove(InputValue movementValue){
        Vector2 movementVector = movementValue.Get<Vector2>();
        
        movementX = movementVector.x;
        movementY = movementVector.y; 
    }

    void OnJump(){
        if(isGrounded){
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }
    void SetCountText(){
        countText.text = "Count: " + count.ToString();
        if (count == 18){
            winTextObject.SetActive(true);
            Destroy(GameObject.FindGameObjectWithTag("Enemy"));
        }
    }
        
    private void FixedUpdate(){
        Vector3 movement = new Vector3(movementX, 0.0f, movementY);
        rb.AddForce(movement * speed);

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
            count = count + 1;
            SetCountText();
        } 
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
            winTextObject.SetActive(true);
            winTextObject.GetComponent<TextMeshProUGUI>().text = "You Lose!";
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}


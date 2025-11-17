using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Transform player;       // reference to the ball
    public float sensitivity = 100f;
    public float distance = 5f;    // how far the camera stays back
    public float verticalClamp = 80f;

    private float yaw = 0f;        // horizontal rotation
    private float pitch = 0f;      // vertical rotation

    void OnLook(InputValue value)
    {
        Vector2 lookInput = value.Get<Vector2>();
        yaw += lookInput.x * sensitivity * Time.deltaTime;
        pitch -= lookInput.y * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -verticalClamp, verticalClamp);
    }

    void LateUpdate()
    {
        // calculate rotation
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // position camera behind player
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        transform.position = player.position + offset;

        // look at player
        transform.LookAt(player.position);
    }
}

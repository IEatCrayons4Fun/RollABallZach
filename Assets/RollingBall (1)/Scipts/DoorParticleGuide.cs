using UnityEngine;

public class DoorArrowGuide : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform door;
    
    [Header("Arrow Settings")]
    [SerializeField] private Color arrowColor = Color.cyan;
    [SerializeField] private float arrowLength = 3f;
    [SerializeField] private float arrowWidth = 0.2f;
    [SerializeField] private float arrowHeadSize = 0.5f;
    [SerializeField] private float heightOffset = 1f; // Height above player
    
    [Header("Behavior")]
    [SerializeField] private bool onlyShowAfterWin = true;
    
    private LineRenderer lineRenderer;
    private LineRenderer arrowHead;
    private PlayerController playerController;
    private Transform player;
    private bool isActive = false;

    void Start()
    {
        // Find player
        playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            player = playerController.transform;
        }
        
        // If door not assigned, assume this script is on the door
        if (door == null)
        {
            door = transform;
        }
        
        CreateArrow();
        
        if (onlyShowAfterWin)
        {
            lineRenderer.enabled = false;
            arrowHead.enabled = false;
        }
        else
        {
            isActive = true;
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Check if player is capsule - if so, hide arrow permanently
        if (playerController != null && playerController.playerType == PlayerController.PlayerType.Capsule)
        {
            if (lineRenderer != null && lineRenderer.enabled)
            {
                lineRenderer.enabled = false;
            }
            if (arrowHead != null && arrowHead.enabled)
            {
                arrowHead.enabled = false;
            }
            isActive = false;
            return;
        }
        
        // Check if should activate after win
        if (onlyShowAfterWin && !isActive && playerController != null)
        {
            if (playerController.GetCount() >= 105)
            {
                ActivateArrow();
            }
        }
        
        if (isActive && lineRenderer != null && lineRenderer.enabled)
        {
            UpdateArrow();
        }
    }

    void CreateArrow()
    {
        // Create main arrow line
        GameObject lineGO = new GameObject("ArrowLine");
        lineGO.transform.parent = transform;
        lineRenderer = lineGO.AddComponent<LineRenderer>();
        
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = arrowColor;
        lineRenderer.endColor = arrowColor;
        lineRenderer.startWidth = arrowWidth;
        lineRenderer.endWidth = arrowWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        
        // Create arrowhead (triangle)
        GameObject arrowHeadGO = new GameObject("ArrowHead");
        arrowHeadGO.transform.parent = transform;
        arrowHead = arrowHeadGO.AddComponent<LineRenderer>();
        
        arrowHead.material = new Material(Shader.Find("Sprites/Default"));
        arrowHead.startColor = arrowColor;
        arrowHead.endColor = arrowColor;
        arrowHead.startWidth = arrowWidth;
        arrowHead.endWidth = arrowWidth;
        arrowHead.positionCount = 4; // Triangle shape
        arrowHead.useWorldSpace = true;
        arrowHead.loop = true;
    }

    void UpdateArrow()
    {
        if (player == null || door == null) return;
        
        // Calculate direction from player to door (horizontal only)
        Vector3 playerPos = player.position + Vector3.up * heightOffset;
        Vector3 doorPos = door.position;
        
        Vector3 direction = new Vector3(doorPos.x - playerPos.x, 0, doorPos.z - playerPos.z);
        direction.Normalize();
        
        // Calculate arrow end point
        Vector3 arrowEnd = playerPos + direction * arrowLength;
        
        // Update main line
        lineRenderer.SetPosition(0, playerPos);
        lineRenderer.SetPosition(1, arrowEnd);
        
        // Calculate arrowhead triangle
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
        Vector3 arrowTip = arrowEnd + direction * arrowHeadSize;
        Vector3 arrowLeft = arrowEnd + perpendicular * arrowHeadSize * 0.5f;
        Vector3 arrowRight = arrowEnd - perpendicular * arrowHeadSize * 0.5f;
        
        // Update arrowhead
        arrowHead.SetPosition(0, arrowTip);
        arrowHead.SetPosition(1, arrowLeft);
        arrowHead.SetPosition(2, arrowEnd);
        arrowHead.SetPosition(3, arrowRight);
    }

    public void ActivateArrow()
    {
        if (lineRenderer != null && arrowHead != null)
        {
            isActive = true;
            lineRenderer.enabled = true;
            arrowHead.enabled = true;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (player != null && door != null)
        {
            Gizmos.color = arrowColor;
            Vector3 start = player.position + Vector3.up * heightOffset;
            Vector3 end = door.position;
            end.y = start.y;
            Gizmos.DrawLine(start, end);
        }
    }
}
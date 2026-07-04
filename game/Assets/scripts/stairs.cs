using UnityEngine;

public class stairs : MonoBehaviour
{
    public Transform targetStairs; 
    public float cooldown = 1f;
    
    private static float lastTeleportTime = -1f;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (Time.time - lastTeleportTime < cooldown)
                return;
            
            if (targetStairs != null)
            {
                lastTeleportTime = Time.time;
                
                
                Vector3 targetPosition = targetStairs.position;
                collision.transform.position = targetPosition;
                
                
            }
        }
    }
}
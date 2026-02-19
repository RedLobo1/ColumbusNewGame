using UnityEngine;

public class InterpolatedItem : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    public bool isOverlapping { get; private set; } = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            isOverlapping = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            isOverlapping = false;
    }
}

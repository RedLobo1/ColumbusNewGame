using UnityEngine;

public class StopMinigameItem : MonoBehaviour
{
    [SerializeField] private string destinationTag = "Destination";

    private Animator animator;
    private static readonly int StopHash = Animator.StringToHash("Stop");

    public bool isOverlapping { get; private set; } = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayStopAnimation()
    {

        //PUT ANIMATION NAME ON FLAG OBJECT HERE
        if (animator != null)
            animator.Play("PlantFlag"); 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(destinationTag))
            isOverlapping = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(destinationTag))
            isOverlapping = false;
    }
}
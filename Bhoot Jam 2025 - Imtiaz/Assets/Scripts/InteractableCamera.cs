using UnityEngine;

public class InteractableCamera : MonoBehaviour
{
    public Transform normalPosition;      // Default camera position
    public Transform interactPosition;    // Target position when right-click is held
    public float moveSpeed = 5f;          // Speed of movement

    private Transform targetTransform;
    private TaskManager taskManager;
    public bool captionSpamPrevent;
    void Start()
    {
        // Set initial target as normal position
        targetTransform = normalPosition;
        taskManager = GameObject.FindGameObjectWithTag("Player").GetComponent<TaskManager>();
    }

    void Update()
    {
        // Update target based on right mouse button
        if (Input.GetMouseButton(1)) // Right mouse button held
        {
            targetTransform = interactPosition;
            if (!captionSpamPrevent)
            {
                taskManager.caption.text = "";
                captionSpamPrevent = true;
            }
        }
        else
        {
            targetTransform = normalPosition;
        }

        // Smooth position transition
        transform.position = Vector3.Lerp(transform.position, targetTransform.position, Time.deltaTime * moveSpeed);
    }
}

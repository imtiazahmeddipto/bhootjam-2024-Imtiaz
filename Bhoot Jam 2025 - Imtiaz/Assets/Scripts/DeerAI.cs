using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class DeerAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float patrolRadius = 10f;
    public float moveSpeed = 3f;
    public float idleTime = 3f;
    public GameObject BloodFX;
    public float deathForce = 10f;

    private NavMeshAgent agent;
    private Animator animator;
    private Vector3 homePosition;
    private bool isIdling;
    private bool isDead;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private MeshCollider mainCollider;
    public AudioSource audioSource;
    public AudioClip dieSFX;
    public GameObject Meat;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        homePosition = transform.position;
        agent.speed = moveSpeed;
        mainCollider = GetComponent<MeshCollider>();
        // Ragdoll setup
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
        ToggleRagdoll(false);

        SetNewDestination();
    }

    void ToggleRagdoll(bool state)
    {
        // Enable/disable animator and main collider
        animator.enabled = !state;
        if (mainCollider != null)
            mainCollider.enabled = !state;

        // Toggle ragdoll physics
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = !state;
            rb.interpolation = state ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
        }

        foreach (Collider col in ragdollColliders)
        {
            // Skip the main collider since we handle it separately
            if (col != mainCollider)
                col.enabled = state;
        }

        // Disable NavMeshAgent
        agent.enabled = !state;
    }

    void Update()
    {
        if (isDead) return;

        animator.SetFloat("Speed", agent.velocity.magnitude);

        if (!isIdling && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(IdleThenMove());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet") && !isDead)
        {
            Die(collision);
            Destroy(Instantiate(BloodFX, collision.transform.position, Quaternion.identity), 10f);
        }
    }

    void Die(Collision collision)
    {
        isDead = true;
        audioSource.PlayOneShot(dieSFX);
        MeatExtract();
        // Enable ragdoll
        ToggleRagdoll(true);

        // Apply force to ragdoll
        if (collision.rigidbody != null)
        {
            Vector3 forceDirection = (transform.position - collision.transform.position).normalized;
            foreach (Rigidbody rb in ragdollRigidbodies)
            {
                rb.AddForce(forceDirection * deathForce, ForceMode.Impulse);
            }
        }

        // Destroy after delay
        Destroy(gameObject, 10f);
    }

    IEnumerator IdleThenMove()
    {
        isIdling = true;
        yield return new WaitForSeconds(idleTime);
        SetNewDestination();
        isIdling = false;
    }

    void SetNewDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += homePosition;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    public void MeatExtract()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject m = Instantiate(Meat, transform.position, Quaternion.identity);
        }

        Destroy(Instantiate(BloodFX, transform.position, Quaternion.identity), 10f);
    }
}
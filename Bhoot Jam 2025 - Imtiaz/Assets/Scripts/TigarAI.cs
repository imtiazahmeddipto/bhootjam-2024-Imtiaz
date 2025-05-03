using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Rendering.Universal;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class TigerAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float patrolRadius = 15f;
    public float walkSpeed = 3f;
    public float runSpeed = 7f;
    public float idleTime = 2f;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float attackDamage = 30f;
    public float attackCooldown = 2f;

    [Header("Effects")]
    public GameObject BloodFX;
    public float deathForce = 15f;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private Vector3 homePosition;
    private bool isIdling;
    private bool isDead;
    private bool isAttacking;
    private float lastAttackTime;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private Collider mainCollider;
    public AudioSource audioSource;
    public AudioClip attackSFX, dieSFX, chaseSFX;
    public GameObject Meat;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        homePosition = transform.position;
        mainCollider = GetComponent<Collider>();

        // Ragdoll setup
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
        ToggleRagdoll(false);

        SetNewDestination();
    }

    void ToggleRagdoll(bool state)
    {
        animator.enabled = !state;
        if (mainCollider != null)
            mainCollider.enabled = !state;

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = !state;
            rb.interpolation = state ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col != mainCollider)
                col.enabled = state;
        }

        agent.enabled = !state;
    }

    void Update()
    {
        if (isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        float normalizedSpeed = agent.velocity.magnitude / runSpeed;
        animator.SetFloat("Speed", normalizedSpeed);


        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer <= attackRange)
            {
                HandleAttack();
            }
            else
            {
                ChasePlayer();
            }
        }
        else
        {
            PatrolBehavior();
        }
    }

    void PatrolBehavior()
    {
        agent.speed = walkSpeed;
        if (!isIdling && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(IdleThenMove());
        }
    }

    private bool oneTimeFlag = false;
    void ChasePlayer()
    {
        if (!oneTimeFlag)
        {
            audioSource.PlayOneShot(chaseSFX);
            oneTimeFlag = true; 
        }

        agent.speed = runSpeed;
        agent.SetDestination(player.position);
        isAttacking = false;
    }

    void HandleAttack()
{
    agent.isStopped = true;

    // Get direction without changing y-axis
    Vector3 direction = (player.position - transform.position);
    direction.y = 0f;

    if (direction != Vector3.zero)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 720f * Time.deltaTime); // 720° per second
    }

    if (Time.time > lastAttackTime + attackCooldown)
    {
        isAttacking = true;
        animator.SetTrigger("Attack");
        audioSource.PlayOneShot(attackSFX);
        DealDamage();
        lastAttackTime = Time.time;
        StartCoroutine(ResetAttack());
    }
}


    void DealDamage()
    {
        // Implement player damage logic here
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            StartCoroutine(DieAfterDelay());
        }
    }
    private IEnumerator DieAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        HorrorPlayerControllerURP playerController = player.GetComponent<HorrorPlayerControllerURP>();
        playerController.Die();
        playerController.dieReason.text = "Die Reason: Tigar Attack";
    }

    IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
        agent.isStopped = false;
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

    // Add these to your existing methods:
    void SetNewDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += homePosition;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
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
    public void MeatExtract()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject m = Instantiate(Meat, transform.position, Quaternion.identity);
        }

        Destroy(Instantiate(BloodFX, transform.position, Quaternion.identity), 10f);
    }
}
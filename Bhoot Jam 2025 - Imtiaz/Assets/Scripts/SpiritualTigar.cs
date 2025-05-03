using UnityEngine.AI;
using UnityEngine;
using System.Collections;

public class SpiritualTigar : MonoBehaviour
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


    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private Vector3 homePosition;
    private bool isIdling;
    private bool isDead;
    private bool isAttacking;
    private float lastAttackTime;
    public AudioClip spiritDieSFX;
    public GameObject HorrorFace;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        homePosition = transform.position;

        SetNewDestination();
    }


    void Update()
    {
        if (isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        float normalizedSpeed = agent.velocity.magnitude / runSpeed;
        animator.SetFloat("Speed", normalizedSpeed);

        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer <= attackRange)  // Check doAttack before attacking
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

    void ChasePlayer()
    {
        agent.speed = runSpeed;
        agent.SetDestination(player.position);
        isAttacking = false;
        HorrorFace.SetActive(true);
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
            lastAttackTime = Time.time;
            StartCoroutine(ResetAttack());
            DealDamage();
            Die();
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
    }

    IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
        agent.isStopped = false;
    }
    void Die()
    {
        if (isDead) return; // prevent double death
        isDead = true;

        GameObject audioObject = new GameObject("DeathAudioSource");
        audioObject.transform.position = transform.position;
        AudioSource deathAudioSource = audioObject.AddComponent<AudioSource>();
        deathAudioSource.spatialBlend = 1f;
        deathAudioSource.volume = .7f;
        deathAudioSource.maxDistance = 20f;
        deathAudioSource.PlayOneShot(spiritDieSFX);

        Destroy(audioObject, spiritDieSFX.length);

       
        Destroy(gameObject, .5f);
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet") && !isDead)
        {
            Die();
        }
    }
}

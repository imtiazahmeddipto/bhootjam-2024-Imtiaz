using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiritPatroll : MonoBehaviour
{
    [Header("Movement Settings")]
    public float patrolRadius = 10f;
    public float moveSpeed = 3f;
    public float idleTime = 3f;
    public float DestroyRange;
    private NavMeshAgent agent;
    private Animator animator;
    private Vector3 homePosition;
    private bool isIdling;
    private bool isDead;
    private Transform player;
    public AudioClip dieSFX;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        homePosition = transform.position;
        agent.speed = moveSpeed;

        SetNewDestination();
    }

    void Update()
    {
        if (isDead) return;

        if (Vector3.Distance(transform.position, player.position) <= DestroyRange)
        {
            Die();
            return;
        }

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
            Die();
        }
    }

    void Die()
    {
        /*GameObject audioObject = new GameObject("DeathAudioSource");
        audioObject.transform.position = transform.position;
        AudioSource deathAudioSource = audioObject.AddComponent<AudioSource>();
        deathAudioSource.spatialBlend = 1f;
        deathAudioSource.volume = 1f;
        deathAudioSource.maxDistance = 30f;
        deathAudioSource.PlayOneShot(dieSFX);
        deathAudioSource.PlayOneShot(dieSFX);*/
        Destroy(gameObject);
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
}

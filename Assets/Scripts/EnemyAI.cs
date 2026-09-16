using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;

    [Header("Detect / Attack range")]
    public float detectRange = 15f;
    public float loseRange = 20f;
    public float attackRange = 5f;
    public float breakAttackRange = 8f;

    [Header("Attack")]
    public float attackDamage = 10f;
    public float minAttackCooldown = 1f;
    public float maxAttackCooldown = 5f;

    public LayerMask hitMask;

    [Header("Debug")]
    public bool debugLog = false;

    private NavMeshAgent agent;
    private float nextAttackTime;

    enum EnemyState
    {
        Patrol,
        Chase,
        Attack
    }

    EnemyState state = EnemyState.Patrol;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            PlayerHealth ph = FindObjectOfType<PlayerHealth>();
            if (ph != null)
            {
                player = ph.transform;
            }
        }

        if (agent != null)
        {
            PickNewPatrolPoint();
        }
    }

    void Update()
    {
        if (player == null)
        {
            return;
        }
        if (agent == null)
        {
            return;
        }

        float distToPlayer = Vector3.Distance(player.position, transform.position);

        switch (state)
        {
            case EnemyState.Patrol:
                if (agent.remainingDistance <= 2f)
                {
                    PickNewPatrolPoint();
                }
                if (distToPlayer <= detectRange)
                {
                    state = EnemyState.Chase;
                }
                break;

            case EnemyState.Chase:
                agent.SetDestination(player.position);
                if (distToPlayer <= attackRange)
                {
                    agent.updateRotation = false;
                    agent.isStopped = true;
                    state = EnemyState.Attack;
                }
                else if (distToPlayer > loseRange)
                {
                    state = EnemyState.Patrol;
                    PickNewPatrolPoint();
                }
                break;

            case EnemyState.Attack:
                Vector3 lookAt = player.position;
                lookAt.y = transform.position.y;
                transform.LookAt(lookAt);

                if (distToPlayer > breakAttackRange)
                {
                    agent.isStopped = false;
                    agent.updateRotation = true;
                    state = EnemyState.Chase;
                    return;
                }

                if (Time.time >= nextAttackTime)
                {
                    TryAttack(distToPlayer);
                    nextAttackTime = Time.time + Random.Range(minAttackCooldown, maxAttackCooldown);
                }
                break;
        }
    }

    void TryAttack(float distToPlayer)
    {
        Vector3 origin = transform.position;
        Vector3 toPlayer = player.position - origin;
        Vector3 dir = toPlayer.normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, distToPlayer, hitMask))
        {
            if (debugLog)
            {
                Debug.Log("[EnemyAI] ray hit: " + hit.collider.name + " / tag=" + hit.collider.tag);
            }

            PlayerHealth ph = hit.collider.GetComponentInParent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(attackDamage);
            }
            else if (debugLog)
            {
                Debug.Log("[EnemyAI] hit but no PlayerHealth on it or parents");
            }
        }
        else if (debugLog)
        {
            Debug.Log("[EnemyAI] ray missed");
        }
    }

    void PickNewPatrolPoint()
    {
        Vector3 near = Random.insideUnitSphere * 10f;
        near.y = 0f;
        Vector3 target = transform.position + near;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 20f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}

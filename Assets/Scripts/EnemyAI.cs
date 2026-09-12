using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent agent;
    private Vector3 patrolTarget;
    public float minAttackCooldown = 1f;
    public float maxAttackCooldown = 5f;
    private float nextAttackTime;
    public LayerMask hitMask;
    enum EnemyState
    {
        Patrol,
        Chase,
        Attack
    }

    EnemyState state = EnemyState.Patrol;

    void Start()
    {
        if(player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if(p != null)
            {
                player = p.transform;
            }
        }
        agent = GetComponent<NavMeshAgent>();
        PickNewPatrolPoint();
    }

    void Update()
    {
        switch (state)
        {
            case EnemyState.Patrol:
                
                if (agent.remainingDistance <= 2f)
                {
                    PickNewPatrolPoint();
                }
                if(Vector3.Distance(player.position,transform.position) <= 15f)
                {
                    state = EnemyState.Chase;
                }
                break;

            case EnemyState.Chase:
                agent.SetDestination(player.position);
                if (Vector3.Distance(player.position,transform.position) <= 3f)
                {
                    agent.updateRotation = false;
                    agent.isStopped = true;
                    state = EnemyState.Attack;
                }
                if(Vector3.Distance(player.position,transform.position) > 20f)
                {
                    state = EnemyState.Patrol;
                    PickNewPatrolPoint();
                }
                break;

            case EnemyState.Attack:
                Vector3 ToTargetDir = player.position;
                ToTargetDir.y = transform.position.y;
                transform.LookAt(ToTargetDir);
                if (Vector3.Distance(player.position, transform.position) > 8f)
                {
                    agent.isStopped = false;
                    agent.updateRotation = true;
                    state = EnemyState.Chase;
                }
                if(Time.time >= nextAttackTime)
                {
                    Vector3 dir = player.position - transform.position;
                    float dist = dir.magnitude;
                    if(Physics.Raycast(transform.position,dir,out RaycastHit hit,dist,hitMask))
                    {
                        if(hit.collider.CompareTag("Player"))
                        {
                            player.GetComponent<PlayerHealth>().TakeDamage(10f);
                        }
                    }
                    nextAttackTime = Time.time + Random.Range(minAttackCooldown,maxAttackCooldown);
                }

                break;
        }

    }
    void PickNewPatrolPoint()
    {
        Vector3 near = Random.insideUnitSphere * 10f;
        near.y = 0;
        patrolTarget = transform.position + near;
        bool canGo = NavMesh.SamplePosition(patrolTarget,out NavMeshHit hit, 20f, NavMesh.AllAreas);
        if(canGo)
        {
            agent.SetDestination(hit.position);
        }
    }
}

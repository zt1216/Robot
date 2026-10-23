using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefabs;
    public Transform[] spawnPoints;
    public float spawnInterval = 1f;
    public float restTime = 8f;

    [Header("Spawn area")]
    public float randomRangeX = 25f;
    public float randomRangeZ = 25f;
    public float minDistanceToPlayer = 10f;
    public float checkSphereRadius = 1.5f;
    private int enemyLayerMask;
    public float maxYOffset = 3f;
    private int spawnBlockMask;

    public Transform player;

    private NavMeshPath pathBuffer;
    private int wave = 0;
    private int toSpawn = 0;
    private float spawnTimer = 0f;
    private float restTimer = 0f;
    private float stuckTimer = 0f;
    public int CurrentWave
    {
        get
        {
            return wave;
        }
    }

    void Start()
    {
        pathBuffer = new NavMeshPath();
        if (MainMenu.loadSave == true)
        {
            int saved = PlayerPrefs.GetInt("SavedWave", 0);
            if (saved > 0)
            {
                wave = saved - 1;
            }
        }
        enemyLayerMask = LayerMask.GetMask("Enemy");
        spawnBlockMask = LayerMask.GetMask("Enemy", "Default");
        if (player == null)
        {
            PlayerHealth ph = FindObjectOfType<PlayerHealth>();
            if (ph != null)
            {
                player = ph.transform;
            }
        }
        StartWave();
    }

    void Update()
    {
        if (toSpawn > 0)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer > spawnInterval)
            {
                SpawnOne();
                spawnTimer = 0f;
            }
        }
        else
        {
            if(AliveCount() == 0)
            {
                stuckTimer = 0f;
                restTimer += Time.deltaTime;
                if(restTimer >= restTime)
                {
                    restTimer = 0f;
                    StartWave();
                }
            }
            else
            {
                stuckTimer += Time.deltaTime;
                if(stuckTimer >= 180f)
                {
                    stuckTimer = 0f;
                    StartWave();
                }
            }
        }
    }

    void StartWave()
    {
        wave++;
        toSpawn = 3 + wave * 2;
        PlayerPrefs.SetInt("SavedWave", wave);
    }

    void SpawnOne()
    {
        if (enemyPrefabs == null)
        {
            Debug.LogError("[EnemySpawner] enemyPrefabs is null");
            toSpawn = 0;
            return;
        }

        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPos = new Vector3(
                transform.position.x + Random.Range(-randomRangeX, randomRangeX),
                transform.position.y + 2f,
                transform.position.z + Random.Range(-randomRangeZ, randomRangeZ));

            NavMeshHit hit;
            if (!NavMesh.SamplePosition(randomPos, out hit, 15f, NavMesh.AllAreas))
            {
                continue;
            }
            if(Mathf.Abs(hit.position.y - transform.position.y) > maxYOffset)
            {
                continue;
            }
            if (player != null && Vector3.Distance(hit.position, player.position) < minDistanceToPlayer)
            {
                continue;
            }
            Vector3 checkCenter = hit.position + Vector3.up * 2.3f;
            if (Physics.CheckSphere(checkCenter, checkSphereRadius, spawnBlockMask))
            {
                continue;
            }
            if(Physics.Raycast(hit.position + Vector3.up * 0.2f,Vector3.up,12f,spawnBlockMask))
            {
                continue;
            }
            if(!NavMesh.CalculatePath(hit.position,player.position,NavMesh.AllAreas,pathBuffer) || pathBuffer.status != NavMeshPathStatus.PathComplete)
            {
                continue;
            }
            SpawnEnemyAt(hit.position);
            toSpawn--;
            return;
        }

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform p = spawnPoints[Random.Range(0, spawnPoints.Length)];
            SpawnEnemyAt(p.position);
            toSpawn--;
            return;
        }

        Vector3 fallback = transform.position;
        NavMeshHit fb;
        if(NavMesh.SamplePosition(fallback,out fb,10f,NavMesh.AllAreas))
        {
            fallback = fb.position;
        }
        SpawnEnemyAt(fallback);
        toSpawn--;
    }
    void SpawnEnemyAt(Vector3 pos)
    {
        GameObject enemy = Instantiate(enemyPrefabs, pos, Quaternion.identity);

        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.player = player;
        }
    }

    int AliveCount()
    {
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }
}

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
    public float checkSphereRadius = 0.6f;

    public Transform player;

    private int wave = 0;
    private int toSpawn = 0;
    private float spawnTimer = 0f;
    private float restTimer = 0f;

    public int CurrentWave
    {
        get
        {
            return wave;
        }
    }

    void Start()
    {
        if (MainMenu.loadSave == true)
        {
            int saved = PlayerPrefs.GetInt("SavedWave", 0);
            if (saved > 0)
            {
                wave = saved - 1;
            }
        }

        if (player == null)
        {
            PlayerHealth ph = FindObjectOfType<PlayerHealth>();
            if (ph != null)
            {
                player = ph.transform;
            }
        }
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
        else if (AliveCount() == 0)
        {
            restTimer += Time.deltaTime;
            if (restTimer >= restTime)
            {
                restTimer = 0f;
                StartWave();
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
            return;
        }

        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-randomRangeX, randomRangeX),
                1f,
                Random.Range(-randomRangeZ, randomRangeZ));

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, 2f, NavMesh.AllAreas))
            {
                if (player != null && Vector3.Distance(hit.position, player.position) < minDistanceToPlayer)
                {
                    continue;
                }
                if (Physics.CheckSphere(hit.position, checkSphereRadius))
                {
                    continue;
                }

                SpawnEnemyAt(hit.position);
                toSpawn--;
                return;
            }
        }

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform p = spawnPoints[Random.Range(0, spawnPoints.Length)];
            SpawnEnemyAt(p.position);
            toSpawn--;
        }
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

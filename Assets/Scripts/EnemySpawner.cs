using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefabs;
    public Transform[] spawnPoints;
    public float spawnInterval = 1f;
    public float restTime = 8f;
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
        if(MainMenu.loadSave ==true)
        {
            int saved = PlayerPrefs.GetInt("SavedWave", 0);
            if(saved > 0)
            {
                wave = saved - 1;
            }
        }
    }
    void Update()
    {
        if (toSpawn > 0)
        {
            spawnTimer += Time.deltaTime;
            if(spawnTimer > spawnInterval)
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
        Transform p = spawnPoints[Random.Range(0,spawnPoints.Length)];
        Instantiate(enemyPrefabs, p.position, Quaternion.identity);
        toSpawn--;
    }
    int AliveCount()
    {
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }
}

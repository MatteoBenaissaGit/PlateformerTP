using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoGeneration : MonoBehaviour
{
    [SerializeField] private List<GameObject> _spawnPoints = null;

    private int _nextSpawnPointIndex;
    public int SpawnPointsCount => _spawnPoints.Count;
    [SerializeField] private GameObject PrefabAmmo;
    [SerializeField] private GameObject SpawnPointAmmo;
    [SerializeField] private int choosedSpawnPoint;

    [Header("Alarm")]
    [SerializeField] private float ammoSpawnAlarm = 0f;
    [SerializeField] private float ammoSpawnCooldown = 5f;

    [Header("Security")]
    [SerializeField] private bool ammoCanSpawn = true;
    private void Start()
    {
        SetupAlarm();
    }

    private void SetupAlarm()
    {
        ammoSpawnAlarm = ammoSpawnCooldown;
    }

    private void Update()
    {
        EnemySpawning();
    }

    private void EnemySpawning()
    {
        //alarm countdown
        ammoSpawnAlarm -= Time.deltaTime;

        if(ammoSpawnAlarm <= 0)
        {
            GetRandomSpawnPoint();
            Instantiate(PrefabAmmo, SpawnPointAmmo.transform.position, Quaternion.identity);
            SetupAlarm();
        }
    }
    private void GetRandomSpawnPoint()
    {
        choosedSpawnPoint = Random.Range(0, _spawnPoints.Count);
        SpawnPointAmmo = _spawnPoints[choosedSpawnPoint];
    }
}

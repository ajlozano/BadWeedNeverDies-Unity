using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagement : MonoBehaviour
{

    public GameObject enemy;
    public GameObject particle;

    private GameObject _player;

    private float _timeToSpawnElapsed = 0;
    private float _timeToNextSpawn = 0;
    private float _maxTimeToSpawn = 5;
    private bool _isSpawning = true;
    private Vector3 _spawnPos;
    private int _maxEnemiesInScene = 4;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("SpawnParticle", 2f);
        _player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isSpawning)
        {
            Invoke("SpawnParticle", _timeToNextSpawn);
            _isSpawning = true;
        }
    }

    void SpawnParticle()
    {
        GameObject[] enemiesSpawned = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemiesSpawned.Length < _maxEnemiesInScene)
        {
            if (particle != null)
            {
                float xPos = Random.Range(-15f, 15f);
                float yPos = Random.Range(-7f, 7f);
                _spawnPos = new Vector3(xPos, yPos, 0);
                Vector3 relativePos = _player.transform.position - _spawnPos;

                if (relativePos.magnitude > 5f)
                {
                    GameObject p = Instantiate(particle, _spawnPos, Quaternion.identity);
                    p.transform.Rotate(new Vector3(225, 0, 0));

                    Invoke("SpawnEnemy", 0.3f);
                }
                else
                {
                    _isSpawning = false;
                }
            }
        }
        else
        {
            _isSpawning = false;
        }
    }
    void SpawnEnemy()
    {
        if (enemy != null)
        {
            Instantiate(enemy, _spawnPos, Quaternion.identity);

            _timeToNextSpawn = Random.Range(1f, _maxTimeToSpawn);
            _isSpawning = false;
        }
    }
}

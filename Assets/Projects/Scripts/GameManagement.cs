using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagement : MonoBehaviour
{
    // Edit in inspector
    [Header("Game Properties")]
    public int _maxEnemiesInScene = 4;
    public float _maxTimeToSpawn = 5;
    [Header("Objects")]
    public GameObject enemy;
    public GameObject particle;
    private GameObject _player;

    // Delta time
    private float _timeToNextSpawn = 0;

    private bool _isSpawning = true;
    private Vector3 _spawnPos;
    

    private void Awake()
    {
        _player = GameObject.Find("Player");
    }

    // Start is called before the first frame update
    void Start()
    {
        Invoke("SpawnParticle", 2f);
    }

    private void FixedUpdate()
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

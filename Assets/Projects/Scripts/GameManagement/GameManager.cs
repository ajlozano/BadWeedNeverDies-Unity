using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    #region Public Properties
    [Header("Game Properties")]
    public int maxEnemiesInScene = 3;
    public float minTimeToEnemySpawn = 1f;
    public float maxTimeToEnemySpawn = 4f;
    public float minTimeToLightningSpawn = 3f;
    public float maxTimeToLightningSpawn = 10f;
    public float StartTimeToLightningWave = 10f;
    public float StartTimeToEnemyWave2 = 20f;
    public float StartTimeToenemyWave3 = 60f;

    [Header("Objects")]
    public GameObject enemy1;
    public GameObject enemy2;
    public GameObject enemy3;
    public GameObject particle;
    public GameObject lightning;
    #endregion

    #region Private Properties
    private GameObject _player;
    private List<GameObject> _enemyList;
    private Canvas _canvas;

    //Audio clips
    [SerializeField] private AudioClip _pauseClip;
    [SerializeField] private AudioClip _unPauseClip;
    [SerializeField] private AudioClip _victoryClip;

    // Delta time
    private const float GAME_TIMEOUT = 600f; // 10 minutes;

    private float _timeToNextEnemySpawn = 0f;
    private float _timeToNextLightningSpawn = 0f;
    private float _remainingTime = 0f;
    // Game
    private bool _isSpawningEnemy = true;
    private bool _isSpawningLightning = false;
    private Vector3 _spawnPos;
    private float _fadeTime = 0.5f;
    private bool _isGameOver = false;
    private int _enemyTypeList;
    #endregion

    #region Main Methods
    private void Awake()
    {
        _player = GameObject.Find("Player");
        _canvas = GameObject.FindObjectOfType<Canvas>();
        _enemyList = new List<GameObject>();
        _enemyList.Add(enemy1);
        _enemyList.Add(enemy2);
        _enemyList.Add(enemy3);

        _enemyTypeList = 1;
    }
    void Start()
    {
        Invoke("FadeStart", _fadeTime);
        if (_canvas != null)
        {
            _canvas.transform.Find("PanelToFade").gameObject.SetActive(true);
            _canvas.transform.Find("PanelToFade").GetComponent<CanvasGroup>().DOFade(0, _fadeTime);
        }
    }
    private void Update()
    {
        _remainingTime += Time.deltaTime;
    }
    private void FixedUpdate()
    {
        if (!_isSpawningEnemy)
        {
            Invoke("SpawnParticle", _timeToNextEnemySpawn);
            _isSpawningEnemy = true;
        }
        
        if (_remainingTime > GAME_TIMEOUT && !_isGameOver)
        {
            _isGameOver = true;
            ExitToMainMenu();
        }
        else {
            if (_remainingTime > StartTimeToLightningWave)
            {
                if (!_isSpawningLightning)
                {
                    Invoke("SpawnLightning", _timeToNextLightningSpawn);
                    _isSpawningLightning = true;
                }
            }
            if (_remainingTime > StartTimeToEnemyWave2)
            {
                maxEnemiesInScene = 4;
                _enemyTypeList = _enemyList.Count - 1;
            }
            if (_remainingTime > StartTimeToenemyWave3)
            {
                maxEnemiesInScene = 8;
                _enemyTypeList = _enemyList.Count;
            }
        }


    }
    #endregion
    #region Private Methods
    private void FadeStart()
    {
        Invoke("SpawnParticle", 2f);
    }
    private void SpawnParticle()
    {
        GameObject[] enemiesSpawned = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemiesSpawned.Length < maxEnemiesInScene)
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
                    _isSpawningEnemy = false;
                }
            }
        }
        else
        {
            _isSpawningEnemy = false;
        }
    }
    private void SpawnEnemy()
    {
        GameObject enemy = _enemyList[Random.Range(0, _enemyTypeList)];
        if (enemy != null)
        {
            Instantiate(enemy, _spawnPos, Quaternion.identity);

            _timeToNextEnemySpawn = Random.Range(minTimeToEnemySpawn, maxTimeToEnemySpawn);
            _isSpawningEnemy = false;
        }
    }

    private void SpawnLightning()
    {
        float xPos = Random.Range(-15f, 15f);
        float yPos = Random.Range(-7f, 7f);
        Vector3 _lightningSpawnPos = new Vector3(xPos, yPos, 0);

        if (lightning != null)
        {
            Instantiate(lightning, _lightningSpawnPos, Quaternion.identity);

            _timeToNextLightningSpawn = Random.Range(minTimeToLightningSpawn, maxTimeToLightningSpawn);
            _isSpawningLightning = false;
        }
    }
    private void FadeEnd()
    {
        SceneManager.LoadScene("MainMenu");
    }

    #endregion
    #region Public Methods
    public void PauseGame(bool pause)
    {
        if (pause)
        {
            _canvas.transform.Find("PauseScreen").gameObject.SetActive(true);
            _player.GetComponent<PlayerBehavior>().OnDisable();
            AudioManager.instance.ExecuteSound(_pauseClip);
            Time.timeScale = 0;
            //GameObject.FindObjectOfType<EventSystem>().sendNavigationEvents = true;
        }
        else
        {
            GameObject.FindObjectOfType<EventSystem>().sendNavigationEvents = false;
            Time.timeScale = 1;
            _canvas.transform.Find("PauseScreen").gameObject.SetActive(false);
            _player.GetComponent<PlayerBehavior>().OnEnable();
            AudioManager.instance.ExecuteSound(_unPauseClip);
        }
    }
    public void ExitToMainMenu()
    {
        Invoke("FadeEnd", _fadeTime);
        if (_canvas != null)
        {
            if (_isGameOver)
            {
                SceneDataTransferManager.levelCompleteText = "Level Complete!";
                AudioManager.instance.ExecuteSound(_pauseClip);
            }
            else
            {
                AudioManager.instance.ExecuteSound(_unPauseClip);
            }
            _canvas.transform.Find("PanelToFade").GetComponent<CanvasGroup>().DOFade(1, _fadeTime);
        }
    }
    public void ExitFromPause()
    {
        Time.timeScale = 1;
        FadeEnd();
    }
    #endregion










}

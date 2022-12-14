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
    public int _maxEnemiesInScene = 8;
    public float _minTimeToEnemySpawn = 1f;
    public float _maxTimeToEnemySpawn = 4f;
    public float _minTimeToLightningSpawn = 3f;
    public float _maxTimeToLightningSpawn = 10f;
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
    private const float TIME_TO_LIGHTNING = 10f; // 10 seconds;
    private float _timeToNextEnemySpawn = 0f;
    private float _timeToNextLightningSpawn = 0f;
    private float _remainingTime = 0f;
    // Game
    private bool _isSpawningEnemy = true;
    private bool _isSpawningLightning = false;
    private Vector3 _spawnPos;
    private float _fadeTime = 0.5f;
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
        if (_remainingTime > GAME_TIMEOUT)
        {
            AudioManager.instance.ExecuteSound(_victoryClip);
            ExitToMainMenu();
        }
    }

    private void FixedUpdate()
    {
        if (!_isSpawningEnemy)
        {
            Invoke("SpawnParticle", _timeToNextEnemySpawn);
            _isSpawningEnemy = true;
        }

        if (_remainingTime > TIME_TO_LIGHTNING) {
            print("entered1");
            if (!_isSpawningLightning)
            {
                print("entered2");

                Invoke("SpawnLightning", _timeToNextLightningSpawn);
                _isSpawningLightning = true;
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
        GameObject enemy = _enemyList[Random.Range(0, _enemyList.Count)];
        if (enemy != null)
        {
            Instantiate(enemy, _spawnPos, Quaternion.identity);

            _timeToNextEnemySpawn = Random.Range(_minTimeToEnemySpawn, _maxTimeToEnemySpawn);
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

            _timeToNextLightningSpawn = Random.Range(_minTimeToLightningSpawn, _maxTimeToLightningSpawn);
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
            AudioManager.instance.ExecuteSound(_unPauseClip);
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

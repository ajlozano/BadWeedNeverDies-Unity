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
    public int _maxEnemiesInScene = 4;
    public float _maxTimeToSpawn = 5;
    [Header("Objects")]
    public GameObject enemy1;
    public GameObject enemy2;
    public GameObject enemy3;
    public GameObject particle;
    #endregion

    #region Private Properties
    private GameObject _player;
    private List<GameObject> _enemyList;
    private Canvas _canvas;

    //Audio clips
    [SerializeField] private AudioClip _pauseClip;
    [SerializeField] private AudioClip _unPauseClip;

    // Delta time
    private float _timeToNextSpawn = 0;
    private bool _isSpawning = true;
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

    private void FixedUpdate()
    {
        if (!_isSpawning)
        {
            Invoke("SpawnParticle", _timeToNextSpawn);
            _isSpawning = true;
        }
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
                    _isSpawning = false;
                }
            }
        }
        else
        {
            _isSpawning = false;
        }
    }
    private void SpawnEnemy()
    {
        GameObject enemy = _enemyList[Random.Range(0, _enemyList.Count)];
        if (enemy != null)
        {
            Instantiate(enemy, _spawnPos, Quaternion.identity);

            _timeToNextSpawn = Random.Range(1f, _maxTimeToSpawn);
            _isSpawning = false;
        }
    }
    private void FadeEnd()
    {
        SceneManager.LoadScene("MainMenu");
    }

    #endregion








}

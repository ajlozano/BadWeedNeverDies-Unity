using System;
using UnityEngine;
public class EnemyBehavior : MonoBehaviour
{

    #region Public Properties
    // Edit in inspector
    [Header("Enemy properties")]
    public EnemyData enemyData;
    // Audio Clips
    [Header("Enemy Audio Clips")]
    public AudioClip spawn;
    public AudioClip throwItself;
    public AudioClip teleport;
    public AudioClip deathExplosion;
    public AudioClip hit;

    #endregion

    #region Private Properties
    // Enemy
    private bool _isDying;
    private bool _isTransformed = false;
    private bool _isGrabbed = false;
    private bool _isThrowing = false;
    private bool _oneTime = false;

    // Delta time
    //public float _destroyTimeElapsed = 0;
    private float _maxDestroyTime = 1f;
    private float _timeToHitElapsed = 0;
    private float _maxTimeToHit = 1f;

    // Objects
    private GameObject _player;
    private GameObject _body;
    private GameObject _grabAimParticle;
    private HealthManager _healthManager;
    private AnimationManager _animManager;
    //public Animator _anim;
    #endregion

    #region Main Methods
    private void Awake()
    {
        _player = GameObject.Find("Player");
        _body = this.transform.Find("Body").gameObject;
        _grabAimParticle = this.transform.Find("PurpleGrab").gameObject;
        _grabAimParticle.SetActive(false);
        _healthManager = this.GetComponent<HealthManager>();
        _animManager = this.GetComponent<AnimationManager>();
        AudioManager.instance.ExecuteSound(spawn);

    }

    private void Start()
    {
        TimerManager.active.AddTimer(enemyData.maxTimeToTransform, () =>
        {
            if (!_isDying)
            {
                TransformMode();
            }
        }, false);

    }

    private void Update()
    {
        if (_isDying)
        {
            if (!_oneTime)
            {
                DeathCycle();
                _oneTime = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if ((!_isGrabbed) && (!_isThrowing))
        {
            if ((_player != null) && (!_isDying))
            {
                MoveEnemy();
            }
        }
        if (_isThrowing)
        {
            GetComponent<Rigidbody2D>().velocity = this.transform.right * enemyData.maxSpeedThrowing;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject target = collision.gameObject;

        if (_isThrowing)
        {
            if (target.tag != "Player" && target.tag != "Altar")
            {
                transform.rotation = Quaternion.identity;
                _isGrabbed = false;
                _isThrowing = false;
                _grabAimParticle.SetActive(false);

            }
            if (target.tag == "Altar")
            {
                Teleport();
                Destroy(this.gameObject);
            }
        }
        else if (!_isThrowing && target.tag == "Player" && !_isDying)
        {
            target.GetComponent<PlayerController>().SetDamage(enemyData.damageSet);
            _timeToHitElapsed = 0f;
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!_isThrowing && collision.transform.tag == "Player" && !_isDying)
        {
            _timeToHitElapsed += Time.deltaTime;
            if (_timeToHitElapsed >= _maxTimeToHit)
            {
                collision.transform.GetComponent<PlayerController>().SetDamage(enemyData.damageSet);
                _timeToHitElapsed = 0f;
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!_isThrowing && collision.transform.tag == "Player")
            _timeToHitElapsed = 0f;
    }
    #endregion

    #region Public Methods
    public bool DisableHabilities(Transform t, GameObject obj)
    {
        if (!_isTransformed)
            return false;

        _isGrabbed = true;
        GetComponent<Rigidbody2D>().isKinematic = true;
        GetComponent<Rigidbody2D>().simulated = false;
        transform.position = obj.transform.position;
        transform.rotation = obj.transform.rotation;
        transform.SetParent(obj.transform);

        _grabAimParticle.SetActive(true);

        return true;
    }
    public void EnableHabilities()
    {
        AudioManager.instance.ExecuteSound(throwItself);

        GetComponent<Rigidbody2D>().isKinematic = false;
        GetComponent<Rigidbody2D>().simulated = true;
        transform.SetParent(null);
        _isGrabbed = false;
        _isThrowing = true;
        TimerManager.active.AddTimer(enemyData.maxTimeThrowing, () =>
        {
            _isThrowing = false;
        }, false);
    }
    public void SetDamage()
    {
        if (!_isTransformed)
        {
            AudioManager.instance.ExecuteSound(hit);
            if (_animManager != null)
                _animManager.SetAnimationId("isHit", true, 0.1f);
            _isDying = _healthManager.SetDamage(enemyData.damageGet);
        }
    }
    #endregion

    #region Private Methods
    private void TransformMode()
    {
        _isTransformed = true;
        GameObject particle = Instantiate(enemyData.transformParticle, transform.position, transform.rotation);
        particle.transform.SetParent(this.transform);
        particle.transform.localScale = Vector3.one / 1.5f;
        particle.transform.localPosition = new Vector3(0, 0.5f, 0);
        _body.GetComponent<SpriteRenderer>().color = Color.red;
    }

    private void DeathCycle()
    {

        _grabAimParticle.SetActive(false);

        GetComponent<Rigidbody2D>().isKinematic = true;
        transform.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);

        if (_animManager != null)
            _animManager.SetAnimationId("isDying", _isDying);

        TimerManager.active.AddTimer(_maxDestroyTime, () =>
        {
            if (enemyData.deathParticle != null)
            {
                AudioManager.instance.ExecuteSound(deathExplosion);

                GameObject particle = Instantiate(enemyData.deathParticle, transform.position, transform.rotation);
                Destroy(particle, 1f);
            }
            Destroy(this.gameObject);
        }, false);
    }

    private void Teleport()
    {
        if (enemyData.teleportParticle != null)
        {
            AudioManager.instance.ExecuteSound(teleport);

            GameObject particle = Instantiate(enemyData.teleportParticle, transform.position, transform.rotation);
            Destroy(particle, 1f);
        }
        if (!_isDying)
            Destroy(this.gameObject);
    }
    private void MoveEnemy()
    {
        Vector2 relativePos = _player.transform.position - transform.position;

        transform.GetComponent<Rigidbody2D>().velocity = relativePos.normalized * enemyData.maxSpeed;

        if (relativePos.normalized.x < 0f)
        {
            _body.transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else
        {
            _body.transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }

    #endregion
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyBehavior : MonoBehaviour
{
    // edit in inspector
    [Header("Enemy properties")]
    public EnemyData enemyData;
    // Enemy
    private bool _isDying;
    [SerializeField]
    private bool _isTransformed = false;
    private bool _isGrabbed = false;
    private bool _isThrowing = false;
    [SerializeField]
    private float _health;
    //Time
    public float _destroyTimeElapsed = 0;
    public float _maxDestroyTime = 1f;
    private float _throwingTimeElapsed = 0;
    [SerializeField]
    private float _timeToTransformElapsed = 0;
    // objects
    private GameObject _player;
    private GameObject _body;
    private Animator _anim;
    private GameObject _destroyParticle;

    private void Awake()
    {
        _player = GameObject.Find("Player");
        _body = this.transform.Find("Body").gameObject;
        _anim = _body.GetComponent<Animator>();
        _health = enemyData.maxHealth;
    }
    // Start is called before the first frame update
    void Start()
    {
        enemyData.deathParticle.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isTransformed)
        {
            _timeToTransformElapsed += Time.deltaTime;
            if (_timeToTransformElapsed >= enemyData.maxTimeToTransform)
            {
                _isTransformed = true;
            }
        }
        if ((!_isGrabbed) && (!_isThrowing))
        {
            if ((_player != null) && (!_isDying))
            {
                MoveEnemy();
            }
            if ((_isDying) && (!_isTransformed))
            {
                DeathCycle();
            }
        }
        if (_isThrowing)
        {
            _throwingTimeElapsed += Time.deltaTime;
            ThrowItself();
        }

    }

    private void DeathCycle()
    {
        GetComponent<Rigidbody2D>().isKinematic = true;
        transform.GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);

        _destroyTimeElapsed += Time.deltaTime;
        if (_destroyTimeElapsed >= _maxDestroyTime)
        {
            _destroyTimeElapsed = 0;
            if (enemyData.deathParticle != null)
            {
                GameObject particle = Instantiate(enemyData.deathParticle, transform.position, transform.rotation);
                Destroy(particle, 1f);
            }
            Destroy(this.gameObject);
        }
        else
        {
            SetAnimationId("isDying", _isDying);
        }
    }

    private void SetAnimationId(String id, bool result)
    {
        if (_anim != null)
        {
            _anim.SetBool(id, result);
        }
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
    private void ThrowItself()
    {
        if (_throwingTimeElapsed >= enemyData.maxTimeThrowing)
        {
            _isThrowing = false;
            _throwingTimeElapsed = 0;
        }
        else
        {
            //transform.Translate(Vector2.right * enemyData.maxSpeedThrowing * Time.deltaTime);
            GetComponent<Rigidbody2D>().velocity = this.transform.right * enemyData.maxSpeedThrowing;
        }
    }
    public bool DisableHabilities(Transform t, GameObject obj)
    {
        if (_isTransformed)
            return false;

        _isGrabbed = true;
        GetComponent<Rigidbody2D>().isKinematic = true;
        GetComponent<Rigidbody2D>().simulated = false;
        transform.position = obj.transform.position;
        transform.rotation = obj.transform.rotation;
        transform.SetParent(obj.transform);

        return true;
    }
    public void EnableHabilities()
    {
        GetComponent<Rigidbody2D>().isKinematic = false;
        GetComponent<Rigidbody2D>().simulated = true;
        transform.SetParent(null);
        _isGrabbed = false;
        _isThrowing = true;
    }
    public void SetDamage(float damage)
    {
        _health -= damage;
        SetAnimationId("isHit", true);

        if (_health <= 0.0f)
        {
            _health = 0.0f;
            _isDying = true;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject target = collision.gameObject;

        if (_isThrowing && target.tag != "Player")
        {
            transform.rotation = Quaternion.identity;
            _isGrabbed = false;
            _isThrowing = false;
            _throwingTimeElapsed = 0;
        }
        else if (!_isThrowing && target.tag == "Player")
        {
            target.GetComponent<PlayerController>().SetDamage(enemyData.damage);
        }
    }
}

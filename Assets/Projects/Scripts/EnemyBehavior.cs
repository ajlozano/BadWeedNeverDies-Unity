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
    [HideInInspector]
    public bool _isDying;
    [HideInInspector]
    public bool _isTransformed = false;
    private bool _isGrabbed = false;
    private bool _isThrowing = false;
    private float _throwingTimeElapsed = 0;

    // objects
    private GameObject _player;
    private GameObject _body;
    private Animator _anim;

    private void Awake()
    {
        _player = GameObject.Find("Player");
        _body = transform.Find("Body").gameObject;
        _anim = this.GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        if (!_isGrabbed)
        {
            if (!_isThrowing)
            {
                if (_player != null)
                {
                    MoveEnemy();
                }
                if (!_isTransformed)
                {
                    if (_isDying)
                    {
                        // Add dying process?
                    }
                }
            }
        }
        if (_isThrowing)
        {
            _throwingTimeElapsed += Time.deltaTime;
            ThrowItself();
        }
    }
    private void AnimationManagement()
    {
        if (_anim != null)
        {
            _anim.SetBool("isDying", _isDying);
        }
    }
    private void MoveEnemy()
    {
        Vector2 relativePos = _player.transform.position - transform.position;

        if (!_isGrabbed)
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
    public void DisableHabilities(Transform t, GameObject obj)
    {
        _isGrabbed = true;
        GetComponent<Rigidbody2D>().isKinematic = true;
        GetComponent<Rigidbody2D>().simulated = false;
        transform.position = obj.transform.position;
        transform.rotation = obj.transform.rotation;
        transform.SetParent(obj.transform);
    }
    public void EnableHabilities()
    {
        GetComponent<Rigidbody2D>().isKinematic = false;
        GetComponent<Rigidbody2D>().simulated = true;
        transform.SetParent(null);
        _isGrabbed = false;
        _isThrowing = true;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((_isThrowing) && (collision.transform.tag != "Player"))
        {
            transform.rotation = Quaternion.identity;
            _isGrabbed = false;
            _isThrowing = false;
            _throwingTimeElapsed = 0;
        }
    }
}

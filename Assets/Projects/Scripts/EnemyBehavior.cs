using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyBehavior : MonoBehaviour
{
    // edit in inspector
    [Header("Enemy properties")]
    public float maxSpeed;
    public float maxRotationSlerp;
    public float maxTimeThrowing;
    public float maxSpeedThrowing;

    // Enemy
    private bool _isGrabbed = false;
    private bool _isThrowing = false;
    private float _throwingTimeElapsed = 0;

    private bool _isTransformed
    {
        get { return _isTransformed; }
    }
    // objects
    private GameObject _player;
    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.Find("Player");
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
                    FollowPlayer();
                }
            }
            else
            {
                _throwingTimeElapsed += Time.deltaTime;
                ThrowItself();
            }
        }
    }
    private void FollowPlayer()
    {
        Vector2 relativePos = _player.transform.position - transform.position;
        Quaternion current = transform.localRotation;
        Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(relativePos.y, relativePos.x) * Mathf.Rad2Deg);
        transform.localRotation = Quaternion.Slerp(current, rotation, Time.deltaTime * maxRotationSlerp);
        transform.Translate(Vector2.right * maxSpeed * Time.deltaTime);
    }
    private void ThrowItself()
    {
        if (_throwingTimeElapsed >= maxTimeThrowing)
        {
            _isThrowing = false;
            _throwingTimeElapsed = 0;
        }
        else
        {
            transform.Translate(Vector2.right * maxSpeedThrowing * Time.deltaTime);
        }
    }
    public void DisableHabilities(Transform t, GameObject obj)
    {
        _isGrabbed = true;
        GetComponent<Rigidbody2D>().isKinematic = true;
        GetComponent<Rigidbody2D>().simulated = false;
        transform.position = obj.transform.position;
        transform.rotation = obj.transform.rotation;
        transform.SetParent(t);
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
        if (_isThrowing)
        {
            _isThrowing = false;
            _throwingTimeElapsed = 0;
        }
    }
}

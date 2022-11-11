using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    // edit in inspector
    [Header("Enemy properties")]
    public float maxSpeed;
    public float maxRotationSlerp;

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
        if (_player != null)
        {
            FollowPlayer();
        }


    }
    private void FollowPlayer()
    {
        Vector2 relativePos = _player.transform.position - transform.position;
        Quaternion current = transform.localRotation;
        Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(relativePos.y, relativePos.x) * Mathf.Rad2Deg);
        transform.localRotation = Quaternion.Slerp(current, rotation, Time.deltaTime * maxRotationSlerp);

        //_speed = Mathf.Lerp()
        transform.Translate(Vector2.right * maxSpeed * Time.deltaTime);
    }

}

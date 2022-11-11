using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    [Header("Effect objects to inlcude")]
    public GameObject impactEffect;
    public GameObject targetExplosionEffect;
    [SerializeField]
    private float _bulletSpeed = 10;
    private GameObject player;

    // process to destroy
    //private bool _isHit = false;
    //private float _destroyTimeElapsed = 0.0f;
    //private float _timeToDestroy = 1.0f;
    private GameObject _impactObjectSpawned;
    private GameObject _explosionObjectSpawned;


    // Start is called before the first frame update
    void Start()
    {
        PlayerController playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        _bulletSpeed = playerController.GetBulletSpeed();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.right * _bulletSpeed * Time.deltaTime);

        // time to destroy objects after impact
        //if (_isHit)
        //{
        //    _destroyTimeElapsed += Time.deltaTime;
        //    if (_destroyTimeElapsed > _timeToDestroy)
        //        DestroyObjects();
        //}
    }

    private void DestroyObjects()
    {
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if((collision.gameObject.tag == "Enemy") || (collision.gameObject.tag == "Scenario"))
        {
            _impactObjectSpawned = Instantiate(impactEffect, transform.position, transform.rotation);
            Destroy(_impactObjectSpawned, 5.0f);
            if (collision.gameObject.tag == "Enemy")
            {
                _explosionObjectSpawned = Instantiate(targetExplosionEffect, collision.transform.position, collision.transform.rotation);
                _explosionObjectSpawned.transform.localScale = collision.transform.localScale;
                Destroy(_explosionObjectSpawned, 5.0f);

                collision.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                Destroy(collision.gameObject);
            }

            Destroy(this.gameObject);
        }
    }
}

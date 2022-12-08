using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    [Header("Effect objects to inlcude")]
    public GameObject impactEffect;
    public GameObject targetExplosionEffect;
    private float _bulletSpeed = 10;
    private GameObject _impactObjectSpawned;

    void Start()
    {
        PlayerController playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        _bulletSpeed = playerController.GetBulletSpeed();
    }

    private void FixedUpdate() => transform.Translate(Vector3.right * _bulletSpeed * Time.deltaTime);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.gameObject.tag == "Enemy") || (collision.gameObject.tag == "Wall"))
        {
            _impactObjectSpawned = Instantiate(impactEffect, transform.position, transform.rotation);
            Destroy(_impactObjectSpawned, 1.0f);
            if (collision.gameObject.tag == "Enemy")
            {
                collision.gameObject.GetComponent<EnemyBehavior>().SetDamage();
            }

            Destroy(this.gameObject);
        }
    }
}

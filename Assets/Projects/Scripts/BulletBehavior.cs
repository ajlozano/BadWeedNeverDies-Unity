using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    #region Public Properties
    [Header("Effect objects to inlcude")]
    public GameObject impactEffect;
    public GameObject targetExplosionEffect;
    #endregion

    #region Private Properties
    private float _bulletSpeed = 10;
    private GameObject _impactObjectSpawned;
    #endregion

    #region Main Methods
    void Start()
    {
        PlayerBehavior playerController = GameObject.Find("Player").GetComponent<PlayerBehavior>();
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
    #endregion
}

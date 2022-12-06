using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    [Header("Effect objects to inlcude")]
    public GameObject impactEffect;
    public GameObject targetExplosionEffect;

    [Header("Bullet properties")]
    public float damage;

    [SerializeField]
    private float _bulletSpeed = 10;

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
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.gameObject.tag == "Enemy") || (collision.gameObject.tag == "Wall"))
        {
            _impactObjectSpawned = Instantiate(impactEffect, transform.position, transform.rotation);
            Destroy(_impactObjectSpawned, 1.0f);
            if (collision.gameObject.tag == "Enemy")
            {
                collision.gameObject.GetComponent<EnemyBehavior>().SetDamage(damage);
            }

            Destroy(this.gameObject);
        }
    }
}

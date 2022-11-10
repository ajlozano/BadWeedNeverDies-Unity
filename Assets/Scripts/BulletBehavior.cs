using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    public GameObject player;
    [SerializeField]
    private float _bulletSpeed = 10;

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if((collision.gameObject.tag == "Enemy") || (collision.gameObject.tag == "Scenario"))
        {
            Destroy(this.gameObject);
        }
    }
}

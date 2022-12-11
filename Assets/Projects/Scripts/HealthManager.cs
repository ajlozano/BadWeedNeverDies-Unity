using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    private const float MAX_HEALTH = 100f;
    public Image healthBar;
    private float _health;
    public float Health { get => _health; set => _health = value; }

    private void Awake() => _health = MAX_HEALTH;

    void Update()
    {
        if (healthBar != null)
            healthBar.fillAmount = _health / MAX_HEALTH ;
    }

    public bool SetDamage(float damage)
    {
        _health -= damage;
        if (_health < 0)
            return true;

        return false;
    }
}

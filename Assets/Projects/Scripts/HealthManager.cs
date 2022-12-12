using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    #region Public Properties
    public Image healthBar;
    public float Health { get => _health; set => _health = value; }
    #endregion

    #region Private Properties
    private const float MAX_HEALTH = 100f;
    private float _health;
    #endregion

    #region Main Methods
    private void Awake() => _health = MAX_HEALTH;
    void Update()
    {
        if (healthBar != null)
            healthBar.fillAmount = _health / MAX_HEALTH;
    }
    #endregion

    #region Public Methods
    public bool SetDamage(float damage)
    {
        _health -= damage;
        if (_health <= 0)
            return true;

        return false;
    }
    #endregion

    #region Private Methods
    #endregion



}

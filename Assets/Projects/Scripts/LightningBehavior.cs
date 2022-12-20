using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningBehavior : MonoBehaviour
{
    #region Private Properties
    private GameObject _crosshairImpact ,_lightning, _rayImpact, _electricity;
    private int _blinkingTimes = 0;
    private const int MAX_BLINKING = 10;
    [SerializeField] private AudioClip _thunderClip;
    #endregion

    #region Main Methods
    private void Awake()
    {
        _crosshairImpact = this.transform.Find("CrosshairImpact").gameObject;
        _lightning = this.transform.Find("Lightning").gameObject;
        _rayImpact = this.transform.Find("RayImpact").gameObject;
        _electricity = this.transform.Find("Electricity").gameObject;

        _lightning.GetComponent<LineRenderer>().sortingLayerName = "Particles";
        _lightning.GetComponent<LineRenderer>().sortingOrder = 5;

        _lightning.SetActive(false);
        _electricity.SetActive(false);
        _rayImpact.SetActive(false);
    }

    private void Start()
    {
        TimerManager.active.AddTimer(0.2f, () => {
            BlinkCrosshair(); 
            }, false);
    }
    #endregion
    #region Public Methods
    private void BlinkCrosshair()
    {
        _blinkingTimes++;
        if (_blinkingTimes < MAX_BLINKING)
        {
            _crosshairImpact.SetActive(!_crosshairImpact.activeInHierarchy);
            TimerManager.active.AddTimer(0.2f, () => {
                BlinkCrosshair();
            }, false);
        }
        else
        {
            _blinkingTimes = 0;
            _crosshairImpact.SetActive(false);
            Impact();
        }
    }
    #endregion
    #region Private Methods
    private void Impact()
    {
        AudioManager.instance.ExecuteSound(_thunderClip);
        _crosshairImpact.SetActive(false);
        _lightning.SetActive(true);
        _rayImpact.SetActive(true);
        _electricity.SetActive(true);
        this.GetComponent<BoxCollider2D>().enabled = true;

        TimerManager.active.AddTimer(1f, () =>
        {
            Destroy(this.gameObject);
        });
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            print("ray impact!");
            collision.gameObject.GetComponent<PlayerBehavior>().SetDamage(100f);
        }
    }
    #endregion
}

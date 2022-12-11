using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Enemy Data")]
public class EnemyData : ScriptableObject
{
    #region Public Properties
    public float maxSpeed;
    public float maxRotationSlerp;
    public float maxTimeThrowing;
    public float maxSpeedThrowing;
    public float maxTimeToTransform;
    public float damageSet;
    public float damageGet;
    public GameObject deathParticle;
    public GameObject teleportParticle;
    public GameObject spawnParticle;
    public GameObject transformParticle;
    #endregion

    #region Private Properties
    #endregion

    #region Main Methods
    #endregion

    #region Public Methods
    #endregion

    #region Private Methods
    #endregion

}

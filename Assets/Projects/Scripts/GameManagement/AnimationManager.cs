using System;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    #region Public Properties
    #endregion

    #region Private Properties
    private Animator _anim;
    private String _currentId;
    #endregion

    #region Main Methods
    private void Awake()
    {
        _anim = this.transform.Find("Body").GetComponent<Animator>();
    }
    #endregion

    #region Public Methods
    public void SetAnimationId(String id, bool result)
    {
        _anim.SetBool(id, result);
    }

    public void SetAnimationId(String id, bool result, float t)
    {
        _currentId = id;
        _anim.SetBool(_currentId, result);
        Invoke("ClearId", t);
    }
    public bool GetAnimationId(String id)
    {
        return _anim.GetBool(id);
    }

    private void ClearId()
    {
        _anim.SetBool(_currentId, false);
    }
    #endregion

    #region Private Methods
    #endregion

}

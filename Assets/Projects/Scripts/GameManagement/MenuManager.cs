using DG.Tweening;
using Microsoft.Unity.VisualStudio.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UIElements.Image;

public class MenuManager : MonoBehaviour
{
    #region Private Properties
    private Canvas _canvas;
    private float _fadeTime = 0.5f;
    #endregion
    #region Main Methods
    private void Awake()
    {
        _canvas = GameObject.FindObjectOfType<Canvas>();
    }
    #endregion
    #region Private Methods
    private void StartGame()
    {
        SceneManager.LoadScene("Level1");
    }
    #endregion
    #region Public Methods
    public void Fade()
    {
        if (_canvas != null)
        {
            _canvas.transform.Find("PanelToFade").gameObject.SetActive(true);
            _canvas.transform.Find("PanelToFade").GetComponent<CanvasGroup>().DOFade(1, _fadeTime);
        }

        Invoke("StartGame", _fadeTime);
    }
    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }
    #endregion

}

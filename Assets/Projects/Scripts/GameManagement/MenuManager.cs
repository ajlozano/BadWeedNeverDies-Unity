using DG.Tweening;
using Microsoft.Unity.VisualStudio.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UIElements.Image;

public class MenuManager : MonoBehaviour
{
    #region Private Properties
    private Canvas _canvas;
    private float _fadeTime = 0.5f;
    [SerializeField] private AudioClip _clickMenu;
    private MenuInputActions _menuControls;
    private InputAction _cancel;
    #endregion
    #region Public Properties
    public GameObject eventSystem;
    #endregion

    #region Main Methods
    private void Awake()
    {
        _canvas = GameObject.FindObjectOfType<Canvas>();
        _menuControls = new MenuInputActions();
    }
    private void OnEnable()
    {
        _cancel = _menuControls.UI.Cancel;
        _cancel.Enable();
        _cancel.performed += Cancel;
    }
    private void OnDisable()
    {
        _cancel.Disable();
    }
    private void Cancel(InputAction.CallbackContext obj)
    {
        //print("Cancel pressed");
        if (_canvas.transform.Find("TutorialPopup").gameObject.activeSelf)
            _canvas.transform.Find("TutorialPopup").gameObject.SetActive(false);
        eventSystem.SetActive(true);
    }
    private void Start()
    {

        _canvas.transform.Find("ScoreTime").
            GetComponent<TextMeshProUGUI>().text = SceneDataTransferManager.score;
        _canvas.transform.Find("LevelCompleteText").
            GetComponent<TextMeshProUGUI>().text = SceneDataTransferManager.levelCompleteText;
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

        AudioManager.instance.ExecuteSound(_clickMenu);

        Invoke("StartGame", _fadeTime);
    }
    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }
    public void Tutorial()
    {
        _canvas.transform.Find("TutorialPopup").gameObject.SetActive(true);
        eventSystem.SetActive(false);
    }
    #endregion

}

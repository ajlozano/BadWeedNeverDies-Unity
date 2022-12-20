using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    #region Private Properties
    private double runningTime = 0;
    private TimeSpan timeSpan;
    private Transform timeTextCanvas;
    #endregion
    #region Main Methods
    void Start()
    {
        timeTextCanvas = transform.Find("RunningTime");
    }

    private void Update()
    {
        if (timeTextCanvas != null)
        {
            runningTime += Time.deltaTime;
            timeSpan = TimeSpan.FromSeconds(runningTime);
            string timeText = string.Format("{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            timeTextCanvas.gameObject.GetComponent<TextMeshProUGUI>().text = timeText;
            SceneDataTransferManager.score = timeText;
        }
    }
    #endregion
}

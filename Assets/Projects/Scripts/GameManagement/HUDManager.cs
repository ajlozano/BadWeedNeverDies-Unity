using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    private double runningTime = 0;
    private TimeSpan timeSpan;
    private Transform timeTextCanvas;
    void Start()
    {
        //Time.timeScale = 0;
        timeTextCanvas = transform.Find("RunningTime");
        print(timeTextCanvas.transform.position);
    }

    private void Update()
    {
        if (timeTextCanvas != null)
        {
            runningTime += Time.deltaTime;
            timeSpan = TimeSpan.FromSeconds(runningTime);
            string timeText = string.Format("{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            timeTextCanvas.gameObject.GetComponent<TextMeshProUGUI>().text = timeText;
        }
    }
}

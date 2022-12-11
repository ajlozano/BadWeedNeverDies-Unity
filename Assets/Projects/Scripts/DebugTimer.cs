using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTimer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TimerManager.active.AddTimer(2f, () =>
        {
            print("Timer 1: " + Time.time);
        }, true);

        TimerManager.active.AddTimer(5f, () =>
        {
            print("Timer 2: " + Time.time);
        }, false);

        TimerManager.active.AddTimer(7f, () =>
        {
            print("Timer 3: " + Time.time);
        }, true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

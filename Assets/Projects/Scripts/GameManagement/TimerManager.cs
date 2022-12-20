using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    private static TimerManager _mactive;
    public static TimerManager active
    {
        get
        {
            if (_mactive == null)
            {
                _mactive = new GameObject("TimerManager").AddComponent<TimerManager>();
                //DontDestroyOnLoad(_mactive.gameObject);
            }
            return _mactive;
        }
    }

    private List<Timer> _timers = new List<Timer>();


    private void Update()
    {
        for (int i = 0; i < _timers.Count; i++)
        {
            if (_timers[i].executionTime < Time.time)
            {
                _timers[i].action?.Invoke();
                if (!_timers[i].looping)
                    _timers.RemoveAt(i);
            }
        }
    }
    public void AddTimer(float _delay, System.Action _action, bool _looping = false)
    {
        _timers.Add(new Timer(_delay, _action, _looping));
    }
}

public class Timer
{
    public float executionTime;
    public System.Action action;
    public bool looping;
    private float mdelay;

    public Timer(float _delay, System.Action _action, bool _looping = false)
    {
        executionTime = Time.time + _delay;
        action = _action;
        looping = _looping;
        mdelay = _delay;

        if (_looping)
            action += () =>
            {
                executionTime = Time.time + mdelay;
            };
    }
}
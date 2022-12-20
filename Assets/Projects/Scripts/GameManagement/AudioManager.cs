using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _mInstance;
    public static AudioManager instance;
    //{
    //    get
    //    {
    //        if (_mInstance == null)
    //        {
    //            _mInstance = new GameObject("AudioManager").AddComponent<AudioManager>();
    //            DontDestroyOnLoad(_mInstance.gameObject);
    //        }
    //        return _mInstance;
    //    }
    //}

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ExecuteSound(AudioClip sound)
    {
        audioSource.PlayOneShot(sound);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : SceneSingleton<AudioManager>
{
    [System.Serializable]   
    public class Audio
    {
        public string name;
        public AudioClip clip;
    }

    public AudioSource Source;
    public Audio[] Audios;

    public void PlayOneShot(string _name)
    {
        foreach (var audio in Audios)
        {
            if(audio.name == _name)
            {
                Source.PlayOneShot(audio.clip);
                return;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMusic : SceneSingleton<MainMusic>
{
    public AudioSource Source;

    public AudioClip Menu;
    public AudioClip Game;

    public void StartPlay()
    {
        Source.Stop();
        Source.clip = Game;
        Source.Play();
    }
}

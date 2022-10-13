using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMusic : SceneSingleton<MainMusic>
{
    public AudioSource Source;



    public void StartPlay()
    {
        Source.Play();
    }
}

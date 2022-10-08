using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject Root;

    private void Start()
    {
        PhotonService.OnRunnerStart += () => { Root.SetActive(false); };
        PhotonService.OnRunnerStartFailed += () => { Root.SetActive(true); };
    }

    public void SetTeam(string _team)
    {
        if(_team == "isp")
        {
            PlayerData.Instance.Team = Team.ISP;
        }
        else
        {
            PlayerData.Instance.Team = Team.Hacker;
        }
    }
}

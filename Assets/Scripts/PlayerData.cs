using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : Singleton<PlayerData>
{
    public Team Team { get; set; }
    public string Nickname { get; set; } = "MyNickname";

    public void SetNickname()
    {
        Nickname = "Player";
    }

    public void SetTeam(Team team)
    {
        Team = team;
    }
}

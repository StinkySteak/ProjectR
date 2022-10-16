using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : Singleton<PlayerData>
{
    public Team Team { get; set; }
    public string Nickname { get; set; } = "MyNickname";

    public void SetNickname(string name)
    {
        Nickname = name;
    }

    public void SetTeam(Team team)
    {
        Team = team;
    }
}

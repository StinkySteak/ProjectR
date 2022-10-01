using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : Singleton<PlayerData>
{
    public Team Team;
    public string Nickname;

    public void SetNickname()
    {
        Nickname = "Player";
    }

    public void SetTeam(Team team)
    {
        Team = team;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SessionListing : MonoBehaviour
{
    public TMP_Text Name;
    public TMP_Text Region;
    public TMP_Text Players;

    public Button Button;

    public void Set(string _name, string _region, int _player, UnityAction _action)
    {
        Name.text = _name;
        Region.text = _region;
        Players.text = $"{_player} / {6}"; // 6 is max players
        Button.onClick.AddListener(_action);
    }
}

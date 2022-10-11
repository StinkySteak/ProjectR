using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject Root;

    [Header("Session Creation")]
    public GameObject CreationPanel;
    public TMP_InputField NameField;
    public TMP_Dropdown RegionDropdown;

    [Header("SessionListing")]
    public GameObject SessionPanel;

    public Transform SessionListingPanel;
    public SessionListing SessionListingPrefab;

    readonly List<SessionListing> SpawnedListing = new();

    private void Start()
    {
        PhotonService.OnRunnerStart += () => { Root.SetActive(false); };
        PhotonService.OnRunnerStartFailed += () => { Root.SetActive(true); };
        PhotonService.OnSessionListUpdated += UpdateSessionListing;

        List<TMP_Dropdown.OptionData> options = new()
        {
            new TMP_Dropdown.OptionData("asia"),
            new TMP_Dropdown.OptionData("jp"),
            new TMP_Dropdown.OptionData("us"),
            new TMP_Dropdown.OptionData("eu")
        };

        RegionDropdown.ClearOptions();
        RegionDropdown.AddOptions(options);
    }

    string GetRegionName(int _value)
    {
        switch (_value)
        {
            case 0: return "asia";
            case 1: return "jp";
            case 2: return "us";
            case 3: return "eu";
            default:
                break;
        }

        return string.Empty;
    }

    public void SetTeam(string _team)
    {
        if (_team == "isp")
        {
            PlayerData.Instance.Team = Team.ISP;
        }
        else
        {
            PlayerData.Instance.Team = Team.Hacker;
        }
    }

    void UpdateSessionListing(List<SessionInfo> sessions)
    {
        SessionPanel.SetActive(true);

        foreach (var listing in SpawnedListing)
        {
            Destroy(listing.gameObject);
        }

        SpawnedListing.Clear();

        foreach (var s in sessions)
        {
            var obj = Instantiate(SessionListingPrefab, SessionListingPanel);

            var listing = obj.GetComponent<SessionListing>();

            listing.Set(s.Name, s.Properties["region"], s.PlayerCount, () => PhotonService.Instance.StartSimulation(s.Name, GameMode.Client));
        }
    }
    public void HostGame()
    {
        if (NameField.text.Length <= 0)
            return;

        PhotonService.Instance.StartSimulation(NameField.text, GameMode.Host,GetRegionName(RegionDropdown.value));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.KCC;

[OrderAfter(typeof(Player))]
public class PlayerSetup : NetworkBehaviour
{
   
    public static PlayerSetup LocalPlayer { get; set; }

    public GameObject[] RemoteVisual;
    public GameObject[] LocalVisual;

    public MeshRenderer Renderer;

    public Material Material_Ally;
    public Material Material_Enemy;

    Player PlayerData = null;

    //CACHE COMPONENT DOWN BELOW

    public PlayerHealth PlayerHealth { get; set; }
    public PlayerController PlayerController { get; set; }
    public PlayerWeaponManager WeaponManager { get; set; }

    public KCC KCC => PlayerController.KCC;
    public Collider MainCollider => PlayerController.KCC.Collider;

    public override void Spawned()
    {
        PlayerHealth = GetComponent<PlayerHealth>();
        PlayerController = GetComponent<PlayerController>();
        WeaponManager = GetComponent<PlayerWeaponManager>();

        if (Object.HasInputAuthority)
            LocalPlayer = this;

        PlayerData = PlayerManager.Instance.GetPlayer(Object.InputAuthority);

        PlayerData.OnPlayerInitialized += OnPlayerInitilized;

        if (Object.HasStateAuthority)
            PlayerData.State = PlayerState.Spawned;

        OnPlayerInitilized();
    }
    void OnPlayerInitilized()
    {
        if (!PlayerData.IsInitialized)
            return;

        if (Player.LocalPlayer == null)
            return;

        var material = PlayerData.Team == Player.LocalPlayer.Team ? Material_Ally : Material_Enemy;

        Renderer.material = material;

        var hasInputAuth = Object.HasInputAuthority;

        foreach (var visual in RemoteVisual)
            visual.SetActive(!hasInputAuth);

        foreach (var visual in LocalVisual)
            visual.SetActive(hasInputAuth);

        ManageColliders();
    }
    /// <summary>
    /// used for players to go pass through barriers
    /// </summary>
    public void ManageColliders()
    {
        foreach (var adv in LevelManager.Instance.Advancements)
        {
            var col = adv.GetMyTeamAdvance(PlayerData.Team).SpawnBarrier.Collider;
            KCC.SetIgnoreCollider(col, true);
            print($"Disabling collider : {col}");
        }

        MainCollider.tag = "Player";

        print("Colliders Managed");
    }
}

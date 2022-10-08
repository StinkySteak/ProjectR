using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.KCC;
using static UnityEngine.EventSystems.PointerEventData;
using Fusion.Sockets;
using System;

public class PlayerInputHandler : NetworkBehaviour, IBeforeUpdate, IBeforeTick, INetworkRunnerCallbacks
{
    [Networked] public NetworkButtons ButtonsPrevious { get; set; }

    public KCC KCC;

    [Networked]
    public NetworkBool InputBlocked { get; set; }

    public PlayerInput CachedInput;
    public PlayerInput RenderInput;
    public PlayerInput FixedInput;

    public PlayerInput BaseFixedInput;
    public PlayerInput BaseRenderInput;

    private bool ResetCachedInput;

    private Vector2 CachedMoveDirection;
    private float CachedMoveDirectionSize;

    [Networked]
    private PlayerInput LastKnownInput { get; set; }

    public bool IsSet(ActionButton button)
    {
        return Runner.Stage != default ? FixedInput.Buttons.IsSet(button) : RenderInput.Buttons.IsSet(button);
    }

    /// <summary>
    /// Check if the button was pressed in current input.
    /// In FUN this method compares current fixed input agains previous fixed input.
    /// In Render this method compares current render input against previous render input OR current fixed input (first Render call after FUN).
    /// </summary>
    public bool WasPressed(ActionButton button)
    {
        return Runner.Stage != default ? FixedInput.Buttons.WasPressed(BaseFixedInput.Buttons, button) : RenderInput.Buttons.WasPressed(BaseRenderInput.Buttons, button);
    }

    /// <summary>
    /// Check if the button was released in current input.
    /// In FUN this method compares current fixed input agains previous fixed input.
    /// In Render this method compares current render input against previous render input OR current fixed input (first Render call after FUN).
    /// </summary>
    public bool WasReleased(ActionButton button)
    {
        return Runner.Stage != default ? FixedInput.Buttons.WasReleased(BaseFixedInput.Buttons, button) : RenderInput.Buttons.WasReleased(BaseRenderInput.Buttons, button);
    }

    public NetworkButtons GetPressedButtons()
    {
        return Runner.Stage != default ? FixedInput.Buttons.GetPressed(BaseFixedInput.Buttons) : RenderInput.Buttons.GetPressed(BaseRenderInput.Buttons);
    }

    public NetworkButtons GetReleasedButtons()
    {
        return Runner.Stage != default ? FixedInput.Buttons.GetReleased(BaseFixedInput.Buttons) : RenderInput.Buttons.GetReleased(BaseRenderInput.Buttons);
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Runner.AddCallbacks(this);
            Runner.ProvideInput = true;
        }

        BaseRenderInput = default;
        BaseFixedInput = default;

        LastKnownInput = default;
        FixedInput = default;
        RenderInput = default;
        CachedInput = default;
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        Runner.RemoveCallbacks(this);
    }
    // 1. Collect input from devices
    void IBeforeUpdate.BeforeUpdate()
    {
        if (Object == null || HasInputAuthority == false)
            return;

        // Store last render input as a base to current render input.
        BaseRenderInput = RenderInput;

        // Reset input for current frame to default
        RenderInput = default;

        // Cached input was polled and explicit reset requested
        if (ResetCachedInput == true)
        {
            ResetCachedInput = false;

            CachedMoveDirection = default;
            CachedMoveDirectionSize = default;
            CachedInput = default;
        }

        ProcessStandaloneInput();
    }
    private void ProcessStandaloneInput()
    {
        Vector2 moveDirection = Vector2.zero;
        Vector2 lookRotationDelta = new(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"));

        if (Input.GetKey(KeyCode.W) == true) { moveDirection += Vector2.up; }
        if (Input.GetKey(KeyCode.S) == true) { moveDirection += Vector2.down; }
        if (Input.GetKey(KeyCode.A) == true) { moveDirection += Vector2.left; }
        if (Input.GetKey(KeyCode.D) == true) { moveDirection += Vector2.right; }

        if (moveDirection.IsZero() == false)
        {
            moveDirection.Normalize();
        }

        // Process input for render, represents current device state.

        RenderInput.MoveDirection = moveDirection * Time.deltaTime;
        RenderInput.LookRotationDelta = lookRotationDelta;

        RenderInput.Buttons.Set(ActionButton.Jump, Input.GetKey(KeyCode.Space));

        RenderInput.Buttons.Set(ActionButton.Fire, Input.GetMouseButton(0));
        RenderInput.Buttons.Set(ActionButton.Fire_2, Input.GetMouseButton(1));

        RenderInput.Buttons.Set(ActionButton.Reload, Input.GetKey(KeyCode.R));

        RenderInput.Buttons.Set(ActionButton.weaponSwitch1, Input.GetKey(KeyCode.Alpha1));
        RenderInput.Buttons.Set(ActionButton.weaponSwitch2, Input.GetKey(KeyCode.Alpha2));

        // Process cached input for next OnInput() call, represents accumulated inputs for all render frames since last fixed update.

        float deltaTime = Time.deltaTime;

        // Move direction accumulation is a special case. Let's say simulation runs 30Hz (33.333ms delta time) and render runs 300Hz (3.333ms delta time).
        // If the player hits W key in last frame before fixed update, the KCC will move in render update by (velocity * 0.003333f).
        // Treating this input the same way for next fixed update results in KCC moving by (velocity * 0.03333f) - 10x more.
        // Following accumulation proportionally scales move direction so it reflects frames in which input was active.
        // This way the next fixed update will correspond more accurately to what happened in render frames.

        CachedMoveDirection += moveDirection * deltaTime;
        CachedMoveDirectionSize += deltaTime;

        CachedInput.MoveDirection = CachedMoveDirection / CachedMoveDirectionSize;
        CachedInput.LookRotationDelta += RenderInput.LookRotationDelta;
        CachedInput.Buttons = new NetworkButtons(CachedInput.Buttons.Bits | RenderInput.Buttons.Bits);
    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (InputBlocked)
            return;

        PlayerInput gameplayInput = CachedInput;

        ResetCachedInput = true;

        CachedInput.LookRotationDelta = default;

        RenderInput.LookRotationDelta = default;

        input.Set(gameplayInput);
    }
    void IBeforeTick.BeforeTick()
    {
        if (InputBlocked == true)
        {
            FixedInput = default;
            BaseFixedInput = default;
            LastKnownInput = default;
            return;
        }

        // Store last known fixed input. This will be compared against new fixed input
        BaseFixedInput = LastKnownInput;

        // Set correct fixed input (in case of resimulation, there will be value from the future)
        FixedInput = LastKnownInput;

        if (GetInput<PlayerInput>(out var input) == true)
        {
            FixedInput = input;

            // Update last known input. Will be used next tick as base and fallback
            LastKnownInput = input;
        }
        else
        {
            // In case we do not get input, clear look rotation delta so player will not rotate but repeat other actions
            FixedInput.LookRotationDelta = default;
        }

        // The current fixed input will be used as a base to first Render after FUN
        BaseRenderInput = FixedInput;
    }

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {

    }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {

    }

    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {

    }

    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
    {

    }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner)
    {

    }

    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }

    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }

    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {

    }

    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
    {

    }

    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
    {

    }
}

public struct PlayerInput : INetworkInput
{
    public Vector2 LookRotationDelta;
    public Vector2 MoveDirection;

    public NetworkButtons Buttons;
}
public enum ActionButton
{
    Forward = 0,
    Backward = 1,
    Left = 2,
    Right = 3,

    Jump = 4,

    Fire = 5,
    Fire_2 = 6,
    Reload = 7,

    weaponSwitch1 = 8,
    weaponSwitch2 = 9,


}
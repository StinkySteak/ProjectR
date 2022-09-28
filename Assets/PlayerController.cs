using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.KCC;
using Projectiles;

[OrderAfter(typeof(BeforeHitboxManagerUpdater))]
[OrderBefore(typeof(KCC), typeof(HitboxManager), typeof(AfterHitboxManagerUpdater))]
[RequireComponent(typeof(KCC), typeof(BeforeHitboxManagerUpdater), typeof(AfterHitboxManagerUpdater))]
public class PlayerController : Agent
{
    public PlayerInputHandler Input;

    public float MaxCameraAngle = 80;
    public Vector3 JumpImpulse = new(0, 6, 0);

    [SerializeField]
    private Transform _cameraPivot;
    [SerializeField]
    private Transform _cameraHandle;
    [SerializeField]
    private GameObject _visual;

    protected override void ProcessEarlyFixedInput()
    {
    //    if (Owner == null || Health.IsAlive == false)
    //        return;

        var input = Input.FixedInput;

        // Here we process input and set properties related to movement / look.
        // For following lines, we should use Input.FixedInput only. This property holds input for fixed updates.

        // Clamp input look rotation delta
        Vector2 lookRotation = KCC.FixedData.GetLookRotation(true, true);
        Vector2 lookRotationDelta = KCCUtility.GetClampedLookRotationDelta(lookRotation, input.LookRotationDelta, -MaxCameraAngle, MaxCameraAngle);

        // Apply clamped look rotation delta
        KCC.AddLookRotation(lookRotationDelta);

        // Calculate input direction based on recently updated look rotation (the change propagates internally also to KCCData.TransformRotation)
        Vector3 inputDirection = KCC.FixedData.TransformRotation * new Vector3(input.MoveDirection.x, 0.0f, input.MoveDirection.y);
        KCC.SetInputDirection(inputDirection);

        if (Input.WasPressed(ActionButton.Jump) == true)
        {
            // By default the character jumps forward in facing direction
            Quaternion jumpRotation = KCC.FixedData.TransformRotation;

            if (inputDirection.IsAlmostZero() == false)
            {
                // If we are moving, jump in that direction instead
                jumpRotation = Quaternion.LookRotation(inputDirection);
            }

            // Applying jump impulse
            KCC.Jump(jumpRotation * JumpImpulse);
        }

        // Another movement related actions here (crouch, ...)
    }
    protected override void OnFixedUpdate()
    {
        // Regular fixed update for Agent class.
        // Executed after all agent KCC updates and before HitboxManager.

        // Setting camera pivot rotation
        Vector2 pitchRotation = KCC.FixedData.GetLookRotation(true, false);
        _cameraPivot.localRotation = Quaternion.Euler(pitchRotation);
    }

    protected override void ProcessLateFixedInput()
    {
        // Executed after HitboxManager. Process other non-movement actions like shooting.

        //if (Owner != null)
        //{
        //    Weapons.ProcessInput(Owner.Input);
        //}
    }

    protected override void OnLateFixedUpdate()
    {
      //  Weapons.OnLateFixedUpdate();
    }

    protected override void ProcessRenderInput()
    {
      //  if (Owner == null || Health.IsAlive == false)
      //      return;

        // Here we process input and set properties related to movement / look.
        // For following lines, we should use Input.RenderInput and Input.CachedInput only. These properties hold input for render updates.
        // Input.RenderInput holds input for current render frame.
        // Input.CachedInput holds combined input for all render frames from last fixed update. This property will be used to set input for next fixed update (polled by Fusion).

        var input = Input;

        // Get look rotation from last fixed update (not last render!)
        Vector2 lookRotation = KCC.FixedData.GetLookRotation(true, true);

        // For correct look rotation, we have to apply deltas from all render frames since last fixed update => stored in Input.CachedInput
        Vector2 lookRotationDelta = KCCUtility.GetClampedLookRotationDelta(lookRotation, input.CachedInput.LookRotationDelta, -MaxCameraAngle, MaxCameraAngle);

        KCC.SetLookRotation(lookRotation + lookRotationDelta);

        Vector3 inputDirection = default;

        // MoveDirection values from previous render frames are already consumed and applied by KCC, so we use Input.RenderInput (non-accumulated input for this frame)
        Vector3 moveDirection = input.RenderInput.MoveDirection.X0Y();
        if (moveDirection.IsZero() == false)
        {
            inputDirection = KCC.RenderData.TransformRotation * moveDirection;
        }

        KCC.SetInputDirection(inputDirection);

        // Jump is extrapolated for render as well.
        if (Input.WasPressed(ActionButton.Jump) == true)
        {
            // By default the character jumps forward in facing direction
            Quaternion jumpRotation = KCC.RenderData.TransformRotation;

            if (inputDirection.IsZero() == false)
            {
                // If we are moving, jump in that direction instead
                jumpRotation = Quaternion.LookRotation(inputDirection);
            }

            KCC.Jump(jumpRotation * JumpImpulse);
        }

        // At his point, KCC haven't been updated yet (except look rotation, which propagates immediately) so camera have to be synced later.
        // Because this is advanced implementation, base class triggers manual KCC update immediately after this method.
        // This allows us to synchronize camera in OnEarlyRender(). To keep consistency with fixed update, camera related properties are updated in regular render update - OnRender().
    }

    // 5.
    protected override void OnLateRender()
    {
        // Setting base camera transform based on handle

        // For render we care only about input authority.
        // This can be extended to state authority if needed (inner code won't be executed on host for other agents, having camera pivots to be set only from fixed update, causing jitter if spectating that player)
        if (HasInputAuthority == true)
        {
            Vector2 pitchRotation = KCC.RenderData.GetLookRotation(true, false);
            _cameraPivot.localRotation = Quaternion.Euler(pitchRotation);

            var cameraTransform = PlayerCamera.LocalPlayer.transform;

            cameraTransform.position = _cameraHandle.position;
            cameraTransform.rotation = _cameraHandle.rotation;
        }

       // Weapons.OnRender();
    }
}

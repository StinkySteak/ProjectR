using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Player1 : NetworkBehaviour
{
    private NetworkCharacterControllerPrototype _cc;
    [SerializeField] private Ball _prefabBall;
    //Spawning in diffrent directions; stores the last move direction, as well as the forward direction of the ball 
    private Vector3 forward;

    [SerializeField] private PhysxBall _prefabPhysxBall;

    [Networked] private TickTimer delay { get; set; }

    [Networked(OnChanged = nameof(OnBallSpawned))]
    public NetworkBool spawned { get; set; }

    private Material _material;
    Material material
    {
        get
        {
            if (_material == null)
                _material = GetComponentInChildren<MeshRenderer>().material;
            return _material;
        }
    }

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterControllerPrototype>();
    }

    public static void OnBallSpawned(Changed<Player1> changed)
    {
        changed.Behaviour.material.color = Color.white;
    }

    public override void Render()
    {
        material.color = Color.Lerp(material.color, Color.blue, Time.deltaTime);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            _cc.Move(5 * data.direction * Runner.DeltaTime);

            if (data.direction.sqrMagnitude > 0)
                forward = data.direction;

            //Limits Spawn Frequencey 
            if (delay.ExpiredOrNotRunning(Runner))
            {
                if ((data.buttons & NetworkInputData.MOUSEBUTTON1) != 0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabBall,
                    transform.position + forward, Quaternion.LookRotation(forward),
                    Object.InputAuthority, (runner, o) =>
                    {
                // Initialize the Ball before synchronizing it
                o.GetComponent<Ball>().Init();
                    });
                    spawned = !spawned;
                }
                else if((data.buttons & NetworkInputData.MOUSEBUTTON2) !=  0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabPhysxBall,
                        transform.position + forward, Quaternion.LookRotation(forward),
                        Object.InputAuthority, (runner, o) =>
                        {
                            o.GetComponent<PhysxBall>().Init(forward * 10);
                        });
                    spawned = !spawned;
                }
            }
        }
    }
}

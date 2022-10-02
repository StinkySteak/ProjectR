using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Ball : NetworkBehaviour
{

    [Networked] private TickTimer life { get; set; }

    public void Init()
    {
        life = TickTimer.CreateFromSeconds(Runner, 5.0f);
    }

    public override void FixedUpdateNetwork()
    {
        //Desapwns the ball after the life timer has expired
        if (life.Expired(Runner)) { Runner.Despawn(Object); }
        else { transform.position += 5 * transform.forward * Runner.DeltaTime; }
    }
}

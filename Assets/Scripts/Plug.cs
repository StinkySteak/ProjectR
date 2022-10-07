using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Plug : NetworkBehaviour
{
    [Range(1, 10)] [SerializeField] float speed = 4f;

    private bool ServicerOn;
    private bool HackerOn;

    private int HackerCount;
    private int ServicerCount;

    public PathingConfig pathConfig;
    private List<Transform> waypoints;
    private int waypointCount;

    private bool canMove;
    private bool whenToGo;





    private void Awake()
    {
        
    }

    public override void FixedUpdateNetwork()
    {
       
    }

    private void OnTriggerStay(Collider other)
    {
        
    }

    private void OnTriggerExit(Collider other)
    {
        
    }

    private void MoveForward()
    {

    }

    private void MoveBackward()
    {

    }

    IEnumerator WaitOnPoint()
    {
        yield return new WaitForSeconds(.1f);
    }


}

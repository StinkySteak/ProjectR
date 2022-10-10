using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "PathConfigScriptableObjects")]
public class PathingConfig : ScriptableObject 
{

    [SerializeField] private Transform pathParent;
    public float totalDistance;

    public List<Transform> GetPathPoints()
    {
        List<Transform> pathPoints = new List<Transform>();
        foreach(Transform path in pathParent)
        {
            pathPoints.Add(path);
        }

        return pathPoints;
    }

}

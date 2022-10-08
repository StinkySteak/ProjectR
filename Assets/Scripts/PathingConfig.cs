using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Path Data", menuName = "PathConfigScriptableObjects")]
public class PathingConfig : ScriptableObject 
{

    [SerializeField] private Transform pathParent;

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

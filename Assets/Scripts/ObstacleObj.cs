using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleObj : MonoBehaviour, IWorldObject
{

    [SerializeField] protected float obstacleValue;

    public float GetVisionValue()
    {
        return obstacleValue;
    }
}

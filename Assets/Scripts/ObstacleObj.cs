using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleObj : MonoBehaviour, IWorldObject
{

    private float obstacleValue = 3f;


    public float GetVisionValue()
    {
        return obstacleValue;
    }
}

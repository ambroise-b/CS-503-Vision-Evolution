using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleObj : MonoBehaviour, IWorldObject
{
    
    public enum ObsType
    {
        NONE,
        WALL,
        BLOCK
    }
    

    [SerializeField] protected float obstacleValue;
    [SerializeField] protected ObsType obsType;
    
    private Vector3 obstacleValueRGB_Wall = new Vector3(98f/255f, 98f/255f, 98f/255f);
    private Vector3 obstacleValueRGB_Block = new Vector3(75f/255f, 214f/255f, 255f/255f);
    
    
    public float GetVisionValue()
    {
        return obstacleValue;
    }

    public Vector3 GetVisionValueRGB()
    {
        if (obsType == ObsType.WALL)
        {
            return obstacleValueRGB_Wall;
        } else if (obsType == ObsType.BLOCK)
        {
            return obstacleValueRGB_Block;
        }
        
        Debug.LogWarning("Some obstacles does not have a proper obsType set");
        
        return Vector3.zero;
        
    }


}

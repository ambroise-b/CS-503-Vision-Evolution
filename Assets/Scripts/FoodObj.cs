using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodObj : MonoBehaviour, IWorldObject
{
    private float foodValue = 2f;
    [SerializeField] protected float energy = 4f;
        

    public float GetVisionValue()
    {
        return foodValue;
    }

    public float GetEnergy()
    {
        return energy;
    }

    public void EatFood()
    {
        Destroy(gameObject);
    }
}

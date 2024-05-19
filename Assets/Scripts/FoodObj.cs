using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodObj : MonoBehaviour, IWorldObject
{
    private float foodValue = 2f;
    [SerializeField] protected float energy = 40f;

    private Vector3 foodValueRGB = new Vector3(255f / 255f, 127f / 255f, 28f / 255f);

    public float GetVisionValue()
    {
        return foodValue;
    }

    public Vector3 GetVisionValueRGB()
    {
        return foodValueRGB;
    }

    public float GetEnergy()
    {
        return energy;
    }

    public void EatFood()
    {
        //Destroy(gameObject);
        gameObject.SetActive(false);
    }
}

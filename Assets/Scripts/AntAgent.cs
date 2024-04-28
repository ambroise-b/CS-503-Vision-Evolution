using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.UI;

public class AntAgent : MonoBehaviour, IWorldObject
{
    // Start is called before the first frame update

    [SerializeField] protected float forwardSpeed;
    [SerializeField] protected float turnSpeed;

    [SerializeField] protected Transform sensorTr;


    [SerializeField] protected float fov;
    [SerializeField] protected int nbRays;
    [SerializeField] protected float visionLength;

    [SerializeField] protected float beginEnergy;
    [SerializeField] private TextMeshProUGUI txt;
    [SerializeField] private GameObject canva;
    
    private float noHitValue = 0f;
    private float antValue = 1f;

    private BoxCollider bc;
    private float energy;

    private bool DEBUG = true;


    private void Start()
    {
        bc = GetComponent<BoxCollider>();
        energy = beginEnergy;

        if (DEBUG)
        {
            canva.SetActive(true);
            txt.text = (int) energy + ".";
        }
        else
        {
            canva.SetActive(false);
        }

        
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            Forward();
        } else if (Input.GetKeyDown(KeyCode.A))
        {
            TurnLeft();
        } else if (Input.GetKeyDown(KeyCode.D))
        {
            TurnRight();
        } else if (Input.GetKeyDown(KeyCode.Space))
        {
            GetVision();
        } else if (Input.GetKeyDown(KeyCode.S))
        {
            DrawVision();
        }

        EatIfPossible();

    }
    
    //-----MOVEMENT-----
    private bool Forward()
    {
        
        Vector3 newPos = transform.position + forwardSpeed * Vector3.Normalize(transform.right);

        RaycastHit[] hits = Physics.BoxCastAll(newPos, bc.size / 2, transform.right, transform.rotation, 0.01f);

        bool canMove = true;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform.GetComponent<ObstacleObj>())
            {
                canMove = false;
            }
        }
        if (canMove)
        {
            transform.position = newPos;
            return true;
        }

        return false;
    }

    private void TurnLeft()
    {
        transform.Rotate(Vector3.up, -turnSpeed);
    }

    private void TurnRight()
    {
        transform.Rotate(Vector3.up, turnSpeed);
    }
 
    
    //-----OBSERVATION-----
    float[] GetVision()
    {
        float start_angle = -fov / 2f;
        float angle_step = fov / (float) (nbRays-1);

        float[] visionData = new float[nbRays];
        

        for (int i = 0; i < nbRays; i++)
        {
            if (Physics.Raycast(sensorTr.position,
                (Quaternion.Euler(0f, start_angle + angle_step * (float) i, 0f) * sensorTr.right) * visionLength,
                out RaycastHit hit, visionLength))
            {
                if (hit.transform.TryGetComponent(out IWorldObject obj))
                {
                    visionData[i] = obj.GetVisionValue();
                    if (DEBUG)
                    {
                        Debug.DrawRay(
                            sensorTr.position, 
                            (Quaternion.Euler(0f, start_angle + angle_step* (float)i, 0f) * sensorTr.right) *visionLength,
                            Color.magenta,
                            2f
                        );
                    }
                    
                }
                
            }
            else
            {
                visionData[i] = noHitValue;
            }
        }

        return visionData;
    }
    
    public float GetVisionValue()
    {
        return antValue;
    }
    
    
    //-----EAT-----
    public float EatIfPossible()
    {

        float totalEnergy = 0f;
        RaycastHit[] hits = Physics.SphereCastAll(sensorTr.position, 0.1f, transform.right, 0.1f);
        
        foreach(RaycastHit hit in hits)
        {
            if (hit.transform.TryGetComponent(out FoodObj food))
            {
                energy += food.GetEnergy();
                totalEnergy += food.GetEnergy();
                food.EatFood();
            }
        }

        if (DEBUG)
        {
            txt.text = (int) energy + ".";
        }
        
        return totalEnergy;
    }
    
    
    
    //-----DEBUG-----
    private void DrawVision()
    {
        
        float start_angle = -fov / 2f;
        float angle_step = fov / (float) (nbRays-1);

        for (int i = 0; i < nbRays; i++)
        {
            
            Debug.DrawRay(
                sensorTr.position, 
                (Quaternion.Euler(0f, start_angle + angle_step* (float)i, 0f) * sensorTr.right) *visionLength,
                Color.red,
                2f
            );
            
        }
        
    }
    
}

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
    [SerializeField] protected float subraySideLength;
    [SerializeField] protected int subraySideNb;
    

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
    float[,,] GetVision()
    {
        float start_angle = -fov / 2f;
        float angle_step = fov / (float) (nbRays-1);

        // visionData[0,0,0] : ray on the top left
        // visionData[0,0,1] : ray on the top and second place from the left
        float[,,] visionData = new float[nbRays, subraySideNb, subraySideNb];
        
        for (int i = 0; i < nbRays; i++)
        {
            //red axis
            Vector3 forwardDirection = (Quaternion.Euler(0f, start_angle + angle_step * (float) i, 0f) * sensorTr.right).normalized;
            
            //blue axis
            Vector3 leftDirection = (Quaternion.Euler(0f, start_angle + angle_step * (float) i, 0f) * sensorTr.forward).normalized;
            Vector3 topDirection = sensorTr.up.normalized;

            float between_rays = subraySideLength / (float) subraySideNb;

            Vector3 first_point = sensorTr.position - ((subraySideLength / 2f) * leftDirection) -
                                  ((subraySideLength / 2f) * topDirection);

            for (int i_vert = 0; i_vert < subraySideNb; i_vert++)
            {
                for (int i_hor = 0; i_hor < subraySideNb; i_hor++)
                {
                    Vector3 currentRayStart = first_point + (i_vert * between_rays * leftDirection) +
                                              (i_hor * between_rays * topDirection);

                    if (Physics.Raycast(currentRayStart, forwardDirection, out RaycastHit hit, visionLength))
                    {
                        if (hit.transform.TryGetComponent(out IWorldObject obj))
                        {
                            visionData[i, subraySideNb - i_vert - 1, subraySideNb - i_hor - 1] = obj.GetVisionValue();
                            if (DEBUG)
                            {
                                Debug.DrawRay(
                                    currentRayStart, 
                                    forwardDirection * visionLength,
                                    Color.magenta,
                                    2f
                                );
                            }
                    
                        }
                    }
                    else
                    {
                        visionData[i, subraySideNb - i_vert - 1, subraySideNb - i_hor - 1] = noHitValue;
                    }

                }
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

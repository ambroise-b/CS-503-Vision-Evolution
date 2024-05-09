using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Random = UnityEngine.Random;


public class AntAgent : Agent, IWorldObject
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

    [SerializeField] private TerrainInstance terrainInstance;

    private float noHitValue = 0f;
    private float antValue = 1f;
    private float stepEnergyCost = 0.1f;
    
    
    private BoxCollider bc;
    private float energy;
    private int foodGathered;

    [SerializeField]private bool DEBUG = true;

    private float[,] viewFilter;
    private float normalizeFactor;


    private void Start()
    {
        bc = GetComponent<BoxCollider>();

        viewFilter = CreateGaussianArray(subraySideNb, 1f, 1f);

        //3f is the max value of GetVisionValue among
        normalizeFactor = 1/( viewFilter[subraySideNb / 2, subraySideNb / 2] * 3f); 
        
        

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
    private float[,,] GetSubVision()
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


    private float[] GetVision()
    {
        float[,,] subVision = GetSubVision();
        float nbElts = subraySideNb * subraySideNb;

        float[] ret = new float[nbRays];
        
        
        //Debug.Log(subVision.Length/(subVision.GetLength(0)*viewFilter.Length));

        for (int k = 0; k < subVision.GetLength(0); k++)
        {
            float totalK = 0f;
            
            for (int i = 0; i < subVision.GetLength(1); i++)
            {
                for (int j = 0; j < subVision.GetLength(2); j++)
                {
                    totalK += subVision[k, i, j] * viewFilter[i, j];
                }
            }

            totalK = (totalK/nbElts) * normalizeFactor;
            ret[k] = totalK;
        }

        return ret;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {

        float[] visionData = GetVision();
        // Target and Agent positions
        //sensor.AddObservation(Target.localPosition);
        for (int i = 0; i < nbRays; i++)
        {
            sensor.AddObservation(visionData[i]);
        }
        
        sensor.AddObservation(energy);
        
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int movement = actionBuffers.DiscreteActions[0];

        if (movement == 0)
        {
            Forward();
        } else if (movement == 1)
        {
            TurnLeft();
        } else if (movement == 2)
        {
            TurnRight();
        }

        float addEnergy = EatIfPossible();

        if (addEnergy > 0f)
        {
            SetReward(10f);
        }

        energy -= stepEnergyCost;

        if (energy <= 0f)
        {
            SetReward(-50f);
            EndEpisode();
        }
        else
        {
            SetReward(-0.1f);
        }
        
        //check if is there is still some food

        if (foodGathered >= terrainInstance.GetTotalFood())
        {
            EndEpisode();
        }

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
                foodGathered += 1;
                
                break;
            }
        }

        if (DEBUG)
        {
            txt.text = (int) energy + ".";
        }
        
        return totalEnergy;
    }
    
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 0;
        } else if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 1;
        } else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 2;
        }
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

    public override void OnEpisodeBegin()
    {
        energy = beginEnergy;
        foodGathered = 0;
        
        terrainInstance.GenerateRandomTerrain(this);
    }

    
    private static float[,] CreateGaussianArray(int size, float sigma, float spacing)
    {
        float[,] grid = new float[size, size];
        int center = size / 2;
        float mu = 0f;  // Center of the peak
        float normalization = 1f / (2f * Mathf.PI * sigma * sigma);
        
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = (i - center) * spacing;
                float y = (j - center) * spacing;
                grid[i, j] = Mathf.Abs(normalization * Mathf.Exp(-((x - mu) * (x - mu) + (y - mu) * (y - mu)) / (2 * sigma * sigma)));
            }
        }
        
        return grid;
    }



}

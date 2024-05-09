using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class TerrainInstance : MonoBehaviour
{

    [SerializeField] protected GameObject foodBlock;
    [SerializeField] protected GameObject obstacleBlock;
    [SerializeField] protected GameObject antAgent;

    [SerializeField] protected int foodBlockNbMin;
    [SerializeField] protected int foodBlockNbMax;

    [SerializeField] protected int obstacleBlockNbMin;
    [SerializeField] protected int obstacleBlockNbMax;

    private float terrainSize = 15f;
    private int maxIteration = 500;
    private float cellSize = 0.7f;
    

    private GameObject[] obstacleCreated;
    private GameObject[] foodCreated;

    private int totalFood;

    protected void Awake()
    {
        obstacleCreated = new GameObject[obstacleBlockNbMax];
        foodCreated = new GameObject[foodBlockNbMax];

    }


    public void GenerateRandomTerrain(AntAgent antAgent)
    {
        
        
        int nbCellSide = (int)Mathf.Floor(terrainSize / cellSize);



        bool[,] isFullGrid = new bool[nbCellSide, nbCellSide];
        
        Vector3 startPoint = transform.position - ((terrainSize / 2f) * Vector3.forward) -
                             ((terrainSize / 2f) * Vector3.right);
        

        int nbFood = Random.Range(foodBlockNbMin, foodBlockNbMax + 1);
        int nbObs = Random.Range(obstacleBlockNbMin, obstacleBlockNbMax + 1);

        totalFood = nbFood;

        //we use this function to do pooling instead recrating each object at the end of each episode
        AdjustObjectsNumber(nbObs, nbFood, startPoint);
        
        
        /*Debug.Log("nbFood = " + nbFood );
        Debug.Log("foodCreated = " + foodCreated.Count);
        Debug.Log("nbObs = " + nbObs);
        Debug.Log("obsCreated = " + obstacleCreated.Count);*/

        //placement : food and obstacle
        int[,] foodPlacement = new int[nbFood, 2];
        int[,] obsPlacement = new int[nbObs, 2];

        int nbFoodPlaced = 0;
        int nbObsPlaced = 0;
        
        for(int i = 0; i < maxIteration; i++)
        {
            //choose which object to drop
            if (nbFoodPlaced >= nbFood && nbObsPlaced >= nbObs)
            {
                break;
            }
            int goToPlace = -1; //0: obstacle, 1: food
            
            if (nbFoodPlaced >= nbFood)
            {
                goToPlace = 0;
            } else if (nbObsPlaced >= nbObs)
            {
                goToPlace = 1;
            }
            else
            {
                int objChoice = Random.Range(0, nbFood + nbObs + 1);

                if (objChoice < nbFood)
                {
                    goToPlace = 1;
                }
                else
                {
                    goToPlace = 0;
                }
            }
            
            //choose where to drop
            int coord0 = Random.Range(0, nbCellSide);
            int coord1 = Random.Range(0, nbCellSide);

            if (!isFullGrid[coord0, coord1])
            {
                //free spot
                isFullGrid[coord0, coord1] = true;

                if (goToPlace == 0)
                {

                    if (nbObsPlaced > nbObs)
                    {
                        Debug.LogError("Max index reached ");
                    }
                    
                    //obstacle
                    obsPlacement[nbObsPlaced, 0] = coord0;
                    obsPlacement[nbObsPlaced, 1] = coord1;
                    nbObsPlaced++;
                }
                else
                {
                    
                    if (nbFoodPlaced > nbFood)
                    {
                        Debug.LogError("Max index reached ");
                    }
                    
                    foodPlacement[nbFoodPlaced, 0] = coord0;
                    foodPlacement[nbFoodPlaced, 1] = coord1;
                    nbFoodPlaced++;
                }
                
            }


        }
        //placement : ant
        int[,] agentPlacementCandidates = new int[(nbCellSide - 2) * (nbCellSide - 2), 2];
        int totalCandidates = 0;

        for (int i0 = 1; i0 < nbCellSide - 1; i0++)
        {
            for (int i1 = 1; i1 < nbCellSide - 1; i1++)
            {
                if (
                    !isFullGrid[i0 - 1, i1 - 1] && !isFullGrid[i0 - 1, i1] && !isFullGrid[i0 - 1, i1 + 1] &&
                    !isFullGrid[i0, i1 - 1] && !isFullGrid[i0, i1] && !isFullGrid[i0, i1 + 1] &&
                    !isFullGrid[i0 + 1, i1 - 1] && !isFullGrid[i0 + 1, i1] && !isFullGrid[i0 + 1, i1 + 1]
                )
                {
                    agentPlacementCandidates[totalCandidates, 0] = i0;
                    agentPlacementCandidates[totalCandidates, 1] = i1;

                    totalCandidates++;
                }
            }
        }

        int agentPlacementIdx = Random.Range(0, totalCandidates);
        int[] agentPlacementCoords = new[]
            {agentPlacementCandidates[agentPlacementIdx, 0], agentPlacementCandidates[agentPlacementIdx, 1]};



        //instantiation : food


        //coord0 -> forward
        //coord1 -> right
        

        float foodSize = foodBlock.transform.localScale.x;

        Vector3 foodStartPoint = startPoint + (Vector3.up * (foodSize / 2f));
        
        
        
        for (int i = 0; i < nbFoodPlaced; i++)
        {
            float objectAngle = Random.Range(0f, 90f);
            Vector3 objPosition = foodStartPoint + ((foodPlacement[i, 0] * cellSize) + cellSize/2) * Vector3.forward +
                                  ((foodPlacement[i, 1] * cellSize) + cellSize/2) * Vector3.right;

            //Instantiate(foodBlock, objPosition, quaternion.Euler(0f, objectAngle, 0f), transform);

            foodCreated[i].transform.position = objPosition;
            foodCreated[i].transform.rotation = quaternion.Euler(0f, objectAngle, 0f);


        }

        //instantiation : obstacle    
        float obsSize = obstacleBlock.transform.localScale.x;
        
        Vector3 obsStartPoint = startPoint + (Vector3.up * (obsSize / 2f));
        
        for (int i = 0; i < nbObsPlaced; i++)
        {
            float objectAngle = Random.Range(0f, 90f);
            Vector3 objPosition = obsStartPoint + ((obsPlacement[i, 0] * cellSize) + cellSize/2) * Vector3.forward +
                                  ((obsPlacement[i, 1] * cellSize) + cellSize/2) * Vector3.right;

            //Instantiate(obstacleBlock, objPosition, quaternion.Euler(0f, objectAngle, 0f), transform);

            obstacleCreated[i].transform.position = objPosition;
            obstacleCreated[i].transform.rotation = quaternion.Euler(0f, objectAngle, 0f);


        }
        
        //AdjustObjectsNumber(nbObsPlaced, nbFoodPlaced, startPoint);

        //instantiate ant
        float antAngle = Random.Range(0f, 90f);

        Vector3 antPosition = startPoint + ((agentPlacementCoords[0] * cellSize) + cellSize / 2) * Vector3.forward +
                              ((agentPlacementCoords[1] * cellSize) + cellSize / 2) * Vector3.right + (0.01f * Vector3.up);


        //Instantiate(antAgent, antPosition, quaternion.Euler(0f, antAngle, 0f), null);

        antAgent.transform.position = antPosition;
        antAgent.transform.rotation = quaternion.Euler(0f, antAngle, 0f);
        


    }

    private void AdjustObjectsNumber(int nbObs, int nbFood, Vector3 startPoint)
    {
        if (obstacleCreated[0] == null)
        {
            Debug.Log("first iteration");
            
            for (int i = 0; i < obstacleCreated.Length; i++)
            {
                GameObject goCreated = Instantiate(obstacleBlock, startPoint, quaternion.identity, transform);
                obstacleCreated[i] = goCreated;
            }
            
            for (int i = 0; i < foodCreated.Length; i++)
            {
                GameObject goCreated = Instantiate(foodBlock, startPoint, quaternion.identity, transform);
                foodCreated[i] = goCreated;
            }
        }
        
        
        Debug.Log("nbFood = " + nbFood);
        Debug.Log("nbObs = " + nbObs);


        for (int i = 0; i < nbObs; i++)
        {
            obstacleCreated[i].SetActive(true);
        }

        for (int i = nbObs; i < obstacleCreated.Length; i++)
        {
            obstacleCreated[i].SetActive(false);
        }
        
        for (int i = 0; i < nbFood; i++)
        {
            foodCreated[i].SetActive(true);
            
        }

        for (int i = nbFood; i < foodCreated.Length; i++)
        {
            foodCreated[i].SetActive(false);
        }
        

        /*if (obstacleCreated.Count == nbObs && foodCreated.Count == nbFood)
        {
            return;
        }
        
        int nbObsToInst = 0;
        int nbObsToDestroy = 0;
        int nbFoodToInst = 0;
        int nbFoodToDestroy = 0;
        

        if (obstacleCreated.Count >= nbObs)
        {
            //need to destroy obstacles
            nbObsToDestroy = obstacleCreated.Count - nbObs;
        }
        else
        {
            //need to instantiate obstacles
            nbObsToInst = nbObs - obstacleCreated.Count;
        }
        
        if (foodCreated.Count >= nbFood)
        {
            //need to destroy food
            nbFoodToDestroy = foodCreated.Count - nbFood;
        }
        else
        {
            //need to instantiate food
            nbFoodToInst = nbFood - foodCreated.Count;
        }


        if (nbObsToDestroy > 0)
        {
            for (int i = 0; i < nbObsToDestroy; i++)
            {
                GameObject goToDestroy = obstacleCreated[obstacleCreated.Count - 1];
                obstacleCreated.RemoveAt(obstacleCreated.Count - 1);
                Destroy(goToDestroy);

            }
        }
        
        if (nbFoodToDestroy > 0)
        {
            for (int i = 0; i < nbFoodToDestroy; i++)
            {
                GameObject goToDestroy = foodCreated[foodCreated.Count - 1];
                foodCreated.RemoveAt(foodCreated.Count - 1);
                Destroy(goToDestroy);

            }
        }

        if (nbObsToInst > 0)
        {

            for (int i = 0; i < nbObsToInst; i++)
            {
                GameObject goCreated = Instantiate(obstacleBlock, startPoint, quaternion.identity, transform);
                obstacleCreated.Add(goCreated);
            }
            
        }
        
        if (nbFoodToInst > 0)
        {
            for (int i = 0; i < nbFoodToInst; i++)
            {
                GameObject goCreated = Instantiate(foodBlock, startPoint, quaternion.identity, transform);
                foodCreated.Add(goCreated);
            }

        }*/
    }
    

    public int GetTotalFood()
    {
        return totalFood;
    }



}

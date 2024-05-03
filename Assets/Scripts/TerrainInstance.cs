using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
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
    
    private void Awake()
    {
        GenerateRandomTerrain();
    }


    private void GenerateRandomTerrain()
    {
        
        
        int nbCellSide = (int)Mathf.Floor(terrainSize / cellSize);

        bool[,] isFullGrid = new bool[nbCellSide, nbCellSide];

        int nbFood = Random.Range(foodBlockNbMin, foodBlockNbMax + 1);
        int nbObs = Random.Range(obstacleBlockNbMin, obstacleBlockNbMax + 1);
        
        /*Debug.Log("Nb food " + nbFood);
        Debug.Log("Nb obs" + nbObs);*/
        
        
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
        int[,] agentPlacementCandidates = new int[(nbCellSide - 2)*(nbCellSide - 2),2];
        int totalCandidates = 0;
        
        for (int i0 = 1; i0 < nbCellSide - 1; i0++)
        {
            for (int i1 = 1; i1 < nbCellSide - 1; i1++)
            {
                if (
                    !isFullGrid[i0 -1, i1 -1] && !isFullGrid[i0 -1, i1] && !isFullGrid[i0 -1, i1 +1] &&
                    !isFullGrid[i0, i1 -1] && !isFullGrid[i0, i1] && !isFullGrid[i0, i1 +1] &&
                    !isFullGrid[i0 +1, i1 -1] && !isFullGrid[i0 +1, i1] && !isFullGrid[i0 +1, i1 +1]
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
        Vector3 startPoint = transform.position - ((terrainSize / 2f) * Vector3.forward) -
                             ((terrainSize / 2f) * Vector3.right);

        float foodSize = foodBlock.transform.localScale.x;

        Vector3 foodStartPoint = startPoint + (Vector3.up * (foodSize / 2f));
        
        
        
        for (int i = 0; i < nbFoodPlaced; i++)
        {
            float objectAngle = Random.Range(0f, 90f);
            Vector3 objPosition = foodStartPoint + ((foodPlacement[i, 0] * cellSize) + cellSize/2) * Vector3.forward +
                                  ((foodPlacement[i, 1] * cellSize) + cellSize/2) * Vector3.right;

            Instantiate(foodBlock, objPosition, quaternion.Euler(0f, objectAngle, 0f), transform);
            
        }

        //instantiation : obstacle    
        float obsSize = obstacleBlock.transform.localScale.x;
        
        Vector3 obsStartPoint = startPoint + (Vector3.up * (obsSize / 2f));
        
        for (int i = 0; i < nbObsPlaced; i++)
        {
            float objectAngle = Random.Range(0f, 90f);
            Vector3 objPosition = obsStartPoint + ((obsPlacement[i, 0] * cellSize) + cellSize/2) * Vector3.forward +
                                  ((obsPlacement[i, 1] * cellSize) + cellSize/2) * Vector3.right;

            Instantiate(obstacleBlock, objPosition, quaternion.Euler(0f, objectAngle, 0f), transform);
        }
        
        //instantiate ant
        float antAngle = Random.Range(0f, 90f);
        
        Vector3 antPosition = startPoint + ((agentPlacementCoords[0] * cellSize) + cellSize/2) * Vector3.forward +
                              ((agentPlacementCoords[1] * cellSize) + cellSize/2) * Vector3.right + (0.01f * Vector3.up);


        Instantiate(antAgent, antPosition, quaternion.Euler(0f, antAngle, 0f), null);


    }
}

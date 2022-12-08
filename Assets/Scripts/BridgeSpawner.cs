using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeSpawner : MonoBehaviour
{

//                     ********************************
//                     |  Written by:  Savran Donmez  |
//                     |  Start Date:  04/12/2022     |
//                     |  Last Update: 08/12/2022     |
//                     ********************************

    #region GLOBAL VAR.

    public GameObject[] bridgePrefabs;

    enum enType
    {
        L_Corner,
        Straight,
        R_Corner
    }

    enum enDirection
    {
        North,
        East,
        West,
    }

    class Segments
    {
        public GameObject segPrefab;
        public enType segType;

        public Segments(GameObject segPrefab, enType segType)
        {
            this.segPrefab = segPrefab;
            this.segType = segType;
        }
    };

    List<GameObject> activeSegments = new List<GameObject> ();
    Segments segment;
    Vector3 spawnCoord = new Vector3(0, 0, -6f);
    enDirection segCurrentdirection = enDirection.North;
    enDirection segNextDirection = enDirection.North;
    Transform playerTransform;

    float segLength = 6f;
    float segWidth = 3f;
    int segsOnScreen = 5; // More than 5 is too much for phones.
    bool stopGame = false;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        segment = new Segments(bridgePrefabs[0], enType.Straight);
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        InitializeSegments();
    }

    // Update is called once per frame
    void Update()
    {
        PlayerTrigger();
    }

    void InitializeSegments()
    {
        for (int i = 0; i < segsOnScreen; i++)
        {
            //CreateSegments();
            SpawnSegments();
        }
    }

    void CreateSegments()
    {
        /* enType.L_Corner  is  bridgePrefabs[6]
           enType.R_Corner  is  bridgePrefabs[7]
           enType.straight  is  bridgePrefabs[Random.Range(0,6)] */

        switch (segCurrentdirection) // the Logic for NOT Turning BACK on world.
        {
            case enDirection.North: // forward, right and left are possible
                segment.segType = (enType)Random.Range(0, 3);
                if(segment.segType == enType.Straight) 
                {
                    segment.segPrefab = bridgePrefabs[Random.Range(0,11)];
                }
                else if(segment.segType == enType.L_Corner) 
                {
                    segment.segPrefab = bridgePrefabs[11]; 
                }
                else if (segment.segType == enType.R_Corner) 
                {
                    segment.segPrefab = bridgePrefabs[12]; 
                }
                break;
            case enDirection.East: // only forward and left are possible
                segment.segType = (enType)Random.Range(0, 2);
                if(segment.segType == enType.Straight)
                {
                    segment.segPrefab = bridgePrefabs[Random.Range(0, 11)];

                }
                else if( segment.segType == enType.L_Corner)
                {
                    segment.segPrefab = bridgePrefabs[11];
                }
                break;
            case enDirection.West: // only forward and right are possible
                segment.segType = (enType)Random.Range(1, 3);
                if (segment.segType == enType.Straight)
                {
                    segment.segPrefab = bridgePrefabs[Random.Range(0, 11)];

                }
                else if (segment.segType == enType.R_Corner)
                {
                    segment.segPrefab = bridgePrefabs[12];
                }
                break;

        }
    }

    // Adds the created bridges to the game
    void SpawnSegments()
    {
        GameObject prefabToInstantiate = segment.segPrefab;
        Quaternion prefabRotation = Quaternion.identity;
        Vector3 offSet = new Vector3(0,0,0); // for turnings.

        switch (segCurrentdirection)
        {
            // NORTH path logic.
            case enDirection.North:
                if (segment.segType == enType.Straight)
                {
                    prefabRotation = Quaternion.Euler(0, 0, 0);
                    segNextDirection = enDirection.North;
                    spawnCoord.z += segWidth;
                }
                else if(segment.segType == enType.R_Corner)
                {
                    prefabRotation = Quaternion.Euler(0, 0, 0);
                    segNextDirection = enDirection.East;
                    spawnCoord.z += segLength;
                    offSet.z += segLength + segWidth / 2;
                    offSet.x += segWidth / 2;
                }
                else if (segment.segType == enType.L_Corner)
                {
                    prefabRotation = Quaternion.Euler(0, 0, 0);
                    segNextDirection = enDirection.West;
                    spawnCoord.z += segLength;
                    offSet.z += segLength + segWidth / 2;
                    offSet.x -= segWidth / 2;
                }
                break;

            // EAST path logic.
            case enDirection.East:
                if (segment.segType == enType.Straight)
                {
                    prefabRotation = Quaternion.Euler(0, 90, 0);
                    segNextDirection = enDirection.East;
                    spawnCoord.x += segLength;
                }
                else if (segment.segType == enType.L_Corner)
                {
                    prefabRotation = Quaternion.Euler(0, 90, 0);
                    segNextDirection = enDirection.North;
                    spawnCoord.x += segLength;
                    offSet.z += segWidth / 2;
                    offSet.x += segLength + segWidth / 2;
                }
                break;

            // WEST path logic.
            case enDirection.West:
                if (segment.segType == enType.Straight)
                {
                    prefabRotation = Quaternion.Euler(0, -90, 0);
                    segNextDirection = enDirection.West;
                    spawnCoord.x -= segLength;
                }
                else if (segment.segType == enType.R_Corner)
                {
                    prefabRotation = Quaternion.Euler(0, -90, 0);
                    segNextDirection = enDirection.North;
                    spawnCoord.x -= segLength;
                    offSet.z += segWidth / 2;
                    offSet.x -= segLength + segWidth / 2;
                }
                break;
        }

        if(prefabToInstantiate != null)
        {
            GameObject spawnedPrefab = Instantiate(prefabToInstantiate, spawnCoord, prefabRotation) as GameObject;
            activeSegments.Add(spawnedPrefab);
            spawnedPrefab.transform.parent = this.transform;
        }

        segCurrentdirection = segNextDirection;
        spawnCoord += offSet;
    }

    //Removes the bridges behind the player
    void RemoveSegments()
    {
        Destroy(activeSegments[0]);
        activeSegments.RemoveAt(0);
    }

    void PlayerTrigger()
    {
        if (stopGame)
            return;

        GameObject go = activeSegments[0];

        if(Mathf.Abs(Vector3.Distance(playerTransform.position, go.transform.position)) > 15f)
        {
            CreateSegments();
            SpawnSegments();
            RemoveSegments();
        }
    }

    public void CleanScene()
    {
        stopGame = true;

        for(int i = activeSegments.Count - 1; i >= 0; i--)
        {
            Destroy(activeSegments[i]);
            activeSegments.RemoveAt(i);
        }

        spawnCoord = new Vector3(0, 0, -6);
        segCurrentdirection = enDirection.North;
        segNextDirection = enDirection.North;
        segment = new Segments(bridgePrefabs[0], enType.Straight);
        InitializeSegments();

        stopGame = false;
    }
}

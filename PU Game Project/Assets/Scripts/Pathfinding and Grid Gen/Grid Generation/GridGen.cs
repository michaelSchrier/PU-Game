﻿//Generates the Grid. Forms a grid of size equal to gridWorldSize. Divides the region into nodes based on the given Node radius. Currently it is set to 0.5. Note, TilePrefabs are spawned here and
//WILL NOT change size based on the Node radius. 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GridGen : MonoBehaviour {

	public bool displayGridGizmos;
    public bool doDiagonal;
	public LayerMask obstacleMask;
    public LayerMask baseMapGenMask;
	private Vector3 gridWorldSize;
    public float nodeRadius = 0.5f;
    public int maxCielingHeight = 4;
    public float tileGraphicSpawnHeight = 0.05f;
    public GameObject tilePrefab;
	public Node[,,] grid;
    private GameObject gridParent;
    RaycastHit[] hitObjects;

	float nodeDiameter;
	int gridSizeX, gridSizeY, gridSizeZ;

    public static GridGen instance;

	void Awake() {

        instance = this;

        BoxCollider boxCol = GetComponent<BoxCollider>();
        gridWorldSize.x = boxCol.size.x;
        gridWorldSize.y = boxCol.size.y;
        gridWorldSize.z = boxCol.size.z;


        if (nodeRadius < 0.05f) //Just to make sure no one can plug in 0 and break the grid gen. 
        {
            nodeRadius = 0.05f;
        }
		nodeDiameter = nodeRadius*2;

		gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y);
        gridSizeZ = Mathf.RoundToInt(gridWorldSize.z/nodeDiameter);

        //StartCoroutine(CreateGrid());
        CreateGrid();
	}

	public int MaxSize {
		get {
			return gridSizeX * gridSizeY * gridSizeZ;
		}
	}

    private void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY, gridSizeZ];
        Vector3 worldBottomTopLeft = transform.position - (Vector3.right * gridWorldSize.x / 2) + (Vector3.up * gridWorldSize.y / 2) - (Vector3.forward * gridWorldSize.z / 2);

        gridParent = new GameObject("Map Hierarchy");

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                Vector3 worldPoint = worldBottomTopLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (z * nodeDiameter + nodeRadius);
                hitObjects = Physics.RaycastAll(worldPoint, Vector3.down, gridSizeY, CombatUtils.nodeSelectionMask);
                Debug.DrawRay(worldPoint, Vector3.down * gridSizeY, Color.blue, 2f);

                hitObjects = hitObjects.OrderBy(hitObject => Vector3.Distance(worldPoint, hitObject.collider.bounds.center)).ToArray();

                Collider lastCollider = null;
                foreach (RaycastHit currentHit in hitObjects)
                {
                    if (lastCollider != null &&
                        (lastCollider.bounds.center.y - (lastCollider.bounds.size.y / 2) < currentHit.collider.bounds.center.y + (currentHit.collider.bounds.size.y / 2) + maxCielingHeight)) 
                    {
                        continue;
                    }
                    lastCollider = currentHit.collider;

                    Vector3 nodePoint = new Vector3(worldPoint.x, (int)currentHit.point.y, worldPoint.z);
                    bool walkable = !(Physics.CheckSphere(nodePoint, nodeRadius, obstacleMask));

                    //Debug.Log("Current Hit Y: " + currentHit.point.y);
                    //Debug.Log("Node Point Y: " + nodePoint.y);
                    int yGridPos = (int)(gridSizeY - currentHit.distance);

                    GameObject tileObject = Instantiate(tilePrefab, new Vector3(nodePoint.x, nodePoint.y + tileGraphicSpawnHeight, nodePoint.z), Quaternion.identity);
                    tileObject.name = "Tile " + x + "," + yGridPos + "," + z;
                    tileObject.transform.SetParent(gridParent.transform);

                    grid[x, yGridPos, z] = new Node(walkable, nodePoint, x, yGridPos, z, tileObject);

                    //yield return null;
                }

                //yield return null;
            }
        }

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for( int z = 0; z < gridSizeZ; z++)
                {
                    if(grid[x,y,z] != null)
                    {
                        grid[x, y, z].nodeNeighbors = GetNeighbours(grid[x, y, z]);
                    }
                }
            }
        }
    }

	public List<Node> GetNeighbours(Node node) //Finds all nodes around the given node in a 3v3 square.
    {
		List<Node> neighbours = new List<Node>();

        if (doDiagonal)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && z == 0) 
                        continue;

                    int checkX = node.gridX + x;
                    int checkZ = node.gridZ + z;

                    if (checkX >= 0 && checkX < gridSizeX && checkZ >= 0 && checkZ < gridSizeZ)
                    {
                        if(grid[checkX, node.gridY, checkZ] != null)
                        {
                            neighbours.Add(grid[checkX, node.gridY, checkZ]);
                        }
                    }
                }
            }
        }
        else
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if ((x == 0 && z == 0) || (x == 1 && z == 1) || (x == -1 && z == 1) || (x == -1 && z == -1) || (x == 1 && z == -1)) // Removes corners and the center (center is the original Node).
                        continue;

                    int checkX = node.gridX + x;
                    int checkZ = node.gridZ + z;

                    if (checkX >= 0 && checkX < gridSizeX && checkZ >= 0 && checkZ < gridSizeZ)
                    {
                        if (grid[checkX, node.gridY, checkZ] != null)
                        {
                            neighbours.Add(grid[checkX, node.gridY, checkZ]);
                        }
                    }
                }
            }
        }

		return neighbours;
	}
	

	public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        return NodeFromWorldPoint(worldPosition, false);
    }
    public Node NodeFromWorldPoint(Vector3 worldPosition, bool fromtTile)
    {
        float posX = ((worldPosition.x - transform.position.x) + gridWorldSize.x * 0.5f) / nodeDiameter;

        float posY = 0;
        if (fromtTile)
        { posY = ((worldPosition.y - transform.position.y - tileGraphicSpawnHeight) + gridWorldSize.y * 0.5f);}
        else
        { posY = ((worldPosition.y - transform.position.y) + gridWorldSize.y * 0.5f);}

        float posZ = ((worldPosition.z - transform.position.z) + gridWorldSize.z * 0.5f) / nodeDiameter;

        posX = Mathf.Clamp(posX, 0, gridWorldSize.x - 1);
        posY = Mathf.Clamp(posY, 0, gridWorldSize.y - 1);
        posZ = Mathf.Clamp(posZ, 0, gridWorldSize.z - 1);

        int x = Mathf.FloorToInt(posX);
        int y = Mathf.FloorToInt(posY);
        int z = Mathf.FloorToInt(posZ);

        //Debug.Log("Given Grid Index: " + x + ", " + y + ", " + z);

        return grid[x, y, z];
    }

    /*
	void OnDrawGizmos() {
		Gizmos.DrawWireCube(transform.position,new Vector3(gridWorldSize.x,1,gridWorldSize.y));
		if (grid != null && displayGridGizmos) {
			foreach (Node n in grid) {
				Gizmos.color = (n.IsWalkable)?Color.white:Color.red;
				Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter-.1f));
			}
		}
	}
    */
    
}
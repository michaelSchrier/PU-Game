﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CursorController : MonoBehaviour
{
    public GameObject visual;

    public Node currentNode;
    public Vector3 nodePos;

    public float catchupSpeed = 20f;

    private bool isActive;

    private bool attemptFloorUp;
    private bool attemptFloorDown;

    public delegate void CursorNewNode(Node givenNode);
    public event CursorNewNode CursorNewNodeEVENT;

    public List<GameObject> shieldPartial = new List<GameObject>();
    public List<GameObject> shieldFull = new List<GameObject>();

    public static CursorController instance;

    private void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(this); }

        visual.SetActive(false);
    }


    void Start()
    {
        ClickSelection.instance.NewSelectionEvent += NewSelection;
        ClickSelection.instance.ClearSelectionEvent += ClearedSelection;
    }

    private void Update()
    {
        if (isActive && transform.position != nodePos)
        {
            visual.transform.Translate((nodePos - visual.transform.position) * Time.deltaTime * catchupSpeed);
        }
    }

    void LateUpdate()
    {
        if (isActive)
        {
            SetPositionOfCursor();
        }
    }


    private void SetPositionOfCursor()
    {
        Node foundNode = null;

        RaycastHit[] hitInfos = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, CombatUtils.gameTerrainMask);
        System.Array.Sort(hitInfos, (x, y) => x.distance.CompareTo(y.distance));

        foreach(RaycastHit currentHit in hitInfos)
        {
            
            if (currentHit.collider.CompareTag("Floor") || currentHit.collider.CompareTag("Map Base"))
            {
                foundNode = GridGen.instance.NodeFromWorldPoint(currentHit.point);
                break;
            }
        }

        if (foundNode != null && foundNode != currentNode)
        {
            currentNode = foundNode;
            nodePos = foundNode.worldPosition;

            if (CursorNewNodeEVENT != null)
            {
                CursorNewNodeEVENT(currentNode); //EVENT for new Node selection. 
            }

            RepositionCoverMarkers();
            
        }
    }

    private void NewSelection(GameObject selection)
    {
        isActive = true;
        visual.SetActive(true);
        currentNode = GridGen.instance.NodeFromWorldPoint(selection.transform.position);
        nodePos = currentNode.worldPosition;
        visual.transform.position = currentNode.worldPosition;

        RepositionCoverMarkers();
    }

    private void ClearedSelection()
    {
        isActive = false;
        visual.SetActive(false);
    }

    private void RepositionCoverMarkers()
    {
        Vector3 firePosPartial = new Vector3(currentNode.worldPosition.x, currentNode.worldPosition.y + (0.5f * GridGen.instance.nodeHeightDif), currentNode.worldPosition.z);
        Vector3 firePosFull = new Vector3(currentNode.worldPosition.x, currentNode.worldPosition.y + (1.5f * GridGen.instance.nodeHeightDif), currentNode.worldPosition.z);

        RepositionHelper(firePosFull, firePosPartial, Vector3.forward, 0);
        RepositionHelper(firePosFull, firePosPartial, Vector3.right, 1);
        RepositionHelper(firePosFull, firePosPartial, Vector3.back, 2);
        RepositionHelper(firePosFull, firePosPartial, Vector3.left, 3);
    }

    private void RepositionHelper(Vector3 firePosFull, Vector3 firePosPartial, Vector3 direction, int index)
    {
        //Debug.DrawRay(firePosPartial, direction * GridGen.instance.nodeRadius * 2, Color.red, 2f);
        if (Physics.Raycast(firePosPartial, direction, (GridGen.instance.nodeRadius * 2), CombatUtils.shotMask))
        {
            //Debug.DrawRay(firePosFull, direction * GridGen.instance.nodeRadius * 2, Color.red, 2f);
            if (Physics.Raycast(firePosFull, direction, (GridGen.instance.nodeRadius * 2), CombatUtils.shotMask))
            {
                shieldFull[index].SetActive(true);
                shieldPartial[index].SetActive(false);
            }
            else
            {
                shieldFull[index].SetActive(false);
                shieldPartial[index].SetActive(true);
            }
        }
        else
        {
            shieldFull[index].SetActive(false);
            shieldPartial[index].SetActive(false);
        }
    }
}

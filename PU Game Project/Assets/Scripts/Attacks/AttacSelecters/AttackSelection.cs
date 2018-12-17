﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MHA.BattleBehaviours;

public abstract class AttackSelection : MonoBehaviour {

    public bool hasLoadedTargets;
    public CharAbility givenAbility;
    protected HashSet<Node> collectedNodes = new HashSet<Node>();
    public TargetPacket attachedTargetPacket;


    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            MadeSelection();
        }
        if (Input.GetMouseButtonDown(0))
        {
            CancelSelection();
        }
    }

    protected void MadeSelection()
    {
        NodeDisplayCleanup();
        MadeSelectionImpl();
        attachedTargetPacket.TargetNodes.Add(collectedNodes);
        hasLoadedTargets = true;

        Destroy(this.gameObject);
    }
    public abstract void MadeSelectionImpl();

    protected void CancelSelection()
    {
        NodeDisplayCleanup();
        givenAbility.CancelTargets();
        Destroy(this.gameObject);
    }

    public virtual void NodeDisplayCleanup()
    {
        DrawIndicators.instance.ClearTileMatStates(true, true, true);
    }
}

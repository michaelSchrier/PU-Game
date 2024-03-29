﻿//OUT OF DATE BUFF SYSTEM. TURN INTO BATTLE EVENT.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public abstract class Buff
{

    protected Unit bTarget;
    protected GameEntity bSource;
    protected string bName;

    public bool isTimed;
    public int currentCooldown;
    public int turnCooldown;

    public Buff(Unit _buffTarget, GameEntity _buffSource, string _buffName)
    {
        this.bTarget = _buffTarget;
        this.bName = _buffName;
        this.bSource = _buffSource;
    }

    public Buff(Unit _buffTarget, GameEntity _buffSource, string _buffName, int _turnCooldown)
    {
        this.bTarget = _buffTarget;
        this.bName = _buffName;
        this.bSource = _buffSource;

        turnCooldown = _turnCooldown;
        if(turnCooldown > 0)
        {
            isTimed = true;
            currentCooldown = turnCooldown;
        }

    }

    public void BuffInitialApplication()
    {
        InitialAppImpl();
    }
    protected abstract void InitialAppImpl();

    public void BuffEndTurnApplication()
    {
        if (isTimed)
        {
            Debug.Log("End Turn Buff: " + bTarget.gameObject.name);
            EndTurnAppImpl();
            Debug.Log("Buff Cooldown: " + currentCooldown);
        }
    }
    protected abstract void EndTurnAppImpl();

    public virtual void RemoveSelf()
    {
        Debug.Log(bName + ": Remove Debuff");
        if (bTarget != null)
        {
            bTarget.RemoveBuff(this);
        }
    }

    public virtual void CooldownReduce()
    {
        if (isTimed)
        {
            currentCooldown--;
            if(currentCooldown <= 0)
            {
                RemoveSelf();
            }
        }
    }


}

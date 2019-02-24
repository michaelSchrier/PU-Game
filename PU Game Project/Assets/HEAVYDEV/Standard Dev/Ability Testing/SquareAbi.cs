﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(fileName = "New Box Attack", menuName = "Abilities/General/BoxAttack")]
public class SquareAbi : CharAbility
{

    [Header("Selectors:")]
    [SerializeField]
    SelectorData.Box firstSelector = new SelectorData.Box();
    [Space]

    [Header("Properties")]
    [SerializeField]
    Attack damage = new Attack();

    public override void Initialize(GameEntity givenEntity)
    {
        base.Initialize(givenEntity);

        PrepCast();
        PrepSelectorPacket();
    }

    private void PrepCast()
    {
        castableAbilities.Add(new Action<EffectDataPacket>(Run));
    }
    private void PrepSelectorPacket()
    {
        selectorData.Add(new List<SelectorData>() { firstSelector });
    }

    private void Run(EffectDataPacket effectDataPacket)
    {
        SelectorPacket currentPacket = ((SelectorPacket)effectDataPacket.GetVarValue("Targets", false)[0]); //Gives packet.
        GameObject projectile = AbilityPrefabRef.GiveAbilityPrefab(AbilityPrefabRef.TakahiroBasic);

        TPorterProjectile projectileEffect = new TPorterProjectile(associatedEntity, effectDataPacket, currentPacket, projectile);
        projectileEffect.REPORTKEY = "HitTargets";
        projectileEffect.finishedEffectAuxCall += DamageOnImpact;

        ResolutionManager.instance.LoadBattleEffect(projectileEffect);
    }

    private void DamageOnImpact(EffectDataPacket effectDataPacket)
    {
        GameObject relevantObject = ((GameObject)effectDataPacket.GetVarValue("HitTargets", false).Last()); //Gets access to GameObject Target.
        Attack attack = damage;
        EffectDealDamage effect = new EffectDealDamage(associatedEntity, effectDataPacket, relevantObject.GetComponent<Unit>(), attack);

        ResolutionManager.instance.LoadBattleEffect(effect);
    }
}
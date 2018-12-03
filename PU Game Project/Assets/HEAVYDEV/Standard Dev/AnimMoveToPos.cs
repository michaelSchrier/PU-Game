﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MHA.BattleBehaviours
{
    public class AnimMoveToPos : BattleAnimation
    {

        Vector3 finalPos;
        GameObject moveTarget;
        float speed;
        bool destroyAtEnd;

        public AnimMoveToPos(Vector3 _finalPos, GameObject _moveTarget, float _speed, bool _destroyAtEnd)
        {
            finalPos = _finalPos;
            moveTarget = _moveTarget;
            speed = _speed;
            destroyAtEnd = _destroyAtEnd;

            LoadBattleAnimation();
        }

        protected override void PlayBattleAnimationImpl()
        {
            mono.StartCoroutine(GridMoveAnim());
        }

        private IEnumerator GridMoveAnim()
        {
            while (true)
            {
                if (moveTarget.transform.position == finalPos)
                {
                    break;
                }

                moveTarget.transform.position = Vector3.MoveTowards(moveTarget.transform.position, finalPos, speed * Time.deltaTime);

                yield return null;
            }

            if (destroyAtEnd)
            {
                GameObject.Destroy(moveTarget);
            }

            AnimFinished = true;
        }
    }
}

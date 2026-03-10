using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 僵尸类敌人
/// </summary>
public class ZombieEnemy : EnemyBase
{
    public override void SwitchState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                stateMechine.EnterState<ZombieIdleState>();
                break;
            case EnemyState.Move:
                stateMechine.EnterState<ZombieMoveState>();
                break;
            case EnemyState.Attack:
                stateMechine.EnterState<ZombieAttackState>();
                break;
            case EnemyState.Dead:
                stateMechine.EnterState<ZombieDeadState>();
                break;
        }
    }
}

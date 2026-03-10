using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 状态基类
/// </summary>
public abstract class StateBase 
{
    /// <summary>
    /// 状态初始化
    /// </summary>
    public abstract void Init(IStateMachineOwner owner);

    /// <summary>
    /// 进入状态
    /// </summary>
    public abstract void Enter();

    /// <summary>
    /// 退出状态
    /// </summary>
    public abstract void Exit();

    /// <summary>
    /// 销毁
    /// </summary>
    public abstract void Destroy();


    public abstract void Update();

}

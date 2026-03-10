using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IStateMachineOwner { }//状态宿主机

/// <summary>
/// 角色状态机
/// </summary>
public class StateMechine
{
    private StateBase currentState;//角色当前状态
    private IStateMachineOwner owner;
    private Dictionary<Type,StateBase> stateDic= new Dictionary<Type, StateBase>();

    public StateMechine(IStateMachineOwner owner)
    {
        this.owner = owner;
    }

    /// <summary>
    /// 进入动画状态
    /// </summary>
    /// <typeparam name="T">状态类</typeparam>
    public void EnterState<T>() where T : StateBase, new()
    {
        if(currentState != null&&currentState.GetType()==typeof(T)) return;
        if(currentState!=null)
            currentState.Exit();
        currentState = LoadState<T>();
        currentState.Enter();
    }
    /// <summary>
    /// 尝试从字典中取出状态
    /// </summary>
    /// <typeparam name="T">状态类</typeparam>
    /// <returns>状态值</returns>
    private StateBase LoadState<T>() where T : StateBase, new()
    {
        Type stateType=typeof(T);//尝试获取状态类型
        //如果状态字典没有该状态
        if( !stateDic.TryGetValue(stateType,out StateBase state))
        {
            state=new T();
            state.Init(owner);
            stateDic.Add(stateType, state);//将新创建的状态记录到字典
        }
        return state;
    }

    /// <summary>
    /// 停止状态机
    /// </summary>
    public void Stop()
    {
        if (currentState != null)
            currentState.Exit();
        foreach(var state in stateDic.Values)
        {
            state.Destroy();
        }
    }
}

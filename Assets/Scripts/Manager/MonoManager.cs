using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 任务处理管理器
/// </summary>
public class MonoManager : SingleMonoBase<MonoManager>
{

    private Action updateAction;

    /// <summary>
    /// 添加任务
    /// </summary>
    /// <param name="task">事件</param>
    public void AddUpdateAction(Action task)
    {
        updateAction += task;
    }

    public void RemoveUpdateAction(Action task)
    {
        updateAction -= task;
    }

    void Update()
    {
        updateAction?.Invoke();
    }
}

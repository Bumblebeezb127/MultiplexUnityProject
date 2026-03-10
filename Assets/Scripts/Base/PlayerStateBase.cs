using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 玩家状态基类
/// </summary>
public class PlayerStateBase : StateBase
{

    protected PlayerController playerController;
    protected PlayerModel playerModel;//当前状态的角色模型
    public override void Init(IStateMachineOwner owner)
    {
        playerController = PlayerController.INSTANCE;
        playerModel = (PlayerModel)owner;
    }
    public override void Destroy()
    {

    }

    public override void Enter()
    {
        MonoManager.INSTANCE.AddUpdateAction(Update);
    }

    public override void Exit()
    {
        MonoManager.INSTANCE.RemoveUpdateAction(Update);
    }
    public override void Update()
    {
        #region 重力计算
        if (!playerModel.characterController.isGrounded)
        {
            playerModel.verticalSpeed += playerModel.gravity * Time.deltaTime;//施加重力
            if (playerModel.IsHover())
            {
                playerModel.SwitchState(PlayerState.Hover);
            }
        }
        else
        {
            playerModel.verticalSpeed = playerModel.gravity * Time.deltaTime; ;
        }
        #endregion

        #region 瞄准状态监听
        if (IsBeControl() && (playerController.isAiming || playerController.isFire))
        {
            playerModel.SwitchState(PlayerState.Aiming);
        }

        #endregion
    }

    /// <summary>
    /// 当前的模型是否被玩家控制
    /// </summary>
    /// <returns></returns>
    public bool IsBeControl()
    {
        return playerModel == playerController.currentPlayerModel;
    }

    /// <summary>
    /// 切换到跳跃状态
    /// </summary>
    public void SwitchToHover()
    {
        //计算跳跃力度
        playerModel.verticalSpeed = Mathf.Sqrt(-2 * playerModel.gravity * playerModel.jumpHeight);
        //切换到悬空状态
        playerModel.SwitchState(PlayerState.Hover);
    }
}

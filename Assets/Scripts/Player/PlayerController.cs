using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;


/// <summary>
/// 玩家控制器
/// </summary>
public class PlayerController : SingleMonoBase<PlayerController>
{
    public PlayerModel currentPlayerModel;//当前操控的角色模型
    private Transform cameraTransform;

    [Tooltip("正常视角相机")]
    public CinemachineFreeLook freeLookCamera;
    [Tooltip("瞄准视角相机")]
    public CinemachineFreeLook aimingCamera;

    #region 玩家输入相关
    private MyInputSystem input;//输入系统
    [HideInInspector]
    public Vector2 moveInput;//移动输入
    [HideInInspector]
    public bool isSprint;
    [HideInInspector]
    public bool isJumping;
    [HideInInspector]
    public bool isAiming;
    [HideInInspector]
    public bool isFire;
    #endregion

    #region 瞄准相关
    [Tooltip("瞄准目标")]
    public Transform AimTarget;
    [Tooltip("射线检测的最大距离")]
    public float maxRayDistance = 1000f;
    [Tooltip("射线检测的层级")]
    public LayerMask aimLayerMask = ~0;
    #endregion

    #region 开火抖动
    private CinemachineImpulseSource impulseSource;
    #endregion


    [Tooltip("转向速度")]
    public float rotationSpeed = 300;

    [HideInInspector]
    public Vector3 localMovement;//本地空间下玩家的移动方向
    [HideInInspector]
    public Vector3 worldMovement;//世界空间下玩家的移动方向

    protected override void Awake()
    {
        base.Awake();
        input = new MyInputSystem();
    }
    void Start()
    {
        cameraTransform = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
        ExitAim();
        //EnterAim();
        impulseSource = aimingCamera.GetComponent<CinemachineImpulseSource>();
        ResetCameraTarget();
    }

    // Update is called once per frame
    void Update()
    {
        #region 获取玩家输入
        moveInput = input.Player.Move.ReadValue<Vector2>().normalized;
        isSprint=input.Player.IsSprint.IsPressed();
        isAiming=input.Player.IsAiming.IsPressed();
        isJumping=input.Player.IsJumping.triggered;
        isFire=input.Player.Fire.IsPressed();
        #endregion

        #region 计算玩家移动方向
        //获取相机的方向向量
        Vector3 cameraForwardProjection = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z);
        //计算世界空间下的方向向量
        worldMovement = cameraForwardProjection * moveInput.y + cameraTransform.right * moveInput.x;
        //将世界空间下的方向向量转换为模型本地空间下的方向向量
        localMovement = currentPlayerModel.transform.InverseTransformVector(worldMovement);
        #endregion

        #region 切换角色输入监听
        if (input.Player.First.triggered)
        {
            SwitchPlayerModel(1);
        }
        else if(input.Player.Second.triggered)
        {
            SwitchPlayerModel(2);
        }
        else if(input.Player.Third.triggered) 
        {
            SwitchPlayerModel(3);
        }
        #endregion
    }

    /// <summary>
    /// 切换角色
    /// </summary>
    /// <param name="index"></param>
    public void SwitchPlayerModel(int index)
    {
        if (index > GameManager.INSTANCE.playerModels.Length || GameManager.INSTANCE.playerModels[index - 1] == null) return;
        currentPlayerModel.Exit();
        currentPlayerModel=GameManager.INSTANCE.playerModels[index-1];
        currentPlayerModel.Enter();
        ResetCameraTarget();
    }

    /// <summary>
    /// 进入瞄准
    /// </summary>
    public void EnterAim()
    {
        //同步瞄准相机和自由相机的旋转角度
        aimingCamera.m_XAxis.Value = freeLookCamera.m_XAxis.Value;
        aimingCamera.m_YAxis.Value = freeLookCamera.m_YAxis.Value;

        //启动瞄准约束
        currentPlayerModel.EnterAim();


        //设置相机的优先级使瞄准相机生效
        freeLookCamera.Priority = 0;
        aimingCamera.Priority = 100;
    }
    /// <summary>
    /// 退出瞄准
    /// </summary>
    public void ExitAim()
    {
        //同步自由相机和瞄准相机的旋转角度
        freeLookCamera.m_XAxis.Value = aimingCamera.m_XAxis.Value;
        freeLookCamera.m_YAxis.Value = aimingCamera.m_YAxis.Value;

        currentPlayerModel.ExitAim();

        //设置相机的优先级使自由相机生效
        freeLookCamera.Priority = 100;
        aimingCamera.Priority = 0;
    }

    /// <summary>
    /// 重置摄像机瞄准目标
    /// </summary>
    public void ResetCameraTarget()
    {
        aimingCamera.Follow = currentPlayerModel.transform;
        aimingCamera.LookAt = currentPlayerModel.transform;
        freeLookCamera.Follow = currentPlayerModel.transform;
        freeLookCamera.LookAt = currentPlayerModel.transform;
    }

    public void ShakeCamera()
    {
        impulseSource.GenerateImpulse();
    } 

    private void OnEnable()
    {
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
}

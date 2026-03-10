using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public enum EnemyState
{
    Idle,
    Move,
    Attack,
    Dead
}
/// <summary>
/// 敌人基类
/// </summary>
public abstract class EnemyBase : MonoBehaviour,IStateMachineOwner
{

    [HideInInspector]
    public Animator animator;
    protected StateMechine stateMechine;

    #region 寻路相关
    [HideInInspector]
    public NavMeshAgent navMeshAgent;//寻路代理
    [Tooltip("转向速度")]
    public float rotationSpeed = 300f;
    [Tooltip("最小攻击距离")]
    public float minAttackDistance = 1f;
    [HideInInspector]
    public PlayerModel attackTarget;
    #endregion

    #region 流血相关预制体
    [Tooltip("喷血溅射特效")]
    public GameObject bloodSmashPrefab;
    [Tooltip("滴血特效")]
    public GameObject bloodDrippingPrefab;
    #endregion

    #region 受击相关
    protected int hitHash;
    protected int moveSpeedHash;
    protected float normalMoveSpeed = 1;
    protected float slowMoveSpeed = 0.5f;
    protected Coroutine recoverSpeedCoroutine;//恢复速度的协程
    #endregion

    #region 血条相关
    [Tooltip("生命值")]
    public float health = 100f;
    private float currentHealth;
    private bool isDead;
    [Tooltip("血条预制体")]
    public GameObject healthBarPrafab;
    [Tooltip("血条的位置")]
    public Transform healthBarPos;
    [HideInInspector]
    public GameObject healthBar;

    [Tooltip("血条框显示时间")]
    public float healthBarShowTime = 6f;
    private float healthBarShowTimer;
    #endregion

    private void Awake()
    {
        stateMechine = new StateMechine(this);
        animator=GetComponent<Animator>();
        navMeshAgent=GetComponent<NavMeshAgent>();
        navMeshAgent.stoppingDistance = minAttackDistance;
        navMeshAgent.angularSpeed = rotationSpeed;
        hitHash = Animator.StringToHash("Hit");
        moveSpeedHash = Animator.StringToHash("MoveSpeed");
        currentHealth = health;
        isDead = false;
        healthBarShowTimer = healthBarShowTime;
    }

    private void Start()
    {
        SwitchState(EnemyState.Idle);
        FindAttackTarget();
        #region 实例化血条框
        healthBar = Instantiate(healthBarPrafab, healthBarPos.position, Quaternion.identity);
        healthBar.transform.SetParent(UIManager.INSTANCE.worldSpaceCanvas.transform);
        #endregion
    }

    protected virtual void Update()
    {
        if (isDead) return;

        #region 血条相关
        if (healthBarShowTimer < healthBarShowTime) 
        {
            healthBar.SetActive(true);
            healthBar.transform.position = healthBarPos.transform.position;
            healthBarShowTimer += Time.deltaTime;
        }
        else
        {
            healthBar.SetActive(false);
        }
        #endregion
    }

    /// <summary>
    /// 寻找离自身最近的PlayerModel
    /// </summary>
    public virtual void FindAttackTarget()
    {
        PlayerModel[] playerModels = GameManager.INSTANCE.playerModels;
        if(playerModels != null && playerModels.Length > 0)
        {
            PlayerModel closestPlayer = null;
            float minDistance = float.MaxValue;
            foreach(PlayerModel playerModel in playerModels)
            {
                if (playerModel != null)
                {
                    float distance = Vector3.Distance(transform.position, playerModel.transform.position);
                    if(distance < minDistance)
                    {
                        minDistance = distance;
                        closestPlayer = playerModel;
                    }
                }
            }
            attackTarget = closestPlayer;
        }
    }

    /// <summary>
    /// 减慢移动动画播放速度
    /// </summary>
    protected virtual void SlowMoveAnimation()
    {
        animator.SetFloat(moveSpeedHash, slowMoveSpeed);
        if (recoverSpeedCoroutine != null)
        {
            StopCoroutine(recoverSpeedCoroutine);
        }
        recoverSpeedCoroutine = StartCoroutine(RecoverMoveSpeed(0.5f));
    }

    /// <summary>
    /// 恢复速度
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    protected IEnumerator RecoverMoveSpeed(float delay)
    {
        //等待指定时间
        yield return new WaitForSeconds(delay);
        //恢复正常移动速度
        animator.SetFloat(moveSpeedHash, normalMoveSpeed);
        recoverSpeedCoroutine = null;
    }

    /// <summary>
    /// 受击
    /// </summary>
    /// <param name="bullet"></param>
    /// <param name="damageMultiplier"></param>
    public virtual void Hurt(PlayerWeaponBullet bullet,float damageMultiplier = 1f)
    {
        #region 受击动画相关
        animator.SetTrigger(hitHash);
        SlowMoveAnimation();
        #endregion

        #region 生成喷血特效
        //计算子弹方向
        Vector3 bulletDir = bullet.transform.forward;
        //根据子弹方向计算旋转
        Quaternion rotation = Quaternion.LookRotation(-bulletDir);
        //生成喷血特效
        Destroy(Instantiate(bloodSmashPrefab, bullet.transform.position,rotation),3);
        #endregion

        #region 生成流血滴落特效
        Destroy(Instantiate(bloodDrippingPrefab, bullet.transform.position + Vector3.up * 0.1f, Quaternion.identity), 3);
        #endregion

        #region 血条相关
        currentHealth -= bullet.damage * damageMultiplier;
        if (currentHealth > 0)
        {
            currentHealth = currentHealth >= 0 ? currentHealth : 0;
            healthBarShowTimer = 0.0f;
            healthBar.GetComponent<EnemyHealthBarUI>().UpdateHealthBar(currentHealth / health);
        }
        else
        {
            SwitchState(EnemyState.Dead);
            navMeshAgent.enabled = false;
            GetComponent<BoxCollider>().enabled = false;
            isDead = true;
            Destroy(healthBar);
            healthBar = null;
        }

        #endregion
    }


    /// <summary>
    /// 是否存在攻击目标
    /// </summary>
    /// <returns></returns>
    public virtual bool HasAttackTarget()
    {
        return attackTarget != null;
    }

    public virtual bool IsAttackTargetInAttackRange()
    {
        if (HasAttackTarget())
        {
            return Vector3.Distance(transform.position, attackTarget.transform.position) < minAttackDistance;

        }
        return false;
    }

    /// <summary>
    /// 追击目标
    /// </summary>
   public virtual void ChaseTarget()
    {
        if (HasAttackTarget())
        {
            navMeshAgent.SetDestination(attackTarget.transform.position);
        }
    }

    /// <summary>
    /// 切换状态
    /// </summary>
    /// <param name="state"></param>
    public abstract void SwitchState(EnemyState state);

    /// <summary>
    /// 播放动画
    /// </summary>
    /// <param name="animationName">动画名称</param>
    /// <param name="transition">过渡时间</param>
    /// <param name="layer">动画层</param>
    public void PlayStateAnimation(string animationName, float transition = 0.25f, int layer = 0)
    {
        animator.CrossFadeInFixedTime(animationName, transition, layer);
    }

    /// <summary>
    /// 销毁敌人
    /// </summary>
    public void Clear()
    {
        stateMechine.Stop();
        Destroy(gameObject);
    }

}

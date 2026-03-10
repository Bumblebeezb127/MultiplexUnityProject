using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//玩家子弹
public class PlayerWeaponBullet : MonoBehaviour
{
    [Tooltip("伤害")]
    public int damage = 10;
    [HideInInspector]
    public Rigidbody rb;
    [Tooltip("推力")]
    public float flyPower = 30f;
    [Tooltip("存活时间")]
    public float lifetime = 10f;

    private Vector3 prevPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rb.velocity = transform.forward * flyPower;//给子弹一个推力
        Destroy(gameObject, lifetime);
        prevPosition = transform.position;
        CheckInitalOverlap();
    }
    private void Update()
    {
        CheckCollision();
        prevPosition = transform.position;
    }

    /// <summary>
    /// 检查子弹是否生成在敌人碰撞体内部
    /// </summary>
    void CheckInitalOverlap()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.1f);
        foreach (Collider hitcollider in hitColliders)
        {
            EnemyBase enemy=hitcollider.GetComponent<EnemyBase>();
            if(enemy != null)
            {
                enemy.Hurt(this, 1);
                Destroy(this.gameObject);
                return;
            }
        }
    }

    void CheckCollision()
    {
        RaycastHit hit;
        Vector3 dir = transform.forward;///
        float distance = Vector3.Distance(transform.position, prevPosition);

        //绘制线段检测碰撞
        if (Physics.Raycast(prevPosition, dir.normalized, out hit, distance))
        {
            //是否为敌人
            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyBase enemy=hit.collider.GetComponent<EnemyBase>();
                enemy.Hurt(this, 1);
            }
        }
    }

}

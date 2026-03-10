using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敌人血条
/// </summary>
public class EnemyHealthBarUI : MonoBehaviour
{
    [Tooltip("血条填充块")]
    public Image healthSlider;

    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        healthSlider.fillAmount = 1f;//填满血条
    }
    private void Update()
    {
        //计算UI到摄像机的方向
        Vector3 dir = -cameraTransform.forward.normalized;//
        //让血条面向摄像机
        transform.rotation = Quaternion.LookRotation(dir);
    }

    public void UpdateHealthBar(float healthRatio)
    {
        healthSlider.fillAmount = healthRatio;
    }
}

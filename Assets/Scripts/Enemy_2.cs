using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_2 : Enemy
{
    // 使用正弦波形修正两点差值
    public Vector3[] points;
    public float birthTime;
    public float lifeTime = 10;

    // 正弦波对运动的影响程度
    public float sinEccentricity = 0.6f;

    private void Start()
    {
        // 声明一个存了两个点的数组
        points = new Vector3[2];
        // 获取屏幕左下角和右上角的点
        Vector3 cbMin = Utils.camBounds.min;
        Vector3 cbMax = Utils.camBounds.max;
        // 声明一个向量，初始值是零向量
        Vector3 v = Vector3.zero;

        // 在屏幕左侧随机取一个点
        // 用左下角的点的横坐标 减去 敌人距离屏幕的内边距，作为x坐标
        v.x = cbMin.x - Main.S.enemySpawnPadding;
        // 随机取一个y值
        v.y = Random.Range(cbMin.y, cbMax.y);
        points[0] = v; // Vector3是值类型变量

        // 在屏幕右侧也随机取一个点
        v = Vector3.zero;
        v.x = cbMax.x + Main.S.enemySpawnPadding;
        v.y = Random.Range(cbMin.y, cbMax.y);
        points[1] = v;

        // 50%的概率换边
        if (Random.value < 0.5f)
        {
            points[0].x *= -1;
            points[1].x *= -1;
        }

        birthTime = Time.time;
    }

    public override void Move()
    {
        float u = (Time.time - birthTime) / lifeTime;

        // u>1说明存活时间超过了生命周期
        if (u > 1)
        {
            Destroy(this.gameObject);
            return;
        }

        u = u + sinEccentricity * Mathf.Sin(u * Mathf.PI * 2);

        pos = (1 - u) * points[0] + u * points[1];
    }
}

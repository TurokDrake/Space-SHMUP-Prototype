using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 10f;
    public float fireRate = 0.3f; // 发射频率
    public float health = 10;
    public int score = 100;
    public int showDamageForFrames = 2; // 显示伤害效果的帧数
    public float powerUpDropChance = 1f; // 掉落升级道具的概率

    public bool _____;

    public Color[] originalColors; // 原本的颜色
    public Material[] materials; // 本对象及子对象的所有材质
    public int remainingDamageFrames = 0; // 剩余的伤害效果帧数
    public Bounds bounds;
    public Vector3 boundsCenterOffset; // 边界中心到该对象的位置的距离


    private void Awake()
    {
        materials = Utils.GetAllMaterials(gameObject);
        originalColors = new Color[materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            originalColors[i] = materials[i].color; //记录原本的颜色，以便后续恢复
        }
        // 找到已经不在屏幕中的敌人
        InvokeRepeating("CheckOffScreen", 0f, 2f); // 在0s后开始（即立刻开始）执行名为"CheckOffScreen"的方法，每隔2s后再执行一次
    }
    private void Update()
    {
        Move();
        if (remainingDamageFrames > 0)
        {
            remainingDamageFrames--;
            if (remainingDamageFrames == 0)
            {
                UnShowDamage();
            }
        }
    }


    public virtual void Move()
    {
        Vector3 tempPos = pos;
        // 在竖直方向上向玩家方向靠近
        tempPos.y -= speed * Time.deltaTime;
        pos = tempPos;
    }

    
    public Vector3 pos
    {
        get { return this.transform.position; }
        set { this.transform.position = value; }
    }

    void CheckOffScreen()
    {
        if (bounds.size == Vector3.zero)
        {
            bounds = Utils.CombineBoundsOfChildren(this.gameObject);
            boundsCenterOffset = bounds.center - transform.position;
        }

        bounds.center = transform.position + boundsCenterOffset;

        Vector3 off = Utils.ScreenBoundsCheck(bounds, BoundsTest.offScreen);

        if (off != Vector3.zero)
        {
            if (off.y < 0)
            {
                Destroy(this.gameObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        switch (other.tag)
        {
            case "ProjectileHero":
                Projectile p = other.GetComponent<Projectile>();

                bounds.center = transform.position + boundsCenterOffset;

                // 若敌人当前不在屏幕内，却被击中，就移除掉子弹
                if (bounds.extents == Vector3.zero || Utils.ScreenBoundsCheck(bounds, BoundsTest.offScreen)!=Vector3.zero)
                {
                    Destroy(other);
                    break;
                }

                ShowDamage();

                health -= Main.W_DEFS[p.type].damageOnHit;
                if (health <= 0)
                {
                    Main.S.ShipDestroyed(this);
                    Destroy(this.gameObject);
                }
                Destroy(other);
                break;
        }
    }

    /// <summary>
    /// 展示受击时的效果
    /// </summary>
    void ShowDamage()
    {
        foreach (Material m in materials)
        {
            m.color = Color.red;
        }
        remainingDamageFrames = showDamageForFrames;
    }
    
    /// <summary>
    /// 恢复本色
    /// </summary>
    void UnShowDamage()
    {
        for(int i = 0; i < materials.Length; i++)
        {
            materials[i].color = originalColors[i];
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Part
{
    public string name; // 组件名称
    public float health; // 组件生命值
    public string[] protectedBy; // 受谁保护

    public GameObject go; // 该组件的游戏对象
    public Material mat; // 显示伤害的材质
}

public class Enemy_4 : Enemy
{
    public Vector3[] points; // 存储插值的两个点
    public float timeStart; // 出生时间
    public float duration = 4; // Enemy_4每段运动的时间长度

    public Part[] parts; // 存储敌机各部件的数组

    private void Start()
    {
        points = new Vector3[2];
        points[0] = pos;
        points[1] = pos;

        InitMovement();

        // 找到所有部分的游戏对象引用和材质引用
        Transform t;
        foreach (Part prt in parts)
        {
            t = transform.Find(prt.name);
            if (t != null)
            {
                prt.go = t.gameObject;
                prt.mat = prt.go.GetComponent<Renderer>().material;
            }
        }
    }

    void InitMovement()
    {
        Vector3 p1 = Vector3.zero;
        float esp = Main.S.enemySpawnPadding;
        Bounds cBounds = Utils.camBounds;
        p1.x = Random.Range(cBounds.min.x + esp, cBounds.max.x - esp);
        p1.y = Random.Range(cBounds.min.y + esp, cBounds.max.y - esp);

        points[0] = points[1];
        points[1] = p1;

        timeStart = Time.time;
    }

    public override void Move()
    {
        float u = (Time.time - timeStart) / duration;
        if (u >= 1)
        {
            InitMovement();
            u = 0;
        }

        u = 1 - Mathf.Pow(1 - u, 2); // 慢速结束的平滑过渡

        pos = (1 - u) * points[0] + u * points[1];
    }

    void OnCollisionEnter(Collision coll)
    {
        GameObject other = coll.gameObject;
        switch (other.tag)
        {
            case "ProjectileHero":
                // 获取子弹对象的组件
                Projectile p = other.GetComponent<Projectile>();

                // 进入屏幕前不会受伤
                bounds.center = transform.position + boundsCenterOffset;
                if (bounds.extents == Vector3.zero || Utils.ScreenBoundsCheck(bounds, BoundsTest.offScreen) != Vector3.zero)
                {
                    Destroy(other);
                    break;
                }

                // Contacts[]是一个由碰撞点组成的数组
                GameObject goHit = coll.GetContact(0).thisCollider.gameObject;
                Part prtHit = FindPart(goHit);
                if (prtHit == null) // 如果没找到prtHit，是因为通常thisCollider.gameObject不是敌机的组件，而是ProjectileHero

                {
                    // 这时可以查看参与碰撞的另一个碰撞器
                    goHit = coll.contacts[0].otherCollider.gameObject;
                    prtHit = FindPart(goHit);
                }

                // 检查这个组件是否受保护
                if (prtHit.protectedBy != null)
                {

                    foreach (string s in prtHit.protectedBy)
                    {
                        // 如果尚未销毁，则暂时不对该组件造成伤害
                        if (!Destroyed(s))
                        {
                            // 消除炮弹并在造成伤害前返回
                            Destroy(other);
                            return;
                        }
                    }
                }
                // 若其未被保护，则受到伤害
                prtHit.health -= Main.W_DEFS[p.type].damageOnHit;

                ShowLocalizedDamage(prtHit.mat);

                if (prtHit.health <= 0)
                {
                    prtHit.go.SetActive(false);
                }

                bool allDestroyed = true;
                foreach(Part prt in parts)
                {
                    // 只要有一个组件存在，就标识为未完全消灭
                    if (!Destroyed(prt))
                    {
                        allDestroyed = false;
                        break;
                    }
                }
                // 如果完全消灭，就销毁该对象
                if (allDestroyed)
                {
                    Main.S.ShipDestroyed(this);

                    Destroy(this.gameObject);
                }

                // 记得消灭炮弹
                Destroy(other);

                break;
        }
    }

    /// <summary>
    /// 根据名称寻找部件
    /// </summary>
    /// <param name="n">部件的游戏对象名称</param>
    /// <returns></returns>
    Part FindPart(string n)
    {
        foreach (Part prt in parts)
        {
            if (prt.name == n)
            {
                return prt;
            }
        }
        return null;
    }

    /// <summary>
    /// 根据游戏对象寻找部件
    /// </summary>
    /// <param name="go">游戏对象</param>
    /// <returns></returns>
    Part FindPart(GameObject go)
    {
        foreach (Part prt in parts)
        {
            if (prt.go == go)
            {
                return prt;
            }
        }
        return null;
    }

    bool Destroyed(GameObject go)
    {
        return Destroyed(FindPart(go));
    }

    bool Destroyed(string n)
    {
        return Destroyed(FindPart(n));
    }

    bool Destroyed(Part prt)
    {
        // 传入的参数不是真正的组件，说明已经被销毁
        if (prt == null)
        {
            return true;
        }

        // 若组件的生命值小于等于0，说明已经被销毁
        return prt.health <= 0;
    }

    /// <summary>
    /// 改变组件的颜色，而非整架飞机的颜色
    /// </summary>
    /// <param name="m">组件的材质</param>
    void ShowLocalizedDamage(Material m)
    {
        m.color = Color.red;
        remainingDamageFrames = showDamageForFrames;
    }
}

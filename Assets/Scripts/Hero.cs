using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    public static Hero S; // 单例
    public float gameRestartDelay = 2f;

    public float speed = 30;
    public float rollMult = -45;
    public float pitchMult = 30;

    public Bounds bounds;
    public delegate void WeaponFireDelegate();
    public WeaponFireDelegate fireDelegate;

    // 飞船盾牌状态信息
    //public float shieldLevel = 1; 
    [SerializeField]
    private float _shieldLevel = 1;
    public float shieldLevel
    {
        get
        {
            return _shieldLevel;
        }
        set
        {
            // 确保状态等级在4以内
            _shieldLevel = Mathf.Min(value, 4);
            // 盾消失后再被击中就死亡
            if (value < 0)
            {
                Destroy(this.gameObject);
                // 角色死亡后，要重新开始游戏
                Main.S.DelayedRestart(gameRestartDelay);
            }
        }
    }

    public Weapon[] weapons;
    public bool _____;

    // 存储最后一次触发护盾碰撞器的对象，防止同一父级的几个游戏对象连续触发护盾
    public GameObject lastTriggerGO = null;

    private void Awake()
    {
        S = this;
        bounds = Utils.CombineBoundsOfChildren(this.gameObject);

        
    }

    private void Start()
    {
        // 重置武器为爆弹
        ClearWeapons();
        weapons[0].SetType(WeaponType.blaster);
    }

    private void Update()
    {
        // 获取用户输入
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");

        Vector3 pos = transform.position;
        pos.x += xAxis * speed * Time.deltaTime;
        pos.y += yAxis * speed * Time.deltaTime;
        transform.position = pos;

        bounds.center = transform.position;

        // off实际上是表示玩家的bounds超出摄像机的bounds的大小和方向的向量
        // center，表示玩家的中心可以到屏幕边缘，这意味着玩家移动到屏幕边缘时，驾驶舱可能会显示一半
        // onScreen，表示玩家的全部部分都在屏幕中显示
        // offScreen，表示玩家可以在屏幕外
        Vector3 off = Utils.ScreenBoundsCheck(bounds, BoundsTest.center);

        // 限制玩家在屏幕中
        if (off != Vector3.zero)
        {
            pos -= off;
            transform.position = pos;
        }

        // 使飞船绕x、y轴旋转，让其更有动感
        transform.rotation = Quaternion.Euler(yAxis * pitchMult, xAxis * rollMult, 0);

        // 若按下了“跳跃”按钮，就进行射击，射击的逻辑都装在这个委托中；这里其实“跳跃”的按键是空格
        if (Input.GetAxis("Jump")==1 && fireDelegate != null)
        {
            fireDelegate();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject go = Utils.FindTaggedParent(other.gameObject);

        if (go != null)
        {
            // 若碰到的对象是上次碰到的，那么判定为连续碰撞到同一对象的其他子对象，所以返回
            if (go == lastTriggerGO)
            {
                return;
            }
            // 否之记录下当前对象
            lastTriggerGO = go;

            // 若对象是敌人，则消灭敌人，同时英雄会扣除盾的等级
            if (go.tag == "Enemy")
            {
                shieldLevel--;
                Destroy(go);
            }
            // 若对象是升级道具，执行吸收升级道具的逻辑
            else if (go.tag == "PowerUp")
            {
                AbsorbPowerUp(go);
            }
            else
            {
                print("触发碰撞事件：" + go.name);
            }
        }
        else
        {
            print("触发碰撞事件：" + other.gameObject.name);
        }


    }

    /// <summary>
    /// 吸收升级道具
    /// </summary>
    /// <param name="go">升级道具的游戏对象</param>
    public void AbsorbPowerUp(GameObject go)
    {
        // 获取其PowerUp组件
        PowerUp pu = go.GetComponent<PowerUp>();
        // 判断PowerUp组件的类型
        switch (pu.type)
        {
            // 如果是盾牌，就升级盾牌
            case WeaponType.shield:
                shieldLevel++;
                break;
            // 若是其他武器
            default:
                // 如果升级道具的武器类型和英雄的武器类型相同
                if (pu.type == weapons[0].type)
                {
                    // 找到一个空武器槽
                    Weapon w = GetEmptyWeaponSlot();
                    if (w != null)
                    {
                        w.SetType(pu.type);
                    }
                }
                else
                {
                    // 武器类型不一样就切换武器
                    ClearWeapons();
                    weapons[0].SetType(pu.type);
                }
                break;
        }

        pu.AbsorbedBy(this.gameObject);
    }

    /// <summary>
    /// 获得空武器槽
    /// </summary>
    /// <returns>武器</returns>
    Weapon GetEmptyWeaponSlot()
    {
        for(int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].type == WeaponType.none)
            {
                return (weapons[i]);
            }
        }
        return null;
    }

    /// <summary>
    /// 清除所有武器，把武器都设为WeaponType.none
    /// </summary>
    void ClearWeapons()
    {
        foreach (Weapon w in weapons)
        {
            w.SetType(WeaponType.none);
        }
    }
}

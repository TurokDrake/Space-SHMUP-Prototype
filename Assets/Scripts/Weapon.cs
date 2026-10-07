using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 定义了武器类型
public enum WeaponType
{
    none, // 默认/无武器
    blaster, // 简单的高爆弹武器
    spread, // 同时发射两发炮弹
    phaser, // 沿波形前进（未实现）
    missile, // 制导导弹（未实现）
    laser, // 按时间造成伤害（未实现）
    shield // 提高护盾等级
}

[System.Serializable]
// 可以在检视面板种设置特定武器的属性
public class WeaponDefinition
{
    public WeaponType type = WeaponType.none;
    public string letter;
    public Color color = Color.white;
    public GameObject projectilePrefab;
    public Color projectileColor = Color.white;
    public float damageOnHit = 0;
    public float continuousDamage = 0;
    public float delayBetweenShots = 0;
    public float velocity = 20;
}

public class Weapon : MonoBehaviour
{
    public static Transform PROJECTILE_ANCHOR;

    public bool _____;

    [SerializeField]
    private WeaponType _type = WeaponType.blaster;
    public WeaponDefinition def;
    public GameObject collar;
    public float lastShot; // 上一次射击的时间

    private void Awake()
    {
        collar = transform.Find("Collar").gameObject;
    }
    private void Start()
    {
        SetType(_type);
        if (PROJECTILE_ANCHOR == null)
        {
            GameObject go = new GameObject("_Projectile_Anchor");
            PROJECTILE_ANCHOR = go.transform;
        }

        GameObject parentGO = transform.parent.gameObject;
        if (parentGO.tag == "Hero")
        {
            Hero.S.fireDelegate += Fire;
        }
    }

    public WeaponType type
    {
        get { return _type; }
        set { SetType(value); }
    }

    public void SetType(WeaponType wt)
    {
        _type = wt;
        if (type == WeaponType.none)
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }
        def = Main.GetWeaponDefinition(_type);
        collar.GetComponent<Renderer>().material.color = def.color;
        lastShot = 0; // 只要设置了武器类型，就能立刻射击
    }

    public void Fire()
    {
        // 若武器未激活，就不能射击
        if (!gameObject.activeInHierarchy)
            return;
        // 距离上次发射时间间隔小于最小间隔，也不能射击
        if (Time.time - lastShot < def.delayBetweenShots)
        {
            return;
        }
        Projectile p;
        switch (type)
        {
            case WeaponType.none:
                break;
            case WeaponType.blaster:
                p = MakeProjectile();
                p.GetComponent<Rigidbody>().velocity = Vector3.up * def.velocity;
                break;
            case WeaponType.spread:
                p = MakeProjectile();
                p.GetComponent<Rigidbody>().velocity = Vector3.up * def.velocity;
                p = MakeProjectile();
                p.GetComponent<Rigidbody>().velocity = new Vector3(-0.2f, 0.9f, 0) * def.velocity;
                p = MakeProjectile();
                p.GetComponent<Rigidbody>().velocity = new Vector3(0.2f, 0.9f, 0) * def.velocity;
                break;
            case WeaponType.phaser:
                break;
            case WeaponType.missile:
                break;
            case WeaponType.laser:
                break;
            case WeaponType.shield:
                break;
            default:
                break;
        }

    }

    public Projectile MakeProjectile()
    {
        GameObject go = Instantiate<GameObject>(def.projectilePrefab);
        if (transform.parent.gameObject.tag == "Hero")
        {
            go.tag = "ProjectileHero";
            go.layer = LayerMask.NameToLayer("ProjectileHero");
        }
        else
        {
            go.tag = "ProjectileEnemy";
            go.layer = LayerMask.NameToLayer("ProjectileEnemy");
        }
        go.transform.position = collar.transform.position;
        go.transform.parent = PROJECTILE_ANCHOR;
        Projectile p = go.GetComponent<Projectile>();
        p.type = type;
        lastShot = Time.time;
        return p;
    }
}

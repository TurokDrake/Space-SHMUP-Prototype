using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private WeaponType _type;

    public WeaponType type
    {
        get
        {
            return _type;
        }
        set
        {
            SetType(value);
        }
    }

    private void Awake()
    {
        // 检测子弹是不是飞出屏幕了
        InvokeRepeating("CheckOffScreen", 2f, 2f);
    }

    // 设置炮弹的颜色
    public void SetType(WeaponType eType)
    {
        _type = eType;
        WeaponDefinition def = Main.GetWeaponDefinition(_type);
        GetComponent<Renderer>().material.color = def.projectileColor;
    }

    // 检查炮弹是不是飞出屏幕了
    void CheckOffScreen()
    {
        // Utils.ScreenBoundsCheck()只有在当前对象的碰撞体边界框在相机边界框内时，才会返回Vector3.zero
        // 反之，如果没有返回Vector3.zero，说明对象的边界框在相机边界框外，即子弹飞出去了
        if (Utils.ScreenBoundsCheck(GetComponent<Collider>().bounds, BoundsTest.offScreen) != Vector3.zero)
        {
            Destroy(this.gameObject);
        }
    }
}


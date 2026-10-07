using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    // 使用二维向量存储随机数的最大值和最小值
    public Vector2 rotMinMax = new Vector2(15, 90);
    public Vector2 driftMinMax = new Vector2(0.25F, 2);
    public float lifeTime = 6f; // 升级道具完全存在的时间长度（即渐隐前的时长）
    public float fadeTime = 4f; // 升级道具逐渐隐藏所需要的时间

    public bool _____;

    public WeaponType type; // 武器类型
    public GameObject cube; // 子对象的引用
    public TextMesh letter; // TextMesh组件的引用
    public Vector3 rotPerSecond; // 欧拉旋转的速度
    public float birthTime;

    private void Awake()
    {
        cube = transform.Find("Cube").gameObject;
        letter = GetComponent<TextMesh>();

        Vector3 vel = Random.onUnitSphere; // 返回在半径为1的球体表面上的一个点
        vel.z = 0; // 使速度在xy平面上
        vel.Normalize(); // 归一化
        vel *= Random.Range(driftMinMax.x, driftMinMax.y);
        GetComponent<Rigidbody>().velocity = vel;

        transform.rotation = Quaternion.identity;
        rotPerSecond = new Vector3(Random.Range(rotMinMax.x, rotMinMax.y), Random.Range(rotMinMax.x, rotMinMax.y), Random.Range(rotMinMax.x, rotMinMax.y));
        InvokeRepeating("CheckOffScreen", 2f, 2f);
        birthTime = Time.time;
    }

    private void Update()
    {
        cube.transform.rotation = Quaternion.Euler(rotPerSecond * Time.time);

        float u=(Time.time - birthTime)/(lifeTime+fadeTime);
       

        // 隔一定时间后，升级道具渐隐
        // 如果生成的时间大于10s，删除
        if (u >= 1)
        {
            Destroy(this.gameObject);
            return;
        }

        // 确定立方体和文字的不透明度
        if (u > 0)
        {
            Color c = cube.GetComponent<Renderer>().material.color;
            c.a = 1f - u;
            cube.GetComponent<Renderer>().material.color = c;

            c = letter.color;
            c.a = 1f - (u * 0.5f);
            letter.color = c;
        }
    }

    public void SetType(WeaponType wt)
    {
        WeaponDefinition def = Main.GetWeaponDefinition(wt);
        cube.GetComponent<Renderer>().material.color = def.color;
        letter.text = def.letter;
        type = wt;
    }

    /// <summary>
    /// 被吸收时
    /// </summary>
    /// <param name="target"></param>
    public void AbsorbedBy(GameObject target)
    {
        Destroy(this.gameObject);
    }

    void CheckOffScreen()
    {
        if (Utils.ScreenBoundsCheck(cube.GetComponent<Collider>().bounds, BoundsTest.offScreen) != Vector3.zero)
        {
            Destroy(this.gameObject);
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    public float rotationsPerSecond = 0.1f;
    public bool _____;
    public int levelShown = 0;
    public Renderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<Renderer>();
    }
    private void Update()
    {
        // 返回小于等于 Hero.S.shieldLevel 的最大整数（即向下取整）作为当前纹理等级
        int currLevel = Mathf.FloorToInt(Hero.S.shieldLevel);

        // 若显示的等级与当前纹理等级不符合，修正显示的等级
        if (levelShown != currLevel)
        {
            levelShown = currLevel;
            Material mat = meshRenderer.material;

            // mainTextureOffset 是纹理偏移量
            // 之所以数字是0.2，是因为shield纹理分了0，0.2，0.4，0.6，0.8几个效果
            // 可以在检视器中尝试修改Mat_Shield 的offset.x，你可以发现，，当设置为0.5偏移量时，显示的纹理不连续
            mat.mainTextureOffset = new Vector2(0.2f * levelShown, 0);
        }

        // 让纹理绕z轴旋转一定角度
        float rZ = (rotationsPerSecond * Time.deltaTime * 360) % 360f;
        transform.rotation = Quaternion.Euler(0, 0, rZ);
    }
}

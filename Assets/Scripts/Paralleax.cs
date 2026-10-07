using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paralleax : MonoBehaviour
{
    public GameObject poi; // 主角飞船
    public GameObject[] panels; // 滚动的前景画面
    public float scrollSpeed = -30f;

    public float motionMult = 0.25f; // 控制前景画面对玩家运动的反馈程度

    private float panelHt; // 每个前景画面的高度
    private float depth; // 前景画面的深度（即z轴的距离）
    // Start is called before the first frame update
    void Start()
    {
        panelHt = panels[0].transform.localScale.y;
        depth = panels[0].transform.position.z;

        panels[0].transform.position = new Vector3(0, 0, depth);
        panels[1].transform.position = new Vector3(0, panelHt, depth);
    }

    // Update is called once per frame
    void Update()
    {
        float tY, tX = 0;
        tY = Time.time * scrollSpeed % panelHt + (panelHt * 0.5f);
        if (poi != null)
        {
            tX = -poi.transform.position.x * motionMult;
        }

        panels[0].transform.position = new Vector3(tX, tY, depth);

        if (tY >= 0)
        {
            panels[1].transform.position = new Vector3(tX, tY - panelHt, depth);
        }
        else
        {
            panels[1].transform.position = new Vector3(tX, tY + panelHt, depth);
        }
    }
}

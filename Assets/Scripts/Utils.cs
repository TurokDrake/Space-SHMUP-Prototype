using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BoundsTest
{
    center, // 游戏对象的中心是否位于屏幕中
    onScreen, // 游戏对象是否完全位于屏幕中
    offScreen // 游戏对象是否完全位于屏幕之外
}

public class Utils : MonoBehaviour
{
    #region Bounds函数


    /// <summary>
    /// 接收两个bounds，返回包含这两个bounds的新bounds
    /// </summary>
    /// <param name="b0">0号bounds</param>
    /// <param name="b1">1号bounds</param>
    /// <returns></returns>
    public static Bounds BoundsUnion(Bounds b0, Bounds b1)
    {
        // 返回范围较大的包围盒
        if (b0.size == Vector3.zero && b1.size != Vector3.zero)
        {
            return b1;
        }
        else if (b0.size != Vector3.zero && b1.size == Vector3.zero)
        {
            return b0;
        }
        else if (b0.size == Vector3.zero && b1.size == Vector3.zero)
        {
            return b0;
        }

        // 若两包围盒的大小均不为零向量
        // 就让b0扩展到b1，这样一来，b0一定包含b1，返回出去的一定是最大的包围盒
        b0.Encapsulate(b1.min);
        b0.Encapsulate(b1.max);
        return b0;
    }

    /// <summary>
    /// 合并所有子对象的bounds并返回一个新bounds
    /// </summary>
    /// <param name="go">指定的对象</param>
    /// <returns></returns>
    public static Bounds CombineBoundsOfChildren(GameObject go)
    {
        Bounds b = new Bounds(Vector3.zero, Vector3.zero);
        // 先获取其身上挂载的renderer与collider组件的bounds
        if (go.GetComponent<Renderer>() != null)
        {
            b = BoundsUnion(b, go.GetComponent<Renderer>().bounds);
        }
        if (go.GetComponent<Collider>() != null)
        {
            b = BoundsUnion(b, go.GetComponent<Collider>().bounds);
        }

        // 再遍历其子对象，由于Transform类实现了枚举器，可以使用foreach遍历
        foreach (Transform t in go.transform)
        {
            // 层层递归，确保子对象的子对象也能包括进来
            b = BoundsUnion(b, CombineBoundsOfChildren(t.gameObject));
        }
        return b;
    }

    #region 设置摄像机的bounds
    private static Bounds _camBounds; // 摄像机的bounds

    public static Bounds camBounds
    {
        get
        {
            if (_camBounds.size == Vector3.zero)
            {
                SetCameraBounds();
            }
            return _camBounds;
        }
    }
    public static void SetCameraBounds(Camera cam = null)
    {
        // 若未传入摄像机，则使用主摄像机
        if (cam == null)
        {
            cam = Camera.main;
        }


        // 摄像机需要为正投影摄像机
        // 摄像机的旋转为[0,0,0]

        // 根据屏幕左上角和右下角坐标创建两个三维向量
        Vector3 topLeft = new Vector3(0, 0, 0);
        Vector3 bottomRight = new Vector3(Screen.width, Screen.height, 0);

        // 转换成世界坐标
        Vector3 boundTLN = cam.ScreenToWorldPoint(topLeft);
        Vector3 boundBRF = cam.ScreenToWorldPoint(bottomRight);

        // 将两个向量的z坐标分别设置为相对摄像机渲染的最近位置和最远位置
        boundTLN.z += cam.nearClipPlane;
        boundBRF.z += cam.farClipPlane;

        // 确定边界框中心
        Vector3 center = (boundTLN + boundBRF) / 2f;
        _camBounds = new Bounds(center, Vector3.zero);

        // 扩展_camBounds
        _camBounds.Encapsulate(boundTLN);
        _camBounds.Encapsulate(boundBRF);
    }
    #endregion

    public static Vector3 ScreenBoundsCheck(Bounds bnd,BoundsTest test = BoundsTest.center)
    {
        return BoundsInBoundsCheck(camBounds, bnd, test);
    }

    public static Vector3 BoundsInBoundsCheck(Bounds bigB,Bounds lilB,BoundsTest test = BoundsTest.onScreen)
    {
        // 获取lilB的中心
        Vector3 pos = lilB.center;

        // 要平移大小和方向
        Vector3 off = Vector3.zero;

        switch (test)
        {
            // 当test为center时，需要把lilB的中心平移到bigB之内
            // off就是最终需要平移的方向和距离
            case BoundsTest.center:
                // 若lilB已经在bigB中，就无需移动lilB的中心
                if (bigB.Contains(pos))
                {
                    return Vector3.zero;
                }

                if (pos.x > bigB.max.x)
                {
                    off.x = pos.x - bigB.max.x;
                }
                else if (pos.x < bigB.min.x)
                {
                    off.x = pos.x - bigB.min.x;
                }

                if (pos.y > bigB.max.y)
                {
                    off.y = pos.y - bigB.max.y;
                }
                else if (pos.y < bigB.min.y)
                {
                    off.y = pos.y - bigB.min.y;
                }

                if (pos.z > bigB.max.z)
                {
                    off.z = pos.z - bigB.max.z;
                }
                else if (pos.z < bigB.min.z)
                {
                    off.z = pos.z - bigB.min.z;
                }
                return off;


                // 当test为onScreen时，需要将lilB整体平移到bigB之内
            case BoundsTest.onScreen:
                if (bigB.Contains(lilB.min) && bigB.Contains(lilB.max))
                {
                    return Vector3.zero;
                }

                if (lilB.max.x > bigB.max.x)
                {
                    off.x = lilB.max.x - bigB.max.x;
                }
                else if (lilB.min.x < bigB.min.x)
                {
                    off.x = lilB.min.x - bigB.min.x;
                }

                if (lilB.max.y > bigB.max.y)
                {
                    off.y = lilB.max.y - bigB.max.y;
                }
                else if (lilB.min.y < bigB.min.y)
                {
                    off.y = lilB.min.y - bigB.min.y;
                }

                if (lilB.max.z > bigB.max.z)
                {
                    off.z = lilB.max.z - bigB.max.z;
                }
                else if (lilB.min.z < bigB.min.z)
                {
                    off.z = lilB.min.z - bigB.min.z;
                }
                return off;


            // 当test为offScreen时，需要将lilB的任意一部分平移到bigB之内
            case BoundsTest.offScreen:
                bool cMin = bigB.Contains(lilB.min);
                bool cMax = bigB.Contains(lilB.max);

                if (cMin || cMax)
                {
                    return Vector3.zero;
                }

                if (lilB.min.x > bigB.max.x)
                {
                    off.x = lilB.min.x - bigB.max.x;
                }
                else if (lilB.max.x < bigB.min.x)
                {
                    off.x = lilB.max.x - bigB.min.x;
                }

                if (lilB.min.y > bigB.max.y)
                {
                    off.y = lilB.min.y - bigB.max.y;
                }
                else if (lilB.max.y < bigB.min.y)
                {
                    off.y = lilB.max.y - bigB.min.y;
                }

                if (lilB.min.z > bigB.max.z)
                {
                    off.z = lilB.min.z - bigB.max.z;
                }
                else if (lilB.max.z < bigB.min.z)
                {
                    off.z = lilB.max.z - bigB.min.z;
                }

                return off;


        }

        return Vector3.zero;
    }

    #endregion

    #region Transform函数
    // 找到有自定义标签的父对象
    public static GameObject FindTaggedParent(GameObject go)
    {
        // 没有自定义标签的对象的标签名为untagged，在检视器能看到
        if (go.tag != "Untagged")
        {
            return go;
        }

        if (go.transform.parent == null)
        {
            return null;
        }

        return (FindTaggedParent(go.transform.parent.gameObject));
    }

    public static GameObject FindTaggedParent(Transform t)
    {
        return FindTaggedParent(t.gameObject);
    }
    #endregion

    #region 材质函数
    /// <summary>
    /// 返回对象及其子对象的所有材质
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public static Material[] GetAllMaterials(GameObject go)
    {
        List<Material> mats = new List<Material>();
        if(go.GetComponent<Renderer>()!=null)
        {
            mats.Add(go.GetComponent<Renderer>().material);
        }
        foreach (Transform t in go.transform)
        {
            mats.AddRange(GetAllMaterials(t.gameObject));
        }
        return mats.ToArray();
    }
    #endregion
}

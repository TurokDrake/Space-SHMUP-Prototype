using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    public static Main S; // 单例
    public static Dictionary<WeaponType, WeaponDefinition> W_DEFS;
    public GameObject[] prefabEnemies; // 敌人的预制体，有5种
    public float enemySpawnPerSecond = 0.5f; // 每秒生成的敌人数量
    public float enemySpawnPadding = 1.5f; // 敌人生成点与边界的内边距
    public WeaponDefinition[] weaponDefinitions;
    public GameObject prefabPowerUp;
    public WeaponType[] powerUpFrequency = new WeaponType[]
    {
        WeaponType.blaster,WeaponType.blaster,WeaponType.spread,WeaponType.shield
    };

    public bool _____;

    public WeaponType[] activeWeaponTypes;
    public float enemySpawnRate; // 生成敌机的时间间隔

    private void Awake()
    {
       S = this;
        Utils.SetCameraBounds(Camera.main);
        enemySpawnRate = 1f / enemySpawnPerSecond;
        Invoke("SpawnEnemy", enemySpawnRate);

        // 根据在检视面板中定义的WeaponDefinition初始化W_DEFS字典
        W_DEFS = new Dictionary<WeaponType, WeaponDefinition>();
        foreach (WeaponDefinition def in weaponDefinitions)
        {
            W_DEFS[def.type] = def;
        }
    }

    // 获取WeaponDefinition
    public static WeaponDefinition GetWeaponDefinition(WeaponType wt)
    {
        // 若字典里包含键wt，返回对应的WeaponDefinition
        if (W_DEFS.ContainsKey(wt))
        {
            return W_DEFS[wt];
        }
        // 若不包含键wt，就返回一个空的WeaponDefinition
        return new WeaponDefinition();
    }

    private void Start()
    {
        activeWeaponTypes = new WeaponType[weaponDefinitions.Length];
        for(int i = 0; i < weaponDefinitions.Length; i++)
        {
            activeWeaponTypes[i] = weaponDefinitions[i].type;
        }
    }
    public void SpawnEnemy()
    {
        // 随机选一个类型的敌人生成
        int ndx = Random.Range(0, prefabEnemies.Length);
        GameObject go = Instantiate<GameObject>(prefabEnemies[ndx]);

        // 确定生成的敌人的位置
        Vector3 pos = Vector3.zero;
        float xMin = Utils.camBounds.min.x + enemySpawnPadding;
        float xMax = Utils.camBounds.max.x - enemySpawnPadding;
        pos.x = Random.Range(xMin, xMax);
        pos.y = Utils.camBounds.max.y + enemySpawnPadding;
        go.transform.position = pos;

        Invoke("SpawnEnemy", enemySpawnRate);

    }

    public void DelayedRestart(float delay)
    {
        Invoke("Restart", delay);
    }
    public void Restart()
    {
        SceneManager.LoadScene("_Scene_0");
    }

    public void ShipDestroyed(Enemy e)
    {
        if (Random.value <= e.powerUpDropChance)
        {
            int ndx = Random.Range(0, powerUpFrequency.Length);
            WeaponType puType = powerUpFrequency[ndx];

            GameObject go = Instantiate<GameObject>(prefabPowerUp);
            PowerUp pu = go.GetComponent<PowerUp>();
            pu.SetType(puType);
            pu.transform.position = e.transform.position;
        }
    }
}

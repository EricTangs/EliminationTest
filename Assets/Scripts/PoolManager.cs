using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象池管理
/// </summary>
public class PoolManager {

    private static PoolManager instance;
    private static object _lock = new object();
    public static PoolManager Instance
    {
        get
        {
            lock (_lock)
            {
                if (instance == null)
                {
                    instance = new PoolManager();
                }
                return instance;
            }
        }
    }

    /// <summary>
    /// 对象池字典
    /// </summary>
    private Dictionary<string, List<GameObject>> poolDic;

    /// <summary>
    /// 加载的资源的保存，加载后保存，减少多次加载到内存
    /// </summary>
    private Dictionary<string, GameObject> loadAssetDic;

    private PoolManager()
    {
        poolDic = new Dictionary<string, List<GameObject>>();
        loadAssetDic = new Dictionary<string, GameObject>();
    }

    /// <summary>
    /// 获得一个对象
    /// </summary>
    /// <param name="objName">对象名</param>
    /// <returns>获取的对象</returns>
    public GameObject GetGameObject(string objName,Transform parentTrans = null)
    {
        GameObject result;
        if (poolDic.ContainsKey(objName))
        {
            if (poolDic[objName].Count>0)
            {
                result = poolDic[objName][0];
                poolDic[objName].Remove(result);
                result.SetActive(true);
                if (parentTrans != null)
                {
                    result.transform.SetParent(parentTrans.transform);
                }
                return result;
            }
        }

        GameObject prefab;
        if (loadAssetDic.ContainsKey(objName))
        {
            prefab = loadAssetDic[objName];
        }
        else
        {
            prefab = Resources.Load<GameObject>("Prefabs/"+objName);
            loadAssetDic.Add(objName,prefab);
        }

        result = GameObject.Instantiate(prefab);
        result.name = objName;
        if (parentTrans != null)
        {
            result.transform.SetParent(parentTrans.transform);
        }
        return result;
    }

    /// <summary>
    /// 回收对象
    /// </summary>
    /// <param name="obj">对象</param>
    public void RecycleGameObject(GameObject obj)
    {
        obj.gameObject.SetActive(false);
        if (poolDic.ContainsKey(obj.name))
        {
            poolDic[obj.name].Add(obj);
        }
        else
        {
            poolDic.Add(obj.name,new List<GameObject>() {
                obj
            });
        }
    }

}

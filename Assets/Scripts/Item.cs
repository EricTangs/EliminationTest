using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour {

    /// <summary>
    /// 行
    /// </summary>
    private int itemRow;
    public int ItemRow
    {
        get
        {
            return itemRow;
        }
        set
        {
            itemRow = value;
        }
    }

    /// <summary>
    /// 列
    /// </summary>
    private int itemColum;
    public int ItemColum
    {
        get
        {
            return itemColum;
        }
        set
        {
            itemColum = value;
        }
    }
    /// <summary>
    /// 类型标记
    /// </summary>
    private int typeFlag;
    public int TypeFlag
    {
        get
        {
            return typeFlag;
        }
        set
        {
            typeFlag = value;   
        }
    }

    /// <summary>
    /// 当前图片
    /// </summary>
    public Image currentImg;

    private GameController gameController;

    private void Awake()
    {
        gameController = GameController.instance;
        currentImg = transform.GetChild(0).GetComponent<Image>();
    }

    /// <summary>
    /// 检查消除大于3个以上的icon
    /// </summary>
    public void CheckEliminationAroundSelf()
    {
        gameController.sameItemList.Clear();
        gameController.eliminationList.Clear();
        gameController.FindAllAroundItem(this);
        gameController.FindAllEliminationItem(this);
    }
}

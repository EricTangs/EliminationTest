using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameController : MonoBehaviour {
    public static GameController instance;

    /// <summary>
    /// 行列
    /// </summary>
    public int tableRow = 6;
    public int tableColum = 6;

    /// <summary>
    /// 离中心点的偏移量
    /// </summary>
    public Vector2 offset = new Vector2(0,0);

    /// <summary>
    /// 是否在操作中
    /// </summary>
    public bool inOperation;

    /// <summary>
    /// 所有图像icon
    /// </summary>
    public Sprite[] allIcons = new Sprite[4];

    /// <summary>
    /// 所有的Item
    /// </summary>
    public Item[,] allItems;

    /// <summary>
    /// 所有item的位置保存
    /// </summary>
    public Vector3[,] allPos;

    /// <summary>
    /// 单个Item尺寸
    /// </summary>
    private float itemSize;

    /// <summary>
    /// 单个item相邻类型形同的item列表
    /// </summary>
    public List<Item> sameItemList;

    //当个item相邻的可消除的item列表
    public List<Item> eliminationList;

    private void Awake()
    {
        instance = this;
        allItems = new Item[tableRow,tableColum];
        allPos = new Vector3[tableRow, tableColum];
        sameItemList = new List<Item>();
        eliminationList = new List<Item>();
    }

    // Use this for initialization
    void Start () {
        Init();
        EliminateAllOutRuleItem();
    }

    // Update is called once per frame
    void Update () {
		
	}

    /// <summary>
    /// 初始化界面
    /// </summary>
    private void Init()
    {
        itemSize = PoolManager.Instance.GetGameObject("Item").GetComponent<RectTransform>().rect.height;
        for (int i = 0; i < tableRow; i++)
        {
            for (int j = 0; j < tableColum; j++)
            {
                GameObject itemObj = PoolManager.Instance.GetGameObject("Item", transform);
                itemObj.transform.localPosition = new Vector3(itemSize * j,itemSize * i,0) + new Vector3(offset.x,offset.y,0);
                Item item = itemObj.GetComponent<Item>();
                item.TypeFlag = Random.Range(0, allIcons.Length);
                item.ItemRow = i;
                item.ItemColum = j;
                item.currentImg.sprite = allIcons[item.TypeFlag];
                allItems[i, j] = item;
                allPos[i, j] = item.transform.position;
            }
        }
    }

    /// <summary>
    /// 检测所有的item该消除的Item
    /// </summary>
    private void EliminateAllOutRuleItem()
    {
        foreach (var item in allItems)
        {
            if (item == null)
                continue;
            item.CheckEliminationAroundSelf();
            if (eliminationList.Count>0)
            {
                inOperation = true;
            }
        }
        inOperation = false;
    }

    /// <summary>
    /// 查看在row，column的item是否合法
    /// </summary>
    /// <param name="row">行列</param>
    /// <param name="colum"></param>
    /// <returns>是否合法</returns>
    public bool CheckItemIllegal(int row ,int column)
    {
        if (row >= 0 && column >= 0 && row < tableRow && column < tableColum)
            return true;
        return false;   
    }

    /// <summary>
    /// 获取上方Item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private Item GetUpItem(Item item)
    {
        int row = item.ItemRow + 1;
        int column = item.ItemColum;
        if (!CheckItemIllegal(row,column))
        {
            return null;
        }
        return allItems[row,column];
    }

    /// <summary>
    /// 获取下方Item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private Item GetDownItem(Item item)
    {
        int row = item.ItemRow - 1;
        int column = item.ItemColum;
        if (!CheckItemIllegal(row, column))
        {
            return null;
        }
        return allItems[row, column];
    }

    /// <summary>
    /// 获取左边Item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private Item GetLeftItem(Item item)
    {
        int row = item.ItemRow;
        int column = item.ItemColum - 1;
        if (!CheckItemIllegal(row, column))
        {
            return null;
        }
        return allItems[row, column];
    }

    /// <summary>
    /// 获取右边Item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private Item GetRightItem(Item item)
    {
        int row = item.ItemRow;
        int column = item.ItemColum + 1;
        if (!CheckItemIllegal(row, column))
        {
            return null;
        }
        return allItems[row, column];
    }

    /// <summary>
    /// 查找单个item相邻的item，并保存到sameItemList里
    /// </summary>
    /// <param name="item">item</param>
    public void FindAllAroundItem(Item item)
    {
        if (sameItemList.Contains(item))
            return;
        //以一个点为中心，检测上下左右
        Item[] tempItemList = new Item[]
        {
            GetUpItem(item),
            GetDownItem(item),
            GetLeftItem(item),
            GetRightItem(item)
        };
        for (int i = 0; i < tempItemList.Length; i++)
        {
            if (tempItemList[i] == null)
                continue;
            //符合要求的添加进来
            if (tempItemList[i].TypeFlag == item.TypeFlag)
            {
                //添加到列表
                sameItemList.Add(item);
                //item.currentImg.color = Color.red;
                FindAllAroundItem(tempItemList[i]);
            }
        }
    }

    /// <summary>
    /// 挑出需要消除的item
    /// </summary>
    /// <param name="item"></param>
    public void FindAllEliminationItem(Item item)
    {
        List<Item> rowTempList = new List<Item>();
        List<Item> columnTempList = new List<Item>();
        //对单个Item的横向和竖向检测
        foreach (var currentItem in sameItemList)
        {
            //在同一行
            if (currentItem.ItemRow == item.ItemRow)
            {
                //没有间隔
                bool canElimination = CheckHasInterval(true,currentItem,item);
                if (canElimination && !rowTempList.Contains(currentItem))
                {
                    rowTempList.Add(currentItem);
                }
            }
            if (currentItem.ItemColum == item.ItemColum)
            {
                //没有间隔
                bool canElimination = CheckHasInterval(false, currentItem, item);
                if (canElimination && !columnTempList.Contains(currentItem))
                {
                    columnTempList.Add(currentItem);
                }
            }
        }
        bool isHorizontalElimination = false;
        if (rowTempList.Count >= 3)
        {
            eliminationList.AddRange(rowTempList);
            isHorizontalElimination = true;
        }
        if (columnTempList.Count >= 3)
        {
            eliminationList.AddRange(columnTempList);
            if (isHorizontalElimination)
            {
                eliminationList.Remove(item);
            }
        }
        if (eliminationList.Count == 0)
            return;
        //for (int m = 0; m < eliminationList.Count; m++)
        //{
        //    eliminationList[m].currentImg.color = Color.red;
        //}
        //消除处理
        DealEliminationItem(eliminationList);
        StartCoroutine(DropItem());
    }

    /// <summary>
    /// Item下落，着列去除不是空的Item,在坠落
    /// </summary>
    /// <returns></returns>
    IEnumerator DropItem()
    {
        for (int i = 0; i < tableColum; i++)
        {
            //将单列存在的item存入队列
            Queue<Item> itemQueue = new Queue<Item>();
            int itemCount = 0;
            for (int j = 0; j < tableRow; j++)
            {
                if (allItems[j, i] != null)
                {
                    itemCount++;
                    itemQueue.Enqueue(allItems[j, i]);
                }
            }
            if (itemCount == 0)
                continue;
            for (int m = 0; m < itemCount; m++)
            {
                Item item = itemQueue.Dequeue();
                allItems[item.ItemRow, item.ItemColum] = null;
                item.ItemRow = m;
                allItems[item.ItemRow, item.ItemColum] = item;
                item.GetComponent<ItemOperation>().ItemMove(m,i,allPos[m,i]);
            }
        }
        yield return new WaitForSeconds(.6f);
        StartCoroutine(CreateItem());
    }

    /// <summary>
    /// 生成item
    /// </summary>
    IEnumerator CreateItem()
    {
        for (int i = 0; i < tableColum; i++)
        {
            //将单列存在的item存入队列
            Queue<Item> itemQueue = new Queue<Item>();
            int itemCount = 0;
            for (int j = 0; j < tableRow; j++)
            {
                if (allItems[j, i] == null)
                {
                    itemCount++;
                    GameObject itemObj = PoolManager.Instance.GetGameObject("Item", transform);
                    Item item = itemObj.GetComponent<Item>();
                    item.transform.position = allPos[tableRow - 1, i];
                    item.ItemRow = j;
                    item.ItemColum = i;
                    itemQueue.Enqueue(item);
                }
            }
            if (itemCount == 0)
                continue;
            for (int m = 0; m < itemCount; m++)
            {
                Item item = itemQueue.Dequeue();
                item.TypeFlag = Random.Range(0,allIcons.Length);
                item.currentImg.sprite = allIcons[item.TypeFlag];
                allItems[item.ItemRow, item.ItemColum] = item;
                item.GetComponent<ItemOperation>().ItemMove(item.ItemRow, item.ItemColum, allPos[item.ItemRow, item.ItemColum]);
                item.GetComponent<CanvasGroup>().alpha = 1;
            }
        }
        yield return new WaitForSeconds(.5f);
        EliminateAllOutRuleItem();
    }

    /// <summary>
    /// 消除EliminationList中Item
    /// </summary>
    /// <param name="tempList"></param>
    private void DealEliminationItem(List<Item> tempList)
    {
        foreach (var item in tempList)
        {
            allItems[item.ItemRow, item.ItemColum] = null;
            item.GetComponent<CanvasGroup>().DOFade(0, .2f).OnComplete(()=> {
                PoolManager.Instance.RecycleGameObject(item.gameObject);
            });
        }
    }

    /// <summary>
    /// 检查两个之间是否有空格
    /// </summary>
    /// <param name="isHorizontal">是否是水平</param>
    /// <param name="beginItem">开始Item</param>
    /// <param name="endItem">结束Item</param>
    /// <returns>有间隔返回false</returns>
    private bool CheckHasInterval(bool isHorizontal,Item begin, Item end)
    {
        //获取图案
        int typeFlag = begin.TypeFlag;
        //如果是横向
        if (isHorizontal)
        {
            //起点终点列号
            int beginIndex = begin.ItemColum;
            int endIndex = end.ItemColum;
            //如果起点在右，交换起点终点列号
            if (beginIndex > endIndex)
            {
                beginIndex = end.ItemColum;
                endIndex = begin.ItemColum;
            }
            //遍历中间的Item
            for (int i = beginIndex + 1; i < endIndex; i++)
            {
                //异常处理（中间未生成，标识为不合法）
                if (allItems[begin.ItemRow, i] == null)
                    return false;
                //如果中间有间隙（有图案不一致的）
                if (allItems[begin.ItemRow, i].TypeFlag != typeFlag)
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            //起点终点行号
            int beginIndex = begin.ItemRow;
            int endIndex = end.ItemRow;
            //如果起点在上，交换起点终点列号
            if (beginIndex > endIndex)
            {
                beginIndex = end.ItemRow;
                endIndex = begin.ItemRow;
            }
            //遍历中间的Item
            for (int i = beginIndex + 1; i < endIndex; i++)
            {
                //如果中间有间隙（有图案不一致的）
                if (allItems[i, begin.ItemColum].TypeFlag != typeFlag)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

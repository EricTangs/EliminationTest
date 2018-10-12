using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ItemOperation : MonoBehaviour,IPointerDownHandler,IPointerUpHandler {
    //按下鼠标位置
    private Vector3 startPos;

    //弹起鼠标位置
    private Vector3 endPos;

    /// <summary>
    /// Item对象
    /// </summary>
    private Item item;

    private void OnEnable()
    {
        item = transform.GetComponent<Item>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        startPos = Input.mousePosition;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (GameController.instance.inOperation)
            return;
        GameController.instance.inOperation = true;
        endPos = Input.mousePosition;
        Vector2 dir = GetDirector();
        if (dir.magnitude != 1)
        {
            Debug.LogError("dir error!");
            GameController.instance.inOperation = false;
            return;
        }

        //交换
        StartCoroutine(ItemExchange(dir));
    }

    /// <summary>
    /// 交换Item
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    IEnumerator ItemExchange(Vector2 dir)
    {
        int row = item.ItemRow + System.Convert.ToInt32((int)dir.y);
        int column = item.ItemColum + System.Convert.ToInt32((int)dir.x);
        bool isIllegal = GameController.instance.CheckItemIllegal(row, column);
        if (!isIllegal)
        {
            GameController.instance.inOperation = false;
            yield break;
        }
        Item exchangeItem = GameController.instance.allItems[row, column];
        if (!exchangeItem || !item)
        {
            GameController.instance.inOperation = false;
            yield break;
        }
        exchangeItem.GetComponent<ItemOperation>().ItemMove(item.ItemRow,item.ItemColum,transform.position);
        this.ItemMove(row,column,GameController.instance.allPos[row,column]);
        bool isBack=false;
        item.CheckEliminationAroundSelf();
        if (GameController.instance.eliminationList.Count == 0)
        {
            isBack = true;
        }
        exchangeItem.CheckEliminationAroundSelf();
        if (GameController.instance.eliminationList.Count != 0)
        {
            isBack = false;
        }

        yield return new WaitForSeconds(.5f);
        //未符合要求恢复交换
        if (isBack)
        {
            int tempRow, tempColumn;
            tempRow = item.ItemRow;
            tempColumn = item.ItemColum;
            //移动
            item.GetComponent<ItemOperation>().ItemMove(exchangeItem.ItemRow,
                exchangeItem.ItemColum, GameController.instance.allPos[exchangeItem.ItemRow, exchangeItem.ItemColum]);
            exchangeItem.GetComponent<ItemOperation>().ItemMove(tempRow,
                tempColumn, GameController.instance.allPos[tempRow, tempColumn]);
        }
        GameController.instance.inOperation = false;
    }

    /// <summary>
    /// 获得滑动方向
    /// </summary>
    /// <returns></returns>
    private Vector2 GetDirector()
    {
        //方向
        Vector2 dir = endPos - startPos;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            //左右两边滑动,dir.x/绝对值获得方向
            return new Vector2(dir.x/Mathf.Abs(dir.x),0);
        }
        else
        {
            //上下滑动
            return new Vector2(0, dir.y/Mathf.Abs(dir.y));
        }
    }

    /// <summary>
    /// 移动至某行某列
    /// </summary>
    /// <param name="row">行</param>
    /// <param name="column">列</param>
    /// <param name="pos">位置</param>
    public void ItemMove(int row ,int column,Vector3 pos)
    {
        item.ItemRow = row;
        item.ItemColum = column;
        GameController.instance.allItems[row, column] = item;
        transform.DOMove(pos,.5f);
    }

}

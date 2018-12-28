using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Vector2 offset;
    public float PosX=-110f;//都是Start里赋值
    public float PosY=0f;
    public float Width = 60f;
    public Text text;

    public int index=1;//从1开始

    public void OnBeginDrag(PointerEventData eventData)
    {
        offset = transform.position - new Vector3(eventData.position.x, eventData.position.y, 0);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position + offset;
        Debug.Log(GetComponent<RectTransform>().anchoredPosition);
        
        //Debug.Log(transform.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        float x = GetComponent<RectTransform>().anchoredPosition.x;
        float y = GetComponent<RectTransform>().anchoredPosition.y;
        float r = Mathf.Sqrt((float)150.0 * 150 - Mathf.Pow(Width / 2, 2)) - Width / 2;
        if (x*x+y*y>Mathf.Pow(r,2))
        {
            GetComponent<RectTransform>().anchoredPosition = new Vector2(PosX, PosY);
        }
    }

    float Spacing()
    {
        if(Font.FontNumber==0)
        {
            Debug.LogError("FontNumber == 0");
            return -1;
        }
        else
        {
            return (300 - Font.FontNumber * Width) / Font.FontNumber;
        }
    }

    public void SetStartPos()
    {
        if (Font.FontNumber % 2 == 1)//字的各位为奇数
        {
            float index_1_X = -((Font.FontNumber - 1)/2 * (Width + Spacing()));
            PosX = index_1_X + (index - 1) * (Width + Spacing());
            PosY = 0f;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(PosX, PosY);
        }
    }
}

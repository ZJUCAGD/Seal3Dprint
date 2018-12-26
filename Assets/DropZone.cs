using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour,IDropHandler {
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop to" + gameObject.name);
    }
}

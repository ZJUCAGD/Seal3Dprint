using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour{
    public GameObject SingleIconPrefab;
    public static List<GameObject> Icons = new List<GameObject>();
    public void Spawn()
    {
        int childCount = transform.childCount;
        for(int i=0;i<childCount;i++)
        {
            Icons.Remove(transform.GetChild(i).gameObject);
            Destroy(transform.GetChild(i).gameObject);
        }
        for(int i=1;i<=Font.FontNumber;i++)
        {
            GameObject go = Instantiate(SingleIconPrefab, transform) as GameObject;
            go.GetComponent<Draggable>().index = i;
            go.GetComponent<Draggable>().SetStartPos();
            go.GetComponent<Draggable>().text.text = Font.str.Substring(go.GetComponent<Draggable>().index - 1, 1);
            Icons.Add(go);
        }
    }
}

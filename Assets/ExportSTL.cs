using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Common;


public class ExportSTL : MonoBehaviour {

    public GameObject[] gameObjects;

    //private void Start()
    //{
    //    gameObjects = new GameObject[2];
    //}

    public void exportSTL()
    {
        string path = saveFileDialog();
        STL.ExportText(gameObjects, path);
    }
    public string saveFileDialog()
    {
        string filepath = "";
        SaveFileDlg pth = new SaveFileDlg();
        pth.structSize = System.Runtime.InteropServices.Marshal.SizeOf(pth);
        pth.filter = "stl (*.stl)";
        pth.file = new string(new char[256]);
        pth.maxFile = pth.file.Length;
        pth.fileTitle = new string(new char[64]);
        pth.maxFileTitle = pth.fileTitle.Length;
        pth.initialDir = Application.dataPath;  // default path  
        pth.title = "";
        pth.defExt = "stl";
        pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
        if (SaveFileDialog.GetSaveFileName(pth))
        {
            filepath = pth.file;//选择的文件路径;  
            Debug.Log(filepath);
        }
        return filepath;
    }

}

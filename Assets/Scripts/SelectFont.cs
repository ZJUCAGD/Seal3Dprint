using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;


public class SelectFont : MonoBehaviour {

    // Use this for initialization
    public Dropdown dropdown;
    public FontFamily selectedFont = null;
    InstalledFontCollection MyFont = new InstalledFontCollection();
    void Start () {
        SetFontList();
	}

    private void SetFontList()
    {
        
        List<string> FontNames = new List<string>();
        
        foreach (var font in MyFont.Families)
        {
            FontNames.Add(font.Name);
        }
        FontNames[0] = "Please select a Font";
        dropdown.AddOptions(FontNames);
    }

    public void IndexChanged(int index)
    {
        selectedFont = MyFont.Families[index];
        Debug.Log("You select " + selectedFont.Name);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using Poly2Tri;
using System;
using System.Linq;
using UnityEngine.UI;


public class Font : MonoBehaviour {
    public static int FontNumber = 0;
    
    public GameObject Prefab;
    public GameObject prefab2;
    public GameObject FontSelect;
    public GameObject TextMeshPrefab;
    public List<GameObject> TextMeshs=new List<GameObject>();

    GraphicsPath path;
    private float GlyphFontSize = 35f;        //17|90    *150*2=
    FontFamily testFont;
    public static string str;
    public float Yoffset = 10f;

    Mesh mesh=null;
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;


    private int CurrentIndex;
    private int AllIndexCount;
    float XaverageLocal;
    float ZaverageLocal;

    public class PolygonHierachy
    {
        public Polygon Current;
        public List<PolygonHierachy> Childs;
        public PolygonHierachy Next;

        public PolygonHierachy(Polygon current)
        {
            Current = current;
            Childs = new List<PolygonHierachy>();
            Next = null;
        }

    }
    
    public List<Polygon> GeneratePolygonsFromGlyph(FontFamily fontFamily, System.Drawing.FontStyle style, string glyph)
    {
        //int PointNum=0;
        XaverageLocal=0;
        ZaverageLocal=0;
        PointF[] pts = null;
        byte[] ptsType = null;

        using (var path = new GraphicsPath())
        {
            path.AddString(glyph, fontFamily, (int)style,
                GetComponent<Font>().GlyphFontSize, new PointF(0f, 0f), StringFormat.GenericDefault);
            path.Flatten();
            if (path.PointCount == 0)
            {
                Debug.LogError("path.pointCount = 0");
                return new List<Polygon>();                
            }
            Debug.Log("path.PointCount="+ path.PointCount);
            pts = path.PathPoints;
            ptsType = path.PathTypes;
        }

        var polygons = new List<Polygon>();
        List<PolygonPoint> points = null;

        for (var i = 0; i < pts.Length; i++)
        {
            XaverageLocal += pts[i].X;
            ZaverageLocal += pts[i].Y;
        }
        XaverageLocal = XaverageLocal / pts.Length;
        ZaverageLocal = ZaverageLocal / pts.Length;
        Debug.Log("XaverageLocal = " + XaverageLocal);
        Debug.Log("ZaverageLocal = " + ZaverageLocal);
        for (var i=0;i<pts.Length;i++)              //! 将mesh 的点的重心放在（0,0）
        {
            pts[i].X -= XaverageLocal;
            pts[i].Y -= ZaverageLocal;
        }
        var start = -1;
        for (var i = 0; i < pts.Length; i++)
        {

            var pointType = ptsType[i] & 0x07;
            if (pointType == 0)
            {
                points = new List<PolygonPoint> { new PolygonPoint(pts[i].X, pts[i].Y) };
                //Debug.Log("第" + i + "个点的坐标为" + pts[i].X + "," + pts[i].Y);
                start = i;
                continue;
            }
            if (pointType != 1) throw new System.Exception("Unsupported point type");
            if ((ptsType[i] & 0x80) != 0)
            {
                if ((pts[i] != pts[start]))
                {
                    //Debug.Log("不等于start");
                    points.Add(new PolygonPoint(pts[i].X, pts[i].Y));
                }

                polygons.Add(new Polygon(points));//在这里生成一个一个的polygon
                //foreach (var point in points)
                //{
                //    XaverageLocal += point.Xf;
                //    ZaverageLocal += point.Yf;
                //}
                points = null;
            }
            else
            {
                points.Add(new PolygonPoint(pts[i].X, pts[i].Y));
            }
        }

        //Debug.Log("polygons.Count = "+ polygons.Count);
        //Debug.Log("PointNum=" + PointNum);

        return polygons;
    }

    public void OnPressGenerateButton()
    {
        testFont = FontSelect.GetComponent<SelectFont>().selectedFont;
        if (testFont == null)
        {
            StartCoroutine(ShowWarning());
            return;
        }
        str = GameObject.Find("InputField").GetComponent<GetText>().enteredString;
        if(str=="")
        {
            StartCoroutine(ShowWarning());
            return;
        }
        if (TextMeshs.Count!=0)
        {
            for(int i=TextMeshs.Count-1;i>=0;i--)
            {
                GameObject go = TextMeshs[i];
                TextMeshs.Remove(go);
                GameObject.Destroy(go);
            }
        }
        else
        {
            Debug.Log("没有");
        }
        FontNumber = str.Length;

        GameObject.Find("Canvas").transform.Find("DropArea").GetComponent<DropZone>().Spawn();

        for (int i=1;i<=FontNumber;i++)
        {
            GameObject go =Instantiate(TextMeshPrefab, transform.position, Quaternion.identity, transform) as GameObject;
            string tmp = str.Substring(i-1, 1);
            GenerateFontMesh(tmp, go);
            float x = DropZone.Icons[i-1].GetComponent<RectTransform>().anchoredPosition.x/150*90;
            float z = DropZone.Icons[i-1].GetComponent<RectTransform>().anchoredPosition.y/150*90;
            go.transform.position = new Vector3(x, 0, z);
            TextMeshs.Add(go);
        }
    }
    public void UpdateFontMesh()
    {
        if (str == "")
            return;
        for (int i = 1; i <= FontNumber; i++)
        {
            float x = DropZone.Icons[i - 1].GetComponent<RectTransform>().anchoredPosition.x / 150 * 90;
            float z = DropZone.Icons[i - 1].GetComponent<RectTransform>().anchoredPosition.y / 150 * 90;
            TextMeshs[i-1].transform.position = new Vector3(x, 0, -z);
        }
    }

    public void GenerateFontMesh(string str,GameObject meshObject)
    {
        CurrentIndex = 0;
        AllIndexCount = 0;

        mesh = new Mesh();
        meshObject.GetComponent<MeshFilter>().mesh = mesh;
        
        Debug.Log("testFont.name = " + testFont.Name);
        //str = GameObject.Find("InputField").GetComponent<GetText>().enteredString;
        if(str==null)
        {
            Debug.LogError("not set a str to convert");
            return;
        }

        var polygonList = GeneratePolygonsFromGlyph(testFont, System.Drawing.FontStyle.Regular, str);

        var polygonSet = CreateSetFromList(polygonList);

        P2T.Triangulate(polygonSet);
        if (polygonSet.Polygons == null)
        {
            Debug.LogError("There is no polygon in the polygonSet");
            return;
        }

        foreach (var polygon in polygonSet.Polygons)
        {
            AllIndexCount += AddIndexCount(polygon);
            //Debug.Log(AddIndexCount(polygon));
        }
        triangles = new int[AllIndexCount];
        vertices = new Vector3[AllIndexCount];
        uvs = new Vector2[AllIndexCount];

        foreach (var polygon in polygonSet.Polygons)
        {
            SetFace(polygon);
        }
    }
    
    public int AddIndexCount(Polygon polygon)
    {
        int TotalIndexCount = polygon.Points.Count * 6 + polygon.Triangles.Count * 3 * 2;
        if(polygon.Holes!=null)
        {
            foreach (var hole in polygon.Holes)
            {
                TotalIndexCount += hole.Points.Count * 6;
            }
        }
        return TotalIndexCount;
    }
    public void SetFace(Polygon polygon)
    {
        
        int PointsCount = polygon.Points.Count;
        /* EdgeCount == PointCount , TriCount == EdgesCount * 2 */
        int SideTriCount = PointsCount * 2;
        int BottomTriCount = polygon.Triangles.Count;      
        //int TotalIndexCount = SideTriCount * 3 + BottomTriCount * 3 * 2;
        ////Debug.Log(TotalIndexCount);
        //if(polygon.Holes!=null)
        //{
        //    foreach (var hole in polygon.Holes)
        //    {
        //        TotalIndexCount += hole.Points.Count * 6;
        //    }
        //}
        ////Debug.Log(TotalIndexCount);
        //triangles = new int[TotalIndexCount];
        //vertices = new Vector3[TotalIndexCount];
        List<Vector3> BottomPoints = new List<Vector3>();   //y=0;
        List<Vector3> TopPoints = new List<Vector3>();      //y=offset;

        for (int i = 0; i < PointsCount; i++) 
        {
            BottomPoints.Add(new Vector3(polygon.Points[i].Xf, 0, polygon.Points[i].Yf));
            TopPoints.Add(new Vector3(polygon.Points[i].Xf, Yoffset, polygon.Points[i].Yf));
        }
        /* set outside faces */ //! For one loop set 2 triangles
        for (int i = 0; i < PointsCount; i++) 
        {
            if (i < PointsCount - 1)
            {                                                               //i+1,i,i'---i',i+1',i+1
                vertices[CurrentIndex + 6 * i] = BottomPoints[i+1];
                vertices[CurrentIndex + 6 * i + 1] = BottomPoints[i];
                vertices[CurrentIndex + 6 * i + 2] = TopPoints[i];
                vertices[CurrentIndex + 6 * i + 3] = TopPoints[i];
                vertices[CurrentIndex + 6 * i + 4] = TopPoints[i+1];
                vertices[CurrentIndex + 6 * i + 5] = BottomPoints[i + 1];
            }
            else
            {
                vertices[CurrentIndex + 6 * i] = BottomPoints[0];
                vertices[CurrentIndex + 6 * i + 1] = BottomPoints[i];
                vertices[CurrentIndex + 6 * i + 2] = TopPoints[i];
                vertices[CurrentIndex + 6 * i + 3] = TopPoints[i];
                vertices[CurrentIndex + 6 * i + 4] = TopPoints[0];
                vertices[CurrentIndex + 6 * i + 5] = BottomPoints[0];
            }
            triangles[CurrentIndex + 6 * i] = CurrentIndex + 6 * i;
            triangles[CurrentIndex + 6 * i + 1] = CurrentIndex + 6 * i + 1;
            triangles[CurrentIndex + 6 * i + 2] = CurrentIndex + 6 * i + 2;
            triangles[CurrentIndex + 6 * i + 3] = CurrentIndex + 6 * i + 3;
            triangles[CurrentIndex + 6 * i + 4] = CurrentIndex + 6 * i + 4;
            triangles[CurrentIndex + 6 * i + 5] = CurrentIndex + 6 * i + 5;
        }
        CurrentIndex += SideTriCount * 3;
        /* set bottom face, which y =0 */
        // index begin from 
        //! SideTriCount * 3 
        for (int i = 0; i < BottomTriCount; i++) 
        {
            //Vector3 _0minus1 = new Vector3(polygon.Triangles[i].Points[0].Xf, 0, polygon.Triangles[i].Points[0].Yf)
            //    - new Vector3(polygon.Triangles[i].Points[1].Xf, 0, polygon.Triangles[i].Points[1].Yf);
            //Vector3 _1minus2 = new Vector3(polygon.Triangles[i].Points[1].Xf, 0, polygon.Triangles[i].Points[1].Yf)
            //    - new Vector3(polygon.Triangles[i].Points[2].Xf, 0, polygon.Triangles[i].Points[2].Yf);
            //Vector3 Normal = Vector3.Cross(_0minus1, _1minus2);
            //Debug.Log(Normal);
            vertices[CurrentIndex + 3 * i] = new Vector3(polygon.Triangles[i].Points[0].Xf, 0, polygon.Triangles[i].Points[0].Yf);
            vertices[CurrentIndex + 3 * i + 1] = new Vector3(polygon.Triangles[i].Points[1].Xf, 0, polygon.Triangles[i].Points[1].Yf);
            vertices[CurrentIndex + 3 * i + 2] = new Vector3(polygon.Triangles[i].Points[2].Xf, 0, polygon.Triangles[i].Points[2].Yf);
            triangles[CurrentIndex + 3 * i] = CurrentIndex + 3 * i;
            triangles[CurrentIndex + 3 * i + 1] = CurrentIndex + 3 * i + 1;
            triangles[CurrentIndex + 3 * i + 2] = CurrentIndex + 3 * i + 2;

        }
        CurrentIndex += BottomTriCount * 3;
        /* set top face, which y =offset */
        for (int i = 0; i < BottomTriCount; i++)
        {
            //vertices oritation 1-----0------2
            vertices[CurrentIndex + 3 * i] = new Vector3(polygon.Triangles[i].Points[1].Xf, Yoffset, polygon.Triangles[i].Points[1].Yf);
            vertices[CurrentIndex + 3 * i + 1] = new Vector3(polygon.Triangles[i].Points[0].Xf, Yoffset, polygon.Triangles[i].Points[0].Yf);
            vertices[CurrentIndex + 3 * i + 2] = new Vector3(polygon.Triangles[i].Points[2].Xf, Yoffset, polygon.Triangles[i].Points[2].Yf);
            triangles[CurrentIndex + 3 * i] = CurrentIndex + 3 * i;
            triangles[CurrentIndex + 3 * i + 1] = CurrentIndex + 3 * i + 1;
            triangles[CurrentIndex + 3 * i + 2] = CurrentIndex + 3 * i + 2;

        }
        CurrentIndex += BottomTriCount * 3;
        /* set holes face */
        if (polygon.Holes != null)
        {
            foreach (var hole in polygon.Holes)
            {
                /* set inside face */
                for (int i = 0; i < hole.Points.Count; i++)
                {
                    if (i < hole.Points.Count - 1)
                    {
                        vertices[CurrentIndex + 6 * i] = new Vector3(hole.Points[i + 1].Xf, 0, hole.Points[i + 1].Yf);
                        vertices[CurrentIndex + 6 * i + 1] = new Vector3(hole.Points[i].Xf, 0, hole.Points[i].Yf);
                        vertices[CurrentIndex + 6 * i + 2] = new Vector3(hole.Points[i].Xf, Yoffset, hole.Points[i].Yf);
                        vertices[CurrentIndex + 6 * i + 3] = new Vector3(hole.Points[i].Xf, Yoffset, hole.Points[i].Yf);
                        vertices[CurrentIndex + 6 * i + 4] = new Vector3(hole.Points[i + 1].Xf, Yoffset, hole.Points[i + 1].Yf);
                        vertices[CurrentIndex + 6 * i + 5] = new Vector3(hole.Points[i + 1].Xf, 0, hole.Points[i + 1].Yf);
                        //Instantiate(Prefab, new Vector3(hole.Points[i].Xf, 0, hole.Points[i].Yf), Quaternion.identity);
                        //Instantiate(Prefab, new Vector3(hole.Points[i + 1].Xf, 0, hole.Points[i + 1].Yf), Quaternion.identity);
                        //Instantiate(Prefab, new Vector3(hole.Points[i].Xf, Yoffset, hole.Points[i].Yf), Quaternion.identity);
                    }
                    else
                    {
                        vertices[CurrentIndex + 6 * i] = new Vector3(hole.Points[0].Xf, 0, hole.Points[0].Yf);
                        vertices[CurrentIndex + 6 * i + 1] = new Vector3(hole.Points[i].Xf, 0, hole.Points[i].Yf);
                        vertices[CurrentIndex + 6 * i + 2] = new Vector3(hole.Points[i].Xf, Yoffset, hole.Points[i].Yf);
                        vertices[CurrentIndex + 6 * i + 3] = new Vector3(hole.Points[i].Xf, Yoffset, hole.Points[i].Yf);
                        vertices[CurrentIndex + 6 * i + 4] = new Vector3(hole.Points[0].Xf, Yoffset, hole.Points[0].Yf);
                        vertices[CurrentIndex + 6 * i + 5] = new Vector3(hole.Points[0].Xf, 0, hole.Points[0].Yf);
                    }
                    triangles[CurrentIndex + 6 * i] = CurrentIndex + 6 * i;
                    triangles[CurrentIndex + 6 * i + 1] = CurrentIndex + 6 * i + 1;
                    triangles[CurrentIndex + 6 * i + 2] = CurrentIndex + 6 * i + 2;
                    triangles[CurrentIndex + 6 * i + 3] = CurrentIndex + 6 * i + 3;
                    triangles[CurrentIndex + 6 * i + 4] = CurrentIndex + 6 * i + 4;
                    triangles[CurrentIndex + 6 * i + 5] = CurrentIndex + 6 * i + 5;

                }
                CurrentIndex += hole.Points.Count * 6;
            }
        }
        UpdateMesh();
    }
    IEnumerator ShowPoint(Polygon polygon)
    {
        //for(int i=0;i<polygon.Points.Count;i++)
        //{
        //    Instantiate(Prefab, new Vector3((float)polygon.Points[i].X, 0, (float)polygon.Points[i].Y), Quaternion.identity);
        //    yield return new WaitForSeconds(0.1f);
        //}
        Debug.Log("polygon.Holes.Count = " + polygon.Holes.Count<Polygon>());
        //todo test the hole point orientation
        foreach(Polygon hole in polygon.Holes)
        {
            foreach(var point in hole.Points)
            {
                Instantiate(Prefab, new Vector3(point.Xf, 0, point.Yf), Quaternion.identity);
                yield return new WaitForSeconds(1f);
            }
        }
    }
    public void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }

    static void ProcessSetLevel(PolygonSet set, PolygonHierachy current)
    {
        while (current != null)
        {
            var poly = current.Current;
            foreach (var child in current.Childs)
            {
                poly.AddHole(child.Current);
                foreach (var grandchild in child.Childs) ProcessSetLevel(set, grandchild);
            }
            set.Add(poly);
            current = current.Next;
        }
    }
    public PolygonSet CreateSetFromList(List<Polygon> source)
    {
        var root = new PolygonHierachy(source[0]);

        for(var i=1;i<source.Count;++i)
        {
            ProcessLevel(source[i],ref root);
        }

        var set = new PolygonSet();
        ProcessSetLevel(set, root);

        return set;
    }

    public void ProcessLevel(Polygon poly,ref PolygonHierachy localRoot)
    {
        if(localRoot==null)
        {
            localRoot = new PolygonHierachy(poly);
            return;
        }

        // Check if source is the new root
        if (CheckIfInside(localRoot.Current.Points, poly.Points))
        {
            var nroot = new PolygonHierachy(poly);
            var tmp = localRoot;
            while (tmp != null)
            {
                var cur = tmp;
                tmp = tmp.Next;
                cur.Next = null;
                nroot.Childs.Add(cur);
            }

            localRoot = nroot;
            return;
        }

        // Check if source is not in the local root
        if (!CheckIfInside(poly.Points, localRoot.Current.Points))
        {
            ProcessLevel(poly, ref localRoot.Next);
            return;
        }

        // Now process the childs
        for (var i = 0; i < localRoot.Childs.Count; ++i)
        {
            if (!CheckIfInside(poly.Points, localRoot.Childs[i].Current.Points)) continue;

            // Process to the child level
            var childRoot = localRoot.Childs[i];
            ProcessLevel(poly, ref childRoot);
            localRoot.Childs[i] = childRoot;
            return;
        }

        // Else -> new child
        var newChildList = new List<PolygonHierachy>();
        var newPoly = new PolygonHierachy(poly);
        newChildList.Add(newPoly);
        for (var i = 0; i < localRoot.Childs.Count; ++i)
        {
            if (CheckIfInside(localRoot.Childs[i].Current.Points, poly.Points))
            {
                newPoly.Childs.Add(localRoot.Childs[i]);
            }
            else
            {
                newChildList.Add(localRoot.Childs[i]);
            }
        }

        localRoot.Childs = newChildList; //.Childs.Add(new PolygonHierachy(poly));
    }

    static bool CheckIfInside(IList<TriangulationPoint> polygonToTest, IList<TriangulationPoint> containingPolygon)
    {
        var t = 0;
        for (var i = 0; i < polygonToTest.Count; ++i)
        {
            if (PointInPolygon(polygonToTest[i], containingPolygon)) t++;
        }

        return ((float)t) >= (polygonToTest.Count * .2f) ? true : false;
    }

    static bool PointInPolygon(TriangulationPoint p, IList<TriangulationPoint> poly)
    {
        PolygonPoint p1, p2;
        var inside = false;
        var oldPoint = new PolygonPoint(poly[poly.Count - 1].X, poly[poly.Count - 1].Y);

        for (var i = 0; i < poly.Count; i++)
        {
            var newPoint = new PolygonPoint(poly[i].X, poly[i].Y);
            if (newPoint.X > oldPoint.X) { p1 = oldPoint; p2 = newPoint; }
            else { p1 = newPoint; p2 = oldPoint; }
            if ((newPoint.X < p.X) == (p.X <= oldPoint.X) && ((long)p.Y - (long)p1.Y) * (long)(p2.X - p1.X)
                 < ((long)p2.Y - (long)p1.Y) * (long)(p.X - p1.X))
            {
                inside = !inside;
            }
            oldPoint = newPoint;
        }
        return inside;
    }
    // Update is called once per frame


    IEnumerator ShowWarning()
    {
        GameObject.Find("Canvas").transform.Find("FontWarningText").gameObject.SetActive(true);
        if (testFont!=null)
            GameObject.Find("Canvas").transform.Find("FontWarningText").GetComponent<Text>().text = "please enter Text!";
        yield return new WaitForSeconds(1f);
        GameObject.Find("Canvas").transform.Find("FontWarningText").GetComponent<Text>().text = "please select Font!";
        GameObject.Find("Canvas").transform.Find("FontWarningText").gameObject.SetActive(false);

    }

}

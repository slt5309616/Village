using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
/// <summary>
///  地图信息处理类
/// </summary>
/// 压缩过的地图信息每个元素的格式如下
/// 72.1,38.0;13635,13635;13689,13710
/// 每个部分用；分割
/// 第一部分为坐标，只保存x，z坐标
/// 以后的每个部分为每一行有效点的开始结束坐标
public class MapUtil  {
    private static MapUtil instance = null;
    Dictionary<int,MapData> mapDic = new Dictionary<int,MapData>();
    float minX = 0;
    float maxX = 0;
    float maxZ = 0;
    float minZ = 0;
    float maxY = 0;
    float minY = 0;
    float cubeWidth = 0;
    int rows = 0;
    int cols = 0;
    static public MapUtil GetInstance() {
        if (instance ==null){
            instance = new MapUtil();
        }
        return instance;
    }
    public void LoadMapDataFromXML(string path) {
        XmlDocument xml = new XmlDocument();
        xml.Load(path);
        XmlNode root = xml.SelectSingleNode("Root");
        List<string> sizeInfo = null;
        List<string> mapDataStr =null;
        foreach (XmlElement node in root.ChildNodes) {
            if (node.SelectSingleNode("Title")!=null){
                sizeInfo = FuncUtil.GetInstance().StrToStrList(node.SelectSingleNode("Info").InnerText);
            }
            else{
                mapDataStr = FuncUtil.GetInstance().SplitStrBy(node.InnerText, "|");
            }
        }

       
        
        //SizeInfo
        cubeWidth  = float.Parse(sizeInfo[0]);
        minX = float.Parse(sizeInfo[1]);
        maxX = float.Parse(sizeInfo[2]);
        minY = float.Parse(sizeInfo[3]);
        maxY = float.Parse(sizeInfo[4]);
        minZ = float.Parse(sizeInfo[5]);
        maxZ = float.Parse(sizeInfo[6]);
        rows = (int)((maxZ - minZ) / cubeWidth)+1;
        cols = (int)((maxX - minX) / cubeWidth)+1;

        foreach (var dataStr in mapDataStr) {
            //data 例子： --72.1,38.0;13635,13635;13689,13710
            var strList = FuncUtil.GetInstance().SplitStrBy(dataStr,";");
            //72.1,38.0=>72.1, 54.9, 38.0
            string vecStr = strList[0];
            //计算初始点，暂时将最大Y作为每个点的Y坐标，之后会进行调整
            Vector3 basePoint = FuncUtil.GetInstance().BuildVector3FromStrList(FuncUtil.GetInstance().StrToStrList(vecStr),maxY);
            //get 13635 in 13635,13635;13689,13710
            int baseIndex = int.Parse(FuncUtil.GetInstance().StrToStrList(strList[1])[0]);
            for (int i = 1; i < strList.Count;i++ ) {
                var indexList = FuncUtil.GetInstance().StrToStrList(strList[i]);
                int beginIndex = int.Parse(indexList[0]);
                int endIndex = int.Parse(indexList[1]);
                var tempBasePoint = basePoint+(new Vector3(cubeWidth, 0, 0) * (beginIndex - baseIndex));
                for (int index = beginIndex; index <= endIndex;index++ ) {
                    Vector3 middle = tempBasePoint;
                    var data = new MapData(middle);
                    data.isActive = true;
                    mapDic.Add(index, data);
                    tempBasePoint += new Vector3(cubeWidth, 0, 0);
                }
            }


            //int index = int.Parse(dataStr.Substring(0, dataStr.IndexOf(",")));
            //var pointStr = dataStr.Substring(dataStr.IndexOf("(") + 1, dataStr.LastIndexOf(")") - dataStr.IndexOf("(") - 1);
            //string isActiveStr = dataStr.Substring(dataStr.LastIndexOf(",")+1,dataStr.Length-dataStr.LastIndexOf(",")-1);
            //Vector3 middle = FuncUtil.GetInstance().BuildVector3FromStrList(FuncUtil.GetInstance().StrToStrList(pointStr));
            //var data = new MapData(middle);
            //data.isActive = bool.Parse(isActiveStr);
            //mapDic.Add(index, data);
        }
        Debug.Log("MapData Load Complete"+mapDic.Count);
    }
    
    /// <summary>
    ///   在DEBUG中显示格子点
    /// </summary>
    public void ShowAllMapPoint() {
        var itr = mapDic.GetEnumerator();
        while (itr.MoveNext()) {
            if (itr.Current.Value.isActive) {
                Debug.DrawLine(itr.Current.Value.middle + Vector3.up, itr.Current.Value.middle);
            } else {
                Debug.DrawLine(itr.Current.Value.middle + Vector3.up, itr.Current.Value.middle,Color.red);
            }  
        }
    }
    /// <summary>
    ///   生成路径
    /// </summary>
    public List<Vector3> GeneratePath(Vector3 beginPoint,Vector3 endPoint) {
        List<Vector3> path = new List<Vector3>();
        int beginRow,beginCol,endRow, endCol;
        
        CaculatePointIndex(beginPoint, out beginRow, out beginCol);
        CaculatePointIndex(endPoint, out endRow, out endCol);
        Debug.Log("beginP:" + beginPoint + ":" + GetIndex(beginRow, beginCol));
        Debug.Log("endP:" + endPoint + ":" + GetIndex(endRow, endCol));
        GeneratePath(path, beginRow, beginCol, endRow, endCol);
        
        return path;
    }
    /// <summary>
    ///   计算index，以及行列
    /// </summary>
    private int CaculatePointIndex(Vector3 point,out int row,out int col) {
        col = (int)((point.x - minX) / cubeWidth);
        row = (int)((point.z - minZ) / cubeWidth);
        int index =(row * cols + col);
        //Debug.Log("Point:" + point+"Caculated index = "+index);

        //Debug.Log("Index:" + (row * cols + col) + "corespond point in dic:" + mapDic[index]);
        return index;
    }
    public void CaculatePointIndex(int index, out int row, out int col) {
        CaculatePointIndex(mapDic[index].middle,out row,out col);
    }

    public int GetIndex(int row,int col) {
        return row * cols + col;
    }

    public Vector3 GetPosFromIndex(int row, int col) {
        return GetPosFromIndex(GetIndex(row,col));
    }
    public Vector3 GetPosFromIndex(int index) {
        return mapDic[index].middle;
    }

    
    private void GeneratePath(List<Vector3> path, int beginRow, int beginCol, int endRow, int endCol) {
        var pathSearch = new PathSearch(mapDic,rows,cols);
        var indexList =pathSearch.GeneratePath(beginRow, beginCol, endRow, endCol);
        foreach (var index in indexList) {
            path.Add(GetPosFromIndex(index));
        }
    }
    
    /// <summary>
    ///   校准Y坐标
    /// </summary>
    public void AddjustMapPoint() {
        var itr = mapDic.GetEnumerator();
        while (itr.MoveNext()) {
            var point = itr.Current.Value.middle + new Vector3(0, 10, 0);
            RaycastHit hit;
            if (Physics.Raycast(point, Vector3.down, out hit)) {
                if (hit.transform.tag.Contains("surface")) {
                    itr.Current.Value.middle.y = hit.point.y;
                }
            } else {
                itr.Current.Value.isActive = false;
            }
        }
        //添加障碍
        DisableMapPoint(new Vector3(87.5f, 56.6f, 44.9f), new Vector3(96.4f, 56.6f, 42.5f));
    }
    /// <summary>
    ///   添加障碍
    /// </summary>
    public void DisableMapPoint(Vector3 leftTop,Vector3 rightBottom) {
        int left, right, top, bottom;
        CaculatePointIndex(leftTop,out top,out left);
        CaculatePointIndex(rightBottom, out bottom, out right);
        DisableMapPoint(left, right, top, bottom);
    }
    /// <summary>
    ///   添加障碍
    /// </summary>
    public void DisableMapPoint(int left, int right, int top, int bottom) {
        for (int y = bottom; y <= top; y++) {
            for (int x = left; x <= right; x++) {
                int index = GetIndex(y, x);
                if (mapDic.ContainsKey(index)) {
                    mapDic[index].isActive = false;
                }
            }
        }
    }

    /// <summary>
    ///   判断点是否有效
    /// </summary>
    public bool isPointEffective(Vector3 point) {
        int row, col;
        CaculatePointIndex(point, out row, out col);
        if (!mapDic.ContainsKey(GetIndex(row, col))) {
            Debug.Log("Out of range for Point" + point);
            return false;
        }
        if (!mapDic[GetIndex(row, col)].isActive) {
            Debug.Log("Point:" + point + " Unreachable");
            return false;
        }
        return true;
    }
}

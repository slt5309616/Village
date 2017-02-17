using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;
using System.Xml;
using System;
public class GenerateMapData  {

    [MenuItem("Edit/Export Current Scene")]
    //生成地图依赖信息
    private static void ExportScene() {
        //获取场景中所有go对应的prefab
        GameObject[] goList = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));;
        var itr = goList.GetEnumerator();
        Debug.Log(goList.Length);
        List<UnityEngine.Object> prefabList = new List<UnityEngine.Object>();
        for (int i = 0; itr.MoveNext(); i++) {
            var go = (GameObject)itr.Current;
            if (PrefabUtility.GetPrefabType(go)==PrefabType.PrefabInstance) {
                var obj = PrefabUtility.GetPrefabParent(go);
                if (prefabList.Contains(obj)) {
                    continue;
                }
                prefabList.Add(obj);
            }
        }

        //获取所有prefab路径
        List<string> prefabPathList = new List<string>();
        var itrPrefabList = prefabList.GetEnumerator();
        while (itrPrefabList.MoveNext()) {
            var prefabPath = AssetDatabase.GetAssetPath((UnityEngine.Object)itrPrefabList.Current);
            if (!prefabPathList.Contains(prefabPath)){
                prefabPathList.Add(prefabPath);
            }
        }
       
        //获取依赖并保存
        XmlDocument xml = new XmlDocument();
        xml.AppendChild(xml.CreateElement("Root"));
        itr = prefabPathList.GetEnumerator();
        while (itr.MoveNext()){
            StringBuilder data = new StringBuilder((string)itr.Current);
            string[] dependPathList;
            if (data.ToString().Contains("Effect")) {
                dependPathList = new string[] { data.ToString() };
            } else {
                dependPathList = AssetDatabase.GetDependencies(new string[] { data.ToString() });
            }
            
            AddNodeToXML(xml, data.Replace("Assets/Resources/", "").ToString(), StringArrayToStr(dependPathList));
        }


        xml.Save(Application.dataPath + "/"+Application.loadedLevelName+".xml");

        BuildPipeline.BuildAssetBundles("Assets/AssetsBundle", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }

    private static void AddNodeToXML(XmlDocument xml, string titleValue, string infoValue)
    {
        XmlNode root = xml.SelectSingleNode("Root");
        XmlElement element = xml.CreateElement("Node");
        element.SetAttribute("Type", "string");

        XmlElement titleElelment = xml.CreateElement("Title");
        titleElelment.InnerText = titleValue;

        XmlElement infoElement = xml.CreateElement("Info");
        infoElement.InnerText = infoValue;

        element.AppendChild(titleElelment);
        element.AppendChild(infoElement);
        root.AppendChild(element);
    }
    private static void AddNodeToXML(XmlDocument xml, string infoValue) {
        XmlNode root = xml.SelectSingleNode("Root");
        XmlElement element = xml.CreateElement("Node");
        element.InnerText = infoValue;
        root.AppendChild(element);
    }
    private static string StringArrayToStr(string [] data) {
        if (data.Length<1){
            return null;
        }
        StringBuilder result = new StringBuilder();
        result.Append(data[0]);
        for (int i = 1; i < data.Length;i++ ) {
            result.Append(",");
            result.Append(data[i]);
        }
        result.Replace("Assets/Resources/", "");
        return result.ToString();
    }


    [MenuItem("Assets/Generate Map Data")]
    public static void GenerateMapdata() {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start(); 
        Dictionary<int, MapData> mapDic = new Dictionary<int, MapData>();
        List<Vector3> pointList = new List<Vector3>();
        var goList = Selection.gameObjects;
        foreach (var go in goList) {
            Vector3 pos = go.transform.position;
            Quaternion rot = go.transform.rotation;
            Vector3 scale = go.transform.localScale;
            //获取网格定点并平移和旋转
            foreach (var point in go.GetComponent<MeshFilter>().sharedMesh.vertices) {
                Vector3 p = new Vector3(point.x * scale.x, point.y * scale.y, point.z * scale.z);
                pointList.Add(rot * p + pos);
            }
        }
        //一些值的初始化
        float cubeWidth = 1f;
        Vector3 lV = new Vector3();
        Vector3 rV = new Vector3();
        Vector3 tV = new Vector3();
        Vector3 bV = new Vector3();
        float minX = pointList[0].x;
        float maxX = pointList[0].x;
        float maxZ = pointList[0].z;
        float minZ = pointList[0].z;
        float maxY = pointList[0].y;
        float minY = pointList[0].y;

        //遍历获取边界
        for (int i = 0; i < pointList.Count; i++) {
            var vertic = pointList[i];
            if (vertic.x<minX) {
                minX=vertic.x;
                lV = vertic;
            }
            if (vertic.x >maxX) {
                maxX = vertic.x;
                rV = vertic;
            }
            if (vertic.z < minZ) {
                minZ =vertic.z;
                bV = vertic;
            }
            if (vertic.z > maxZ) {
                maxZ = vertic.z;
                tV = vertic;
            }
            if (vertic.y >maxY){
                maxY = vertic.y;
            }
            if (vertic.y < minY) {
                minY = vertic.y;
            }
        }
        Debug.Log(lV + "/n" + rV + "/n" + tV + "/n" + bV);
        int rows = (int)((maxZ - minZ) / cubeWidth) + 1;
        int cols = (int)((maxX - minX) / cubeWidth) + 1;

        int totalNum = 0;
        for (float j = minZ + cubeWidth / 2f; j < maxZ; j += cubeWidth) {
            for (float i = minX + cubeWidth / 2f; i < maxX; i += cubeWidth) {
                MapData data = new MapData(new Vector3(i + cubeWidth / 2f, maxY, j + cubeWidth / 2f));
                
                int col = (int)((data.middle.x - minX - cubeWidth / 2f) / cubeWidth);
                int row = (int)((data.middle.z - minZ - cubeWidth / 2f) / cubeWidth);
                int index = (row * cols + col);
                //Debug.Log(data.middle+":"+index);
                if (mapDic.ContainsKey(index)){
                    Debug.Log("Index exist for"+index+":"+mapDic[index].middle);
                }
                mapDic.Add(index, data);
                totalNum++;
            }
        }

        //通过ray的碰撞来标记有效
        var itr = mapDic.GetEnumerator();
        while (itr.MoveNext()) {
            var point = itr.Current.Value.middle + new Vector3(0, 10, 0);
            RaycastHit hit;
            
            if (Physics.Raycast(point, Vector3.down, out hit)) {
                if (hit.transform.tag.Contains("surface")) {
                    Debug.DrawLine(hit.point+Vector3.up, hit.point,Color.red);
                    itr.Current.Value.middle.y = hit.point.y;
                    itr.Current.Value.isActive = true;
                }
            }
        }
        //删除无效点
        for (int i = 0; i < totalNum; i++) {
            if (mapDic[i].isActive == false) {
                mapDic.Remove(i);
            }
        }
        Debug.Log(mapDic.Count);

        itr = mapDic.GetEnumerator();
        while (itr.MoveNext()) {
            Debug.DrawLine(itr.Current.Value.middle + Vector3.up, itr.Current.Value.middle);
        }


        XmlDocument xml = new XmlDocument();
        xml.AppendChild(xml.CreateElement("Root"));
        AddNodeToXML(xml, "SizeInfo", cubeWidth.ToString() + "," + minX.ToString() + "," + maxX.ToString() + "," + minY.ToString() + "," + maxY.ToString() + "," + minZ.ToString() + "," + maxZ.ToString());

        CompressAndSaveData(xml,mapDic);

        string scenePath = EditorApplication.currentScene;
        xml.Save(Application.dataPath + "/" + FuncUtil.GetInstance().GetFileNameByAssetName(scenePath) + "Map.xml");
        stopwatch.Stop();
        System.TimeSpan timespan = stopwatch.Elapsed;
        double milliseconds = timespan.TotalMilliseconds;
        Debug.Log(milliseconds);
    }
    private static void CompressAndSaveData(XmlDocument xml,Dictionary<int, MapData> mapDic) {
        
        
        int num = 0;//处理了的data的数量,用来判断是否遇到map结尾
        int startIndex = 0;
        int endIndex = 0;
        Vector3 startPoint = Vector3.zero;
        float curZ=0f;
        var itr = mapDic.GetEnumerator();
        StringBuilder totalStr = new StringBuilder();
        StringBuilder str = null;
        while (itr.MoveNext()) {
            //如果换行或者到了末尾
            if (curZ!=itr.Current.Value.middle.z||num==mapDic.Count) {
                if (curZ!=0){
                    str.Append(startIndex.ToString() + "," + endIndex.ToString());
                    totalStr.Append(str.ToString() + "|");
                }
                //数值初始化
                curZ = itr.Current.Value.middle.z;           
                startIndex = 0;
                endIndex = 0;
                startPoint = itr.Current.Value.middle;                
                str = new StringBuilder();

                Vector2 xzCoordinate = new Vector2(startPoint.x,startPoint.z);
                //将起始点信息写入str
                str.Append(FuncUtil.GetInstance().RemoveStrInStr(xzCoordinate.ToString(), new List<string>() { "(",")"}) + ";");
            }
            //Not the End of Line
            //如果当前行刚开始，设置startIndex
            if (startIndex == 0) {
                startIndex = itr.Current.Key;
                endIndex = itr.Current.Key;
            } else {
                //当前行已经开始
                //如果两个点是相邻的，即其中间没有因为空缺点被删除出现间隔
                //endIndex设置为当前index
                //否则就遇到了空点，把当前的有效段写入当前字符串段，并设置新的startIndex
                if ((itr.Current.Key - endIndex) == 1) {
                    endIndex = itr.Current.Key;
                } else {
                    str.Append(startIndex.ToString() + "," + endIndex.ToString() + ";");
                    startIndex = itr.Current.Key;
                    endIndex = itr.Current.Key;
                }

            }
            num++;
        }
        AddNodeToXML(xml, totalStr.ToString().Remove(totalStr.ToString().LastIndexOf("|")));
    }
}

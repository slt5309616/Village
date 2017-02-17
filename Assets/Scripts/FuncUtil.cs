using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class FuncUtil  {

    static public FuncUtil instance = null;
    public static FuncUtil GetInstance() {
        if (instance == null) {
            instance = new FuncUtil();
        }
        return instance;
    }
    /// <summary>
    ///   判断asset是否是图片
    /// </summary>
    public bool IsAssetTexture(string assetName) {
        return assetName.Contains(".tga") || assetName.Contains(".dds") || assetName.Contains(".png") || assetName.Contains(".jpg");
    }
    /// <summary>
    ///   通过相对路径名获取文件名
    ///   Eg.Scene/Prefabs/Prefab_10000_603002.prefab=>Prefab_10000_603002
    /// </summary>
    public string GetFileNameByAssetName(string name) {
        int begin = name.LastIndexOf("/");
        int end = name.LastIndexOf(".");
        return name.Substring(begin + 1, end - begin - 1);
    }
    /// <summary>
    ///   使用“，”将一个str，分割为List
    /// </summary>
    public List<string> StrToStrList(string str) {
        if (str.Length <= 1) {
            return null;
        }
        List<string> result = new List<string>();
        int begin = -1;
        int end = 0;
        while (end != str.Length) {
            end = str.IndexOf(",", begin + 1);
            //reach the last one?
            if (end == -1 && begin != (str.Length - 1)) {
                end = str.Length;
            }
            result.Add(str.Substring(begin + 1, end - begin - 1));
            begin = end;
        }
        return result;
    }
    /// <summary>
    ///   使用split将一个str，分割为List
    /// </summary>
    public List<string> SplitStrBy(string str,string split) {
        if (str.Length <= 1) {
            return null;
        }
        List<string> result = new List<string>();
        int begin = -1;
        int end = 0;
        while (end != str.Length) {
            end = str.IndexOf(split, begin + 1);
            //reach the last one?
            if (end == -1 && begin != (str.Length - 1)) {
                end = str.Length;
            }
            result.Add(str.Substring(begin + 1, end - begin - 1));
            begin = end;
        }
        return result;
    }
    /// <summary>
    ///  通过List生成vector3
    ///  如果List.count不为3则返回空
    /// </summary>
    public Vector3 BuildVector3FromStrList(List<string> strList) {
        if (strList.Count!=3){
            StringBuilder str = new StringBuilder();
            foreach (var s in strList){
                str.Append(s);
            }
            Debug.Log("Build str from List"+str.ToString()+"error");
        }
        return new Vector3(float.Parse(strList[0]),float.Parse(strList[1]),float.Parse(strList[2]));
    }
    /// <summary>
    ///  通过List生成vector3
    ///  如果指定Y坐标，则List中包含x，z
    /// </summary>
    public Vector3 BuildVector3FromStrList(List<string> strList,float y) {
        if (strList.Count != 2) {
            StringBuilder str = new StringBuilder();
            foreach (var s in strList) {
                str.Append(s);
            }
            Debug.Log("Build Vector3 from List(x,z)" + str.ToString() + "error");
        }
        return new Vector3(float.Parse(strList[0]),y, float.Parse(strList[1]));
    }
    public string RemoveStrInStr(string mainStr,List<string> removeStrList) {
        StringBuilder data = new StringBuilder(mainStr);
        foreach (var str in removeStrList){
            data.Remove(data.ToString().IndexOf(str), str.Length);
        }
        return data.ToString();
    }
}

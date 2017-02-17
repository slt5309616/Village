using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
///   单独加载测试
/// </summary>
public class LoadTest : MonoBehaviour {

    List<string> urlList = new List<string> {"file://E:/workspace/Village/Assets/AssetsBundle/preload/shader/shaderinit.ab",
                                        "file://E:/workspace/Village/Assets/AssetsBundle/scene/textures/texture_60601_604017.ab",
                                        "file://E:/workspace/Village/Assets/AssetsBundle/scene/textures/texture_60201_602019.ab",
                                        "file://E:/workspace/Village/Assets/AssetsBundle/scene/models/materials/texture_10100_604017.ab",
                                     "file://E:/workspace/Village/Assets/AssetsBundle/scene/models/materials/texture_60601_604017.ab",
                                    "file://E:/workspace/Village/Assets/AssetsBundle/scene/prefabs/prefab_10100_604017.ab"};

    List<WWW> wwwReady = new List<WWW>();
    List<WWW> wwwDone = new List<WWW>();
    int loadedIndex = 0;
    int index = 0;
	// Use this for initialization
	void Start () {
        StartCoroutine(LoadAB("file://"+Application.dataPath));
	}
    void Update() {
        if (wwwReady.Count>0&&wwwReady[0].isDone) {
            
            Debug.Log( wwwReady[0].url+"  is done");

            if (wwwReady[0].url != urlList[5]) {
                //wwwReady[0].assetBundle.LoadAllAssets();
            }
            loadedIndex++;
            wwwDone.Add(wwwReady[0]);
            wwwReady.Remove(wwwReady[0]);
        }
        if ((loadedIndex<6)&&(wwwReady.Count == 0)){
            WWW www = new WWW(urlList[loadedIndex]);
            wwwReady.Add(www); 
        }
        if (loadedIndex==6) {

            var obj = wwwDone[5].assetBundle.LoadAsset("Prefab_10100_604017");
            GameObject go = (GameObject)Instantiate(obj);
            foreach (var www in wwwDone) {
                Debug.Log(www.url+"|||||Error:"+www.error+"|||||Size:"+www.size);
            }
            loadedIndex++;
        }
    }
    IEnumerator LoadAB(string path) {


        WWW www1 = new WWW(path + "/AssetsBundle/AssetsBundle");
        yield return www1;

        var abmanifast = www1.assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        var depends = abmanifast.GetAllDependencies("unit_121001_test.ab");
        AssetBundle[] abArray = new AssetBundle[depends.Length];
        for (int i = 0; i < depends.Length; i++) {
            var tempWWW = new WWW(path + depends[i]);
            abArray[i] = tempWWW.assetBundle;
        }

        WWW www = new WWW(path + "/AssetsBundle/prefabs/" + "unit_121001_test.ab");
        yield return www;
        var obj = www.assetBundle.LoadAsset("Unit_121001_Test");
        yield return obj = Instantiate(obj);

        www.assetBundle.Unload(false);
        www1.assetBundle.Unload(false);
        for (int i = 0; i < abArray.Length; i++) {
            abArray[i].Unload(false);
        }
    }
}

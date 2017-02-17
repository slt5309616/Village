using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneConfig {
    static public SceneConfig instance = null;
    /// <summary>
    ///   wwwReady最大size
    /// </summary>
    public int maxListSize = 10;
    /// <summary>
    ///   场景数
    /// </summary>
    public int maxScenes = 2;
    public int currentSceneIndex = 0;
    public List<string> sceneNameList = new List<string> { "Load", "Load2" };
    public Dictionary<string, string> sceneConfigList = new Dictionary<string, string> { { "Load", "10000.xml" }, { "Load2", "10100.xml" } };
    private Dictionary<int, Vector3> playerInitPos = new Dictionary<int, Vector3> { { 0, new Vector3(55.85f, 62.3f, 75.05389f) }, { 1, new Vector3(12.80437f, 90.34858f, 69.53145f) } };
    private string _mapDataPathPrefix = Application.dataPath;
    private string _mapDataPathSuffix = "Map.xml";
    public static SceneConfig GetInstance() {
        if (instance==null){
            instance = new SceneConfig();
        }
        return instance;
    }
    
    private string _prefix = "http://static.m16.mingchao.com/";
    private string _suffix = ".ab?v=111165";
    private SceneConfig() {
        currentSceneIndex = Application.loadedLevel;
    }
    public string prefix {
        get {
            return _prefix;
        }
        private set {
        }
    }
    public string suffix {
        get {
            return _suffix;
        }
        private set {
        }
    }
    public string mapDataPath {
        get {
            return _mapDataPathPrefix + "/" + sceneNameList[currentSceneIndex] + _mapDataPathSuffix;
        }
        private set {

        }
    }
    public  int GetNextSceneIndex() {
        return currentSceneIndex++ == (maxScenes - 1) ? currentSceneIndex =0: currentSceneIndex;
    }
    public string GetNextSceneName() {
        var index = currentSceneIndex++ == (maxScenes - 1) ? currentSceneIndex = 0 : currentSceneIndex;
        return sceneNameList[index];
    }
    public Vector3 GetCurrentScenePlayerPos() {
        return playerInitPos[currentSceneIndex];
    }
}

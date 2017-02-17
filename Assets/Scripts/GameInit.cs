using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;
using UnityEditor;

public class GameInit : MonoBehaviour {
    private string sceneName;
    private string sceneID;
    enum HeroState {
        HERO_IDLE,
        HERO_RUN,
        HERO_RUN_Left,
        HERO_RUN_RIGHT,
        HERO_WALK
    }
    struct GoConfig{
        public string unitType;
        public string id;
        public string occid;
        public string name;
        public string tag;
        public bool isSurface;
        public bool isPerLoad;
        public string asset;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Transform parent;
        public XmlElement node;
    }


    private string assetXmlPath = "";
    private HeroState gameState ;
    private GameObject goEffects;
    private GameObject goMap;
    private GameObject surfaceLayer;
    private GameObject preloadLayer;
    private GameObject otherLayer;
    private GameObject goPlayer;
    private Animator playerCtrl;
    private Vector3 cameraVelocity = Vector3.zero;
    private Vector3 tarPoint;
    private float speed = 0.1f;
    private Vector3 cameraOffset = new Vector3(0, 40, -40);



    private Dictionary<string,List<string>> assetsDependDic;
    /// <summary>
    ///   正在加载的WWW列表
    /// </summary>
    private List<WWW> wwwReady = new List<WWW>();
    /// <summary>
    ///   加载完成的WWW列表{url，WWW}
    /// </summary>
    private Dictionary<string, WWW> wwwDone = new Dictionary<string, WWW>();
    /// <summary>
    ///   所有需要加载的url列表
    /// </summary>
    private List<string> assetWaitingList = new List<string>() {};
    /// <summary>
    ///   保存所有go，key：occid
    /// </summary>
    private Dictionary<string, GameObject> goDict = new Dictionary<string, GameObject>();
    /// <summary>
    ///   保存所有go的config，key：occid
    /// </summary>
    private Dictionary<string, GoConfig> configDict = new Dictionary<string, GoConfig>();
    /// <summary>
    ///   总资源量，因为手动在最开始加载了本地的shader.ab，所以初始值为1
    /// </summary>
    private int totalAssetNum=1;
    /// <summary>
    ///   保存所有prefab的资源名，{url,asset}
    /// </summary>
    private Dictionary<string, string> mainAsseturlDict = new Dictionary<string, string>(); //{url,asset}
    /// <summary>
    ///   待加载texture 的url列表
    /// </summary>
    private List<string> textureWaitingList = new List<string>();
    /// <summary>
    ///   待加载material 的url列表
    /// </summary>
    private List<string> matWaitingList = new List<string>();
    /// <summary>
    ///   待加载prefab 的url列表
    /// </summary>
    private List<string> prefabWaitingList = new List<string>();
    /// <summary>
    ///   光照贴图WWW列表
    /// </summary>
    private Dictionary<string, WWW> lightMapList = new Dictionary<string, WWW>();
    private bool isLoadTextureDone = false;
    private bool isLoadMatDone = false;
    private bool isPlayerInitialed = false;
    /// <summary>
    ///   寻路路径结果
    /// </summary>
    private List<Vector3> path = new List<Vector3>();
    private string shaderUrl = "file://E:/workspace/Village/Assets/AssetsBundle/preload/shader/shaderinit.ab";

	void Start () {
        wwwReady.Add(new WWW(shaderUrl));
        
        InitGameScene();
        MapUtil.GetInstance().LoadMapDataFromXML(SceneConfig.GetInstance().mapDataPath);

	}
    private void InitGameScene() {

        assetsDependDic = new Dictionary<string, List<string>>();
        //assetsLoadedDic = new Dictionary<string, bool>();
        assetXmlPath = Application.dataPath + "/" + SceneConfig.GetInstance().sceneConfigList[Application.loadedLevelName];
        LoadAssetsXml();

        XmlDocument xml = new XmlDocument();
        xml.Load(Application.dataPath + "/Resources/XMLLib/Map/Map_" + SceneConfig.GetInstance().sceneConfigList[Application.loadedLevelName]);
        XmlNodeList rootNode = xml.SelectSingleNode("root").ChildNodes;
        XmlElement sceneNode = (XmlElement)rootNode.Item(0);
        sceneName = sceneNode.GetAttribute("name");
        sceneID= sceneNode.GetAttribute("ID");
        var pathsLightMapFar = FuncUtil.GetInstance().StrToStrList(sceneNode.GetAttribute("pathLightMapFar"));
        var pathsLightMapNear = FuncUtil.GetInstance().StrToStrList(sceneNode.GetAttribute("pathLightMapNear"));

        LoadLightMapFromHttp(pathsLightMapFar, pathsLightMapNear);

        goEffects = new GameObject("Effects");
        goMap = new GameObject("Map");
        surfaceLayer = new GameObject("SurfaceLayer");
        surfaceLayer.transform.parent = goMap.transform;
        preloadLayer = new GameObject("PerloadLayer");
        preloadLayer.transform.parent = goMap.transform;
        otherLayer = new GameObject("OtherLayer");
        otherLayer.transform.parent = goMap.transform;

        foreach (XmlElement node in sceneNode) {
            if (node.Name == "Render") {
                LoadRender(node);
            }
            if (node.Name == "Map"){
                LoadMapFromXML(node);
            }
            if (node.Name == "Effects") {
                LoadEffectFromXML(node, false);
            }
        }
    }

    private void  LoadAssetsXml() {


        XmlDocument xml = new XmlDocument();
        xml.Load(assetXmlPath);
        XmlNode root= xml.SelectSingleNode("Root");
        foreach (XmlElement node in root.ChildNodes) {
            var assetDependInfo = node.SelectSingleNode("Info");
            var assetsList = FuncUtil.GetInstance().StrToStrList(assetDependInfo.InnerText);
            
            var assetsName = node.SelectSingleNode("Title");
            assetsDependDic.Add(assetsName.InnerText, assetsList);
        }
    }
    //split a str into strs by ','


    private void LoadRender(XmlElement node) {

        RenderSettings.fog = node.GetAttribute("fog")=="True"?true:false;

        List<string> temp;
        temp = FuncUtil.GetInstance().StrToStrList(node.GetAttribute("fogColor"));
        RenderSettings.fogColor = new Color(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
        RenderSettings.fogMode = (FogMode)int.Parse(node.GetAttribute("fogMode"));
        RenderSettings.fogDensity = float.Parse(node.GetAttribute("fogDensity"));
        RenderSettings.fogStartDistance = float.Parse(node.GetAttribute("fogStartDistance"));
        RenderSettings.fogEndDistance = float.Parse(node.GetAttribute("fogEndDistance"));
        temp = FuncUtil.GetInstance().StrToStrList(node.GetAttribute("ambientLight"));
        RenderSettings.ambientLight = new Color(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
        RenderSettings.haloStrength = float.Parse(node.GetAttribute("haloStrength"));
        RenderSettings.flareStrength = float.Parse(node.GetAttribute("flareStrength"));
    }
    private void LoadLightMap(List<string> pathsLightMapFar,List<string> pathsLightMapNear) {

        if (pathsLightMapFar.Count!=0){
            LightmapData[] maps = new LightmapData[pathsLightMapFar.Count];
            for (int i =0;i<pathsLightMapFar.Count;i++){
                var str =  pathsLightMapFar[i];
                LightmapData data = new LightmapData();
                data.lightmapFar = Resources.Load(str.Substring(0, str.IndexOf("."))) as Texture2D;
                maps[i] = data;
            }
            LightmapSettings.lightmaps = maps;
        }
        if (pathsLightMapNear.Count != null) {

        }

        LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
    }
    private void LoadLightMapFromHttp(List<string> pathsLightMapFar, List<string> pathsLightMapNear) {
        if (pathsLightMapFar.Count != 0) {
            for (int i = 0; i < pathsLightMapFar.Count; i++) {
                var url = SceneConfig.GetInstance().prefix + pathsLightMapFar[i].Substring(0, pathsLightMapFar[i].IndexOf(".")).ToLower() + SceneConfig.GetInstance().suffix;
                lightMapList.Add(url,new WWW(url));
                totalAssetNum++;
            }
        }
    }
    private void LoadMapFromXML(XmlElement node) {

        foreach (XmlElement go in node) {
            var t = go.GetAttribute("asset");
            //Debug.Log("Start Load  " + t);
            LoadGameObject(go);
        }
    }
    private void LoadEffectFromXML(XmlElement node,bool hasParent) {
       
        foreach (XmlElement go in node.ChildNodes){
            LoadGameObject(go, goEffects.transform);
        }
    }


    private void LoadGameObject(XmlElement node,Transform parent =null) {

        GoConfig config = new GoConfig();
        config.name = node.GetAttribute("name");
        config.occid = node.GetAttribute("occid");
        config.isPerLoad = node.GetAttribute("isPerLoad") == "True" ? true : false;
        config.isSurface = node.GetAttribute("isSurface") == "True" ? true : false;
        config.asset = node.GetAttribute("asset");
        config.parent = parent;
        config.id = node.GetAttribute("id");
        config.unitType = node.GetAttribute("unitType");
        config.node = node;
        config.tag = node.GetAttribute("tag");
        List<string> temp;
        temp = FuncUtil.GetInstance().StrToStrList(node.GetAttribute("position"));
        config.position = new Vector3(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
        temp = FuncUtil.GetInstance().StrToStrList(node.GetAttribute("rotation"));
        config.rotation = new Quaternion(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]), float.Parse(temp[3]));
        temp = FuncUtil.GetInstance().StrToStrList(node.GetAttribute("scale"));
        config.scale = new Vector3(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
        configDict.Add(config.occid, config);
        //Debug.Log("Load GO " + config.occid);
        LoadAsset(config.asset, config.occid); 
    }

    //全部texture和mat放进各自的list里面
    void LoadAsset(string asset, string occid) {
        var depends = assetsDependDic[asset];
        for (int i = 0; i < depends.Count;i++ ) {
            
            //剔除shader
            if (!depends[i].Contains("shader") && !depends[i].Contains("FBX")) {
                var url = SceneConfig.GetInstance().prefix + depends[i].Substring(0, depends[i].IndexOf(".")).ToLower() + SceneConfig.GetInstance().suffix;
                if (!assetWaitingList.Contains(url)){
                    assetWaitingList.Add(url);
                    totalAssetNum++;
                    //根据assetName将url放进对应的list
                    if (FuncUtil.GetInstance().IsAssetTexture(depends[i])) {
                        textureWaitingList.Add(url);
                    }
                    if (depends[i].Contains(".mat")) {
                        matWaitingList.Add(url);
                    }
                    if (depends[i] == asset) {
                        prefabWaitingList.Add(url);
                        mainAsseturlDict.Add(asset, url);

                    }
                }
            } 
        }                                   
    }
    /// <summary>
    ///   创建www
    /// </summary>
    void GetWWW(string url) {
        WWW www = new WWW(url);  
        wwwReady.Add(www);

    }
    void Update() {
        MapUtil.GetInstance().ShowAllMapPoint();

        if (textureWaitingList.Count == 0 && wwwReady.Count == 0) {
            isLoadTextureDone = true;
        }
        if (matWaitingList.Count == 0 && wwwReady.Count == 0) {
            isLoadMatDone = true;
        }
        if (assetWaitingList.Count>0&&wwwReady.Count<SceneConfig.GetInstance().maxListSize){
            string url = null;
            if (textureWaitingList.Count>0){
                url = textureWaitingList[0];
                textureWaitingList.Remove(url);
            } else {
                if (isLoadTextureDone) {
                    if (matWaitingList.Count > 0) {
                        url = matWaitingList[0];
                        matWaitingList.Remove(url);
                    } else {
                        if (isLoadMatDone){
                            if (prefabWaitingList.Count > 0) {
                                url = prefabWaitingList[0];
                                prefabWaitingList.Remove(url);
                            }
                        }
                    }
                }
            }
            if (url!=null){;
                assetWaitingList.Remove(url);
                GetWWW(url);
            }
        }
        //遍历LightMapList，将其中已经完成的www放入done列表
        var itr = lightMapList.GetEnumerator();
        while (itr.MoveNext()){
            if (itr.Current.Value.isDone){
                if (!wwwDone.ContainsKey(itr.Current.Key)) {
                    wwwDone.Add(itr.Current.Key, itr.Current.Value);
                }
            }
        }

        //遍历ready列表，如果加载完成，放入wwwDone
        for (int i = 0; i < wwwReady.Count;i++ ) {
            if (wwwReady[i].isDone){
                if (!string.IsNullOrEmpty(wwwReady[i].error)) {
                    Debug.Log(wwwReady[i].url + wwwReady[i].error);
                }
                var www = wwwReady[i];
                Debug.Log("Add Url to wwwDone " + www.url);
                wwwDone.Add(www.url,www);
                wwwReady.Remove(www);
            }
        }
        //所有资源全部加载完成，创建地图
        if(wwwDone.Count == totalAssetNum) {
            SetLightMap();
            Debug.Log("wwwDone total :"+wwwDone.Count);
            InitialGameObject();
            MapUtil.GetInstance().AddjustMapPoint();
            SetPlayer();
        }
        if (Input.GetMouseButtonDown(0)) {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if (!hit.transform.tag.Contains("surface")) {
                    return;
                }
                

                if (MapUtil.GetInstance().isPointEffective(hit.point)){
                    path = MapUtil.GetInstance().GeneratePath(goPlayer.transform.position, hit.point);
                    if (path.Count > 0) {
                        SetGameState(HeroState.HERO_RUN);
                    }
                }
                
            }
        }
        //调整摄像机
        if (isPlayerInitialed){
            transform.position = Vector3.SmoothDamp(transform.position, goPlayer.transform.position + cameraOffset, ref cameraVelocity, 0.01f);
            transform.LookAt(goPlayer.transform.position);
        }
        //右键换图
        if (Input.GetMouseButtonDown(1)) {
            LoadNextScene();
        }
    }
    void SetLightMap() {
        int i = 0;
        var itr = lightMapList.GetEnumerator();
        LightmapData[] maps = new LightmapData[lightMapList.Count];
        while(itr.MoveNext()){
            LightmapData data = new LightmapData();
            data.lightmapFar = itr.Current.Value.assetBundle.LoadAllAssets<Texture2D>()[0];
            maps[i++] = data;
        }
        LightmapSettings.lightmaps = maps;
        LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
    }
    void InitialGameObject() {

        foreach (var www in wwwDone) {
            www.Value.assetBundle.LoadAllAssetsAsync();
        }

        var itr = configDict.GetEnumerator();
        while(itr.MoveNext()) {
            GoConfig config= itr.Current.Value;
            if (mainAsseturlDict.ContainsKey(config.asset)) {
                //从mainAsseturlDict得到需要创建的go的prefab的url
                string assetUrl = mainAsseturlDict[config.asset];
                //从wwwDine中得到对应url的www
                var obj = wwwDone[assetUrl].assetBundle.LoadAsset(config.name);

                
                //Debug.Log("Initial|" + config.asset + "|" + occid + "|" + assetUrl);
                GameObject gameObj = (GameObject)Instantiate(obj, config.position, config.rotation);
                var occid = itr.Current.Key;
                goDict.Add(occid, gameObj);
                //Debug.Log("Add occid to goDic " + occid);
                
                gameObj.transform.localScale = config.scale;
                gameObj.name = config.name;
                gameObj.tag = config.tag;
                if (config.isSurface) {
                    gameObj.transform.parent = surfaceLayer.transform;
                } else {
                    if (config.isPerLoad) {
                        gameObj.transform.parent = preloadLayer.transform;
                    } else {
                        gameObj.transform.parent = otherLayer.transform;
                    }
                }
                if (config.parent != null) {
                    gameObj.transform.parent = config.parent;
                }

                if (config.node.ChildNodes.Count > 0) {
                    foreach (XmlElement property in config.node.ChildNodes) {
                        if (property.Name == "Renderers") {
                            LoadRenderToObj(property, gameObj);
                        }
                        if (property.Name == "Effects") {
                        }
                    }
                }
            } else {
                Debug.Log(config.asset + "is not in mainAsseturlLst");
            }
        }
        totalAssetNum = 0;
        var itrWWW = wwwDone.GetEnumerator();
        while (itrWWW.MoveNext()) {
            itrWWW.Current.Value.assetBundle.Unload(false);
        }

    }

    private void SetPlayer() {
        gameState = HeroState.HERO_IDLE;
        goPlayer = (GameObject)Instantiate(Resources.Load("Prefabs/Unit_121001_Test"), SceneConfig.GetInstance().GetCurrentScenePlayerPos(), new Quaternion(0, 0, 0, 0));
        playerCtrl = goPlayer.GetComponent<Animator>();
        isPlayerInitialed = true;
    }
    /// <summary>
    ///   设置render
    /// </summary>
    private void LoadRenderToObj(XmlElement node, GameObject go) {

        Debug.Log("setting render to "+go.name);
        foreach (XmlElement renderNode in node.ChildNodes) {
            string renderName = renderNode.GetAttribute("name");
            bool receiverShadows = renderNode.GetAttribute("receiveShadows") == "True" ? true : false;

            List<string> temp;
            temp = FuncUtil.GetInstance().StrToStrList(renderNode.GetAttribute("boundsMin"));
            Vector3 boundsMin = new Vector3(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
            temp = FuncUtil.GetInstance().StrToStrList(renderNode.GetAttribute("boundsMax"));
            Vector3 boundsMax = new Vector3(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
            int lightmapIndex = int.Parse(renderNode.GetAttribute("lightmapIndex"));
            temp = FuncUtil.GetInstance().StrToStrList(renderNode.GetAttribute("lightmapTilingOffset"));
            Vector4 lightmapTilingOffset = new Vector4(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]), float.Parse(temp[3]));

            
            Renderer render;
            //动画文件应该配置的是其子节点的render
            if (go.name.Contains("Anim")) {
                render = go.transform.Find(renderName).GetComponent<Renderer>();
            } else {
                render = go.GetComponent<Renderer>();
            }
            render.lightmapIndex = lightmapIndex;
            render.lightmapScaleOffset = lightmapTilingOffset;
            render.receiveShadows = receiverShadows;
            render.bounds.SetMinMax(boundsMin, boundsMax);

        }
    }
    void LoadNextScene() {
        var sceneName = SceneConfig.GetInstance().GetNextSceneName();
        Application.LoadLevel(sceneName);
    }
    void FixedUpdate() {
        switch (gameState) {
            case HeroState.HERO_RUN:
                Move(speed);
                break;
            default:
                break;
        }
    }
    void SetGameState(HeroState state) {
        switch (state) {
            case HeroState.HERO_IDLE:
                //播放站立动画
                playerCtrl.CrossFade("Idle", 0.1f);
                break;
            case HeroState.HERO_RUN:
                //播放奔跑动画
                playerCtrl.CrossFade("Run", 0.1f);
                break;
        }
        gameState = state;
    }
    void Move(float speed) {
        if (path.Count>0){
            tarPoint = path[path.Count-1];
            goPlayer.transform.LookAt(tarPoint);
        } else {
            SetGameState(HeroState.HERO_IDLE);
        }
        
        if (Mathf.Abs(Vector3.Distance(tarPoint, goPlayer.transform.position)) ==0) {
            goPlayer.transform.position = Vector3.MoveTowards(goPlayer.transform.position, tarPoint, speed);
        } else {
            path.Remove(tarPoint);
        }
    }

    string GetOccidByFileName(string name){
        var itr = configDict.GetEnumerator();
        while (itr.MoveNext()){
            if (itr.Current.Value.name==name){
                return itr.Current.Value.occid;
            }
        }
        Debug.Log("No occid for" + name);
        return null;
    }

}

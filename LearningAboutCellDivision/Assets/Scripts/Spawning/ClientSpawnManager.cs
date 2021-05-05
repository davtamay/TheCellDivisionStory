using BzKovSoft.ObjectSlicerSamples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClientSpawnManager : SingletonComponent<ClientSpawnManager>
{

    public static ClientSpawnManager Instance
    {
        get { return ((ClientSpawnManager)_Instance); }
        set { _Instance = value; }
    }

    [Header("Current User Setup")]
    public GameObject _mainPlayer;
    public EntityData_SO _mainClient_entityData;
    public Scene_List sceneList;
    public string _UI_Teacher_Display_sceneName;
    public string _UI_Student_Display_sceneName;


    public EntityData_SO mainPlayer_head;
    public EntityData_SO mainPlayer_L_Hand;
    public EntityData_SO mainPlayer_R_Hand;

    [Header("Spawn_Setup")]
    public Transform _CenterToSpawnClients;
    public int _clientReserveCount;
    public float _spreadRadius;

    [Header("External User Setup")]
    public GameObject _otherPlayersDummyPrefab;
    public Dictionary<GameObject, EntityData_SO> _clientDict;
    public Dictionary<uint, Non_MainClientData> _availableClientIDToGODict = new Dictionary<uint, Non_MainClientData>();

    [Header("List of Reserved & Current Clients")]

    public List<uint> _client_ID_List = new List<uint>();

    public List<GameObject> _availableGOList = new List<GameObject>();


    public List<Non_MainClientData> _ExternalClientList = new List<Non_MainClientData>();

    public List<Text> _clientUser_Names_UITextReference = new List<Text>();
    public List<Text> _clientUser_Dialogue_UITextReference = new List<Text>();

    private int currentClientCount;

    //#region NETWORK OBJECTS
    [Header("List of Network Objects")]
    public List<GameObject> allNetWork_GO_upload_list = new List<GameObject>();

    public Dictionary<int, Net_Register_GameObject> _EntityID_To_NetObject = new Dictionary<int, Net_Register_GameObject>();
    public Dictionary<int, int> _EntityID_to_urlListIndex = new Dictionary<int, int>();
    public Dictionary<int, int> _urlListIndex_to_EntityID = new Dictionary<int, int>();

    public Dictionary<int, Net_Register_GameObject> _indexURLList_To_NetObject;

    public String_List listOfObjects;

    public bool isSpawningFinished;
    public bool isURL_LoadingFinished = false;

    //public IEnumerator Start()
    //{
    //  //  SceneManager.LoadSceneAsync(3, LoadSceneMode.Additive);

    //    yield return StartCoroutine(Instantiate_Reserved_Clients());

    //    //RESET PREVIOUS DATA
    //    _mainClient_entityData.rot = new Vector4();
    //    _mainClient_entityData.pos = Vector3.zero;

    //    _mainPlayer = GameObject.FindGameObjectWithTag("Player");

    //    //REGISTER ACTIVE CLIENT
    //    //#if !UNITY_EDITOR && UNITY_WEBGL
    //    Client_Refresh(new Coords(NetworkUpdateHandler.Instance.client_id, 0, (int)Entity_Type.main_Player, 1, Vector3.zero, Quaternion.identity));

    //    //Setup Client ID;
    //    mainPlayer_head.clientID = (uint) NetworkUpdateHandler.Instance.client_id;
    //    mainPlayer_head.clientID = (uint)NetworkUpdateHandler.Instance.client_id;
    //    mainPlayer_head.clientID = (uint)NetworkUpdateHandler.Instance.client_id;

    //    //Setup Entity ID
    //    mainPlayer_head.entityID = (111 * 1000) + ((int)Entity_Type.main_Player * 100) + (1);
    //    mainPlayer_L_Hand.entityID = (111 * 1000) + ((int)Entity_Type.main_Player * 100) + (2);
    //    mainPlayer_R_Hand.entityID = (111 * 1000) + ((int)Entity_Type.main_Player * 100) + (3);


    //    //test skipping clients
    //    //Client_Refresh(new Coords(2, 1, (int)Entity_Type.users_head, 1, Vector3.zero, Quaternion.identity));
    //    //Client_Refresh(new Coords(2, 2, (int)Entity_Type.users_Lhand, 1, Vector3.zero, Quaternion.identity));
    //    //Client_Refresh(new Coords(2, 3, (int)Entity_Type.users_Rhand, 1, Vector3.zero, Quaternion.identity));

    //    ////Setup Client ID;
    //    //mainPlayer_head.clientID = (uint)2;
    //    //mainPlayer_L_Hand.clientID = (uint)2;
    //    //mainPlayer_R_Hand.clientID = (uint)2;

    //    //TESTING SLICING
    //    yield return new WaitUntil(() => isURL_LoadingFinished);


    //    if (_mainClient_entityData.isTeacher)
    //        Scene_Manager.Instance.LoadSceneAdditiveAsync(_UI_Teacher_Display_sceneName, true);
    //    else
    //        Scene_Manager.Instance.LoadSceneAdditiveAsync(_UI_Student_Display_sceneName, true);


    //    //Debug.Log(sceneList._scene_Reference_List[0].sceneIndex);

    //    //Interaction_Refresh(new Interact { targetEntity_id = 0, interactionType = (int)INTERACTIONS.CHANGE_SCENE });
    //    //Interaction_Refresh(new Interact { targetEntity_id = 1, interactionType = (int)INTERACTIONS.CHANGE_SCENE });
    //    // NETWORK_CALL_On_Select_Scene_Refence(sceneList._scene_Reference_List[0].sceneIndex);//sceneList._scene_Reference_List[0].sceneIndex);
    //    //  NETWORK_CALL_On_Select_Scene_Refence()
    //    //  Interaction_Refresh(new Interact(1,1, (int)INTERACTIONS.CHANGE_SCENE));
    //    //issue with null reference no knife found?
    //    //yield return new WaitForSeconds(5);

    //    //TESTINGSlicing(111308, new Vector3(-0.34f,-0.84f, 0.42f), new Vector3(0.01f, 1.00f, -0.01f), new Vector3(0.32f, 0.50f, -0.17f));
    //    //TESTINGSlicing(111503, new Vector3(-0.87f, 0.05f, -0.50f), new Vector3(1.00f, 0.01f, 0.01f), new Vector3(0.34f, -0.04f, 0.50f));

    //}

    #region Register Network Managed Objects
    public void LinkNewNetworkObject(GameObject nGO, int index)
    {
        Net_Register_GameObject tempNet = nGO.AddComponent<Net_Register_GameObject>();
        tempNet.Instantiate(index);

    }
    public int RegisterNetWorkObject(int entityID, Net_Register_GameObject nRG)
    {
        //numbers over urls are gameobjects in scene to be modified
        _EntityID_to_urlListIndex.Add(entityID, nRG.positionWithin_urlList);
        _urlListIndex_to_EntityID.Add(nRG.positionWithin_urlList, entityID);
        _EntityID_To_NetObject.Add(entityID, nRG);
        allNetWork_GO_upload_list.Add(nRG.gameObject);

        return _EntityID_To_NetObject.Count;
    }
    #endregion


    public List<Button> _assetButtonRegisterList;
    public List<Button> _sceneButtonRegisterList;

    #region Dashboard Button Reference Setup
    public void On_Select_Asset_Refence_Button(int index, Button button)
    {
        GameObject currentObj = default;
        Net_Register_GameObject netRegisterComponent = default;

        try
        {
            currentObj = allNetWork_GO_upload_list[index];
            netRegisterComponent = currentObj.GetComponent<Net_Register_GameObject>();//GetComponentInChildren<Net_Register_GameObject>(true);
        }
        catch
        {

            return;
        }
        if (!allNetWork_GO_upload_list[index].activeInHierarchy)
        {
            var colors = button.colors;
            colors.normalColor = Color.green;
            colors.highlightedColor = Color.green + Color.black * 0.5f;
            button.colors = colors;

            EventSystem.current.SetSelectedGameObject(button.gameObject);


            NetworkUpdateHandler.Instance.InteractionUpdate(new Interact
            {
                sourceEntity_id = _mainClient_entityData.entityID,
                targetEntity_id = index,//netRegisterComponent.positionWithin_urlList,
                interactionType = (int)INTERACTIONS.RENDERING,

            });
            //#endif
            currentObj.SetActive(true);
            MainClientUpdater.Instance.PlaceInNetworkUpdateList(netRegisterComponent);
        }
        else
        {

            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white + Color.black * 0.5f;
            button.colors = colors;

            button.OnSelect(new BaseEventData(EventSystem.current));

            currentObj.SetActive(false);
            MainClientUpdater.Instance.RemoveFromInNetworkUpdateList(netRegisterComponent);

            NetworkUpdateHandler.Instance.InteractionUpdate(new Interact
            {
                sourceEntity_id = _mainClient_entityData.entityID,
                targetEntity_id = index,
                interactionType = (int)INTERACTIONS.NOT_RENDERING,

            });

        }
    }

    //public void On_Select_Scene_Refence_Button(int index, Button button)
    public void On_Select_Scene_Refence_Button(Scene_Reference sceneRef, Button button)
    {
        NetworkUpdateHandler.Instance.InteractionUpdate(new Interact
        {
            sourceEntity_id = _mainClient_entityData.entityID,
            targetEntity_id = sceneRef.sceneIndex,
            interactionType = (int)INTERACTIONS.CHANGE_SCENE,

        });

        NETWORK_CALL_On_Select_Scene_Refence(sceneRef.sceneIndex);

    }
    #endregion

    #region External Call_Dashboard Button Reference Setup


    public void NETWORK_CALL_On_Select_Scene_Refence(int sceneID)
    {
        StartCoroutine(NETWORK_CALL_On_Select_Scene_Refence_Process(sceneID));
    }

    public IEnumerator NETWORK_CALL_On_Select_Scene_Refence_Process(int sceneID)
    {
      
        yield return new WaitUntil(() => _sceneButtonRegisterList.Count > 0);

        for (int i = 0; i <= sceneList.scene_StringList.Count; i++)
        {
            try
            {
                _sceneButtonRegisterList[i].interactable = true;
                SceneManager.UnloadSceneAsync(sceneList.scene_StringList[i]);
            }
            catch
            { }
        }

        _sceneButtonRegisterList[sceneID].interactable = false;

        //build offset added to id because of initial additive scene teacher pad
        SceneManager.LoadSceneAsync(sceneList._scene_Reference_List[sceneID].name, LoadSceneMode.Additive);

    }


    public void NetworkCall_On_Select_Asset_Refence_Button(int index, bool activeState)
    {

        StartCoroutine(NetworkCall_On_Select_Asset_Refence_Button_Process(index, activeState));
    }

    public IEnumerator NetworkCall_On_Select_Asset_Refence_Button_Process(int index, bool activeState)
    {

        yield return new WaitUntil(() => _assetButtonRegisterList.Count > 0);

        GameObject currentObj = default;
        Net_Register_GameObject netRegisterComponent = default;

        try
        {
            currentObj = allNetWork_GO_upload_list[index];
            netRegisterComponent = currentObj.GetComponent<Net_Register_GameObject>();//GetComponentInChildren<Net_Register_GameObject>(true);
        }
        catch
        {

            yield break;
        }

        Button button = _assetButtonRegisterList[index];

        if (!activeState)//!allNetWork_GO_upload_list[index].activeInHierarchy)
        {
            var colors = button.colors;
            colors.normalColor = Color.green;
            colors.highlightedColor = Color.green + Color.black * 0.5f;
            button.colors = colors;

            EventSystem.current.SetSelectedGameObject(button.gameObject);

            currentObj.SetActive(true);
        }
        else
        {

            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white + Color.black * 0.5f;
            button.colors = colors;

            button.OnSelect(new BaseEventData(EventSystem.current));

            currentObj.SetActive(false);

        }
    }
    #endregion

    //Give me the id that is being inclueded
    public void UpdateClients(int client_count)
    {
        currentClientCount = _client_ID_List.Count;
    }

    #region Interaction Network Receiving Calls

    public void Interaction_Refresh(Interact newData)
    {
        StartCoroutine(Interaction_Process(newData));
    }


    public IEnumerator Interaction_Process(Interact newData)
    {
        yield return new WaitUntil(() => isURL_LoadingFinished);

        switch (newData.interactionType)
        {
            case (int)INTERACTIONS.RENDERING:

                NetworkCall_On_Select_Asset_Refence_Button(newData.targetEntity_id, false);

                //    allNetWork_GO_upload_list[newData.targetEntity_id].SetActive(true);

                break;

            case (int)INTERACTIONS.NOT_RENDERING:

                NetworkCall_On_Select_Asset_Refence_Button(newData.targetEntity_id, true);

                //       allNetWork_GO_upload_list[newData.targetEntity_id].SetActive(false);

                break;

            case (int)INTERACTIONS.GRAB:

                //if (_EntityID_To_NetObject.ContainsKey(newData.targetEntity_id))
                //{

                allNetWork_GO_upload_list[newData.targetEntity_id].GetComponent<Net_Register_GameObject>().entity_data.isCurrentlyGrabbed = true;
                //      _EntityID_To_NetObject[newData.targetEntity_id].entity_data.isCurrentlyGrabbed = true;
                //   }
                break;

            case (int)INTERACTIONS.DROP:
                //if (_EntityID_To_NetObject.ContainsKey(newData.targetEntity_id))
                //{
                allNetWork_GO_upload_list[newData.targetEntity_id].GetComponent<Net_Register_GameObject>().entity_data.isCurrentlyGrabbed = false;
                //  _EntityID_To_NetObject[newData.targetEntity_id].entity_data.isCurrentlyGrabbed = false;
                //      }

                break;

            case (int)INTERACTIONS.CHANGE_SCENE:


                //      NETWORK_CALL_On_Select_Scene_Refence(sceneList._scene_Reference_List[newData.targetEntity_id].sceneIndex);
           //     NETWORK_CALL_On_Select_Scene_Refence(sceneList._scene_Reference_List[0]);

                NETWORK_CALL_On_Select_Scene_Refence(newData.targetEntity_id);
                //   scene_StringList[newData.targetEntity_id]);
                //SceneManager.LoadSceneAsync(newData.targetEntity_id, LoadSceneMode.Additive);
                //Scene_Manager.Instance.LoadSceneAdditiveAsync(SceneManager.GetSceneAt(newData.targetEntity_id));

                break;



        }


    }
    #endregion

    #region Client and Object NetworkReceivingCalls
    //FILTERING SYSTEM
    public void Client_Refresh(Coords newData)
    {
       
        StartCoroutine(CLIENT_REFRESH_PROCESS(newData));

    }

    public IEnumerator CLIENT_REFRESH_PROCESS(Coords newData)
    {
        yield return new WaitUntil(() => isURL_LoadingFinished);


        //REGISTER NEW CLIENTS
        if (!_client_ID_List.Contains((uint)newData.clientId) && newData.entityType != (int)Entity_Type.objects)
        {
         

            //GRAB 
            var non_mainClientData = _availableGOList[_client_ID_List.Count].GetComponentInChildren<Non_MainClientData>();
            non_mainClientData.Id = (uint)newData.clientId;
            _availableClientIDToGODict.Add((uint)newData.clientId, non_mainClientData);


            if (newData.entityType != (int)Entity_Type.main_Player)
                _availableGOList[_client_ID_List.Count].SetActive(true);

            //ADD CLIENT ID TO LIST TO AVOID REDUNDANCY FROM HANDS AND HEAD
            _client_ID_List.Add((uint)newData.clientId);



            
        }

        //MOVE CLIENTS AND OBJECTS
        switch (newData.entityType)
        {
            //HEAD MOVE
            case 0:
                //changed from local to world
                _availableClientIDToGODict[(uint)newData.clientId]._EntityContainer_MAIN.transform.position = newData.pos;
                _availableClientIDToGODict[(uint)newData.clientId]._EntityContainer_MAIN.transform.rotation = new Quaternion(newData.rot.x, newData.rot.y, newData.rot.z, newData.rot.w);
                break;
            //HANDL MOVE
            case 1:
                _availableClientIDToGODict[(uint)newData.clientId]._EntityContainer_hand_L.transform.position = newData.pos;
                _availableClientIDToGODict[(uint)newData.clientId]._EntityContainer_hand_L.transform.rotation = new Quaternion(newData.rot.x, newData.rot.y, newData.rot.z, newData.rot.w);
                break;
            //HANDR MOVE
            case 2:
                _availableClientIDToGODict[(uint)newData.clientId]._EntityContainer_hand_R.transform.position = newData.pos;
                _availableClientIDToGODict[(uint)newData.clientId]._EntityContainer_hand_R.transform.rotation = new Quaternion(newData.rot.x, newData.rot.y, newData.rot.z, newData.rot.w);
                break;
            //OBJECT MOVE
            case 3:
                _EntityID_To_NetObject[newData.entityId].transform.position = newData.pos;
                _EntityID_To_NetObject[newData.entityId].transform.rotation = newData.rot;
                break;

            //MAIN PLAYER ONLY CALLED FOR INITIAL SETUP
            case 5:
                var temp = _availableClientIDToGODict[(uint)newData.clientId].transform;
                var ROT = _availableClientIDToGODict[(uint)newData.clientId]._EntityContainer_MAIN.entity_data.rot;

                //GameObject
                _mainPlayer.transform.position = temp.position;
                _mainPlayer.transform.rotation = new Quaternion(ROT.x, ROT.y, ROT.z, ROT.w);

                //hands entity data
                _mainClient_entityData.pos = temp.position;
                _mainClient_entityData.rot = new Vector4(ROT.x, ROT.y, ROT.z, ROT.w);
                _mainPlayer.transform.parent.GetChild(0).localPosition = temp.position;
                _mainPlayer.transform.parent.GetChild(0).localRotation = new Quaternion(ROT.x, ROT.y, ROT.z, ROT.w);

                //Turn Off Dummy 
                var parObject = temp.parent.parent.gameObject;
                parObject.name = "Main_Client";
                parObject.SetActive(false);
                break;

        }
    }

    #endregion



    #region String Update Calls;

    public enum STRINGTYPE
    {
        TUTORIAL,
        CLIENT_NAME,
        DIALOGUE,

    }
    public struct New_Text
    {
       public int target;
       public int stringType;
       public string text;

    }
    
    public void Text_Refresh(New_Text newText)
    {
        switch (newText.stringType)
        {
            case (int)STRINGTYPE.TUTORIAL:
                break;

            case (int)STRINGTYPE.DIALOGUE:
                break;

            case (int)STRINGTYPE.CLIENT_NAME:
                break;

        }
    }

    #endregion
    #region Slicing Calls For Network Export
    public void TESTINGSlicing(int entityData, Vector3 bladeDir, Vector3 moveDir, Vector3 knifeOrigin)//public void TESTINGSlicing(int entityData, Quaternion rotation, Vector3 pos, Vector3 dir, Vector3 origin)
    {
        
        if(_EntityID_To_NetObject.ContainsKey(entityData))
        {
        //    Debug.Log("NotGoing");
            KnifeSliceableAsync kSaSync = _EntityID_To_NetObject[entityData].GetComponent<KnifeSliceableAsync>();
           kSaSync.PropagatedSlice(bladeDir, moveDir, knifeOrigin);

        }
        else
        {
            
            StartCoroutine(WaitForSlicingObject(entityData, bladeDir, moveDir, knifeOrigin));
            return;
        }
       
    }
    public IEnumerator WaitForSlicingObject(int entityData, Vector3 bladeDir, Vector3 moveDir, Vector3 knifeOrigin)
    {
      
        while (true)
        {
            if (!_EntityID_To_NetObject.ContainsKey(entityData))
            {
                yield return null;
                Debug.Log("OBJECT DOES NOT EXIST");
            }
            else
            {
                yield return new WaitForSeconds(3);
              //  yield return new WaitUntil(() =>  _EntityID_To_NetObject[entityData].GetComponent<Rigidbody>());
               
                    KnifeSliceableAsync kSaSync = _EntityID_To_NetObject[entityData].GetComponent<KnifeSliceableAsync>();
                    kSaSync.PropagatedSlice(bladeDir, moveDir, knifeOrigin);
                    yield break;
                
            }
        }
            
    }
    #endregion

    #region Setup Client Spots
    public IEnumerator Instantiate_Reserved_Clients()
    {
        _clientDict = new Dictionary<GameObject, EntityData_SO>();

        float degrees = 360f / _clientReserveCount;
        Vector3 offset = new Vector3(0, 0, 4);

        GameObject instantiation = default;

        //Create all players with simple GameObject Representation
        for (int i = 0; i < _clientReserveCount; i++)
        {

            Vector3 TransformRelative = Quaternion.Euler(0f, degrees * i + 1, 0f) * (transform.position + offset);
            var gameObjectParentRelative = new GameObject($"Client_{i + 1}");

            instantiation = Instantiate(_otherPlayersDummyPrefab, Vector3.zero, Quaternion.identity, gameObjectParentRelative.transform);

            //Obtain each avatars UI_TEXT REFERENCE FROM THEIR CANVAS IN PREFAB
            Transform  canvas = instantiation.GetComponentInChildren<Canvas>().transform;
            _clientUser_Names_UITextReference.Add(canvas.GetChild(0).GetComponent<Text>());
            canvas.GetChild(0).GetComponent<Text>().text = $"Client_{i + 1}";

            _clientUser_Dialogue_UITextReference.Add(canvas.GetChild(1).GetComponent<Text>());

            //Set up links for network call references
            var non_mainClientData = instantiation.GetComponentInChildren<Non_MainClientData>(true);

            non_mainClientData._EntityContainer_hand_L.entity_data = ScriptableObject.CreateInstance<EntityData_SO>();
            non_mainClientData._EntityContainer_hand_R.entity_data = ScriptableObject.CreateInstance<EntityData_SO>();
            var clientData_Main = non_mainClientData._EntityContainer_MAIN.entity_data = ScriptableObject.CreateInstance<EntityData_SO>();

            gameObjectParentRelative.transform.position = TransformRelative;

            //Same orientation
            TransformRelative.y = _CenterToSpawnClients.transform.position.y;

            Quaternion newRot = Quaternion.LookRotation(_CenterToSpawnClients.transform.position - TransformRelative, Vector3.up);

            clientData_Main.rot = new Vector4(newRot.x, newRot.y, newRot.z, newRot.w);

            non_mainClientData.transform.parent.localRotation = new Quaternion(newRot.x, newRot.y, newRot.z, newRot.w);

            _ExternalClientList.Add(non_mainClientData);

            _availableGOList.Add(instantiation);

            _clientDict[instantiation] = clientData_Main;

            instantiation.SetActive(false);

        }

        isSpawningFinished = true;
        yield return null;
    }

    private Vector3 GetSlotLocation(int slot)
    {

        Vector3 location = transform.position;

        float degrees = 360f / _clientReserveCount + 1;
        // degrees = 30;
        degrees *= slot;

        //   location.y = -1f;

        location.x = Mathf.Cos(Mathf.Deg2Rad * degrees);
        location.x *= _spreadRadius;
        location.z = Mathf.Cos(Mathf.Deg2Rad * degrees);
        location.z *= _spreadRadius;
        return location;
    }

    #endregion

}

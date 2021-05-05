using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Runtime.InteropServices;

[System.Serializable]
public class Int_UnityEvent: UnityEvent<int> { }

public struct Coords
{
    public int clientId;
    public int entityId;
    public int entityType; 
    public float scaleFactor;
    public Quaternion rot;
    public Vector3 pos;
 
    public Coords(int clientId, int entityId, int entityType, float scaleFactor, Vector3 pos, Quaternion rot)
    {
        this.clientId = clientId;
        this.entityId = entityId;
        this.entityType = entityType;
        this.scaleFactor = scaleFactor;
        this.pos = pos;
        this.rot = rot;
    }

}

public struct Interact
{
    
    public int sourceEntity_id;
    public int targetEntity_id;
    public int interactionType;

    public Interact(int sourceEntity_id, int targetEntity_id, int interactionType)
    {
      
        this.sourceEntity_id = sourceEntity_id;
        this.targetEntity_id = targetEntity_id;
        this.interactionType = interactionType;

    }

}

public class NetworkUpdateHandler :  SingletonComponent<NetworkUpdateHandler>
{

    public static NetworkUpdateHandler Instance
    {
        get { return ((NetworkUpdateHandler)_Instance); }
        set { _Instance = value; }
    }

    // import callable js functions
    [DllImport("__Internal")]
    private static extern int GetClientIdFromBrowser();

    // import callable js functions
    [DllImport("__Internal")]
    private static extern int GetSessionIdFromBrowser();

    // import callable js functions
    [DllImport("__Internal")]
    private static extern int GetIsTeacherFlagFromBrowser();

    [DllImport("__Internal")]
    private static extern void InitSocketIOReceive(float[] array, int size);

    [DllImport("__Internal")]
    private static extern void InitSocketIOClientCounter();

    [DllImport("__Internal")]
    private static extern void SocketIOSendPosition(float[] array, int size);

    [DllImport("__Internal")]
    private static extern void SocketIOSendInteraction(int[] array, int size);

    [DllImport("__Internal")]
    private static extern void InitSocketIOReceiveInteraction(int[] array, int size);

    [DllImport("__Internal")]
    private static extern void EnableMicrophone();

    [DllImport("__Internal")]
    private static extern string GrabAssets();


    // session id from JS
    public int session_id;

    // client_id from JS
    public int client_id;

    // is the current client a teacher? from JS
    public int isTeacher;

    public EntityData_SO mainEntityData;

    // internal network update sequence counter
    private int seq = 0;


    // field to array index mapping
    const int SEQ = 0;
    const int SESSION_ID = 1;
    const int CLIENT_ID = 2;
    const int ENTITY_ID = 3;
    const int ENTITY_TYPE = 4;
    const int SCALE = 5;
    const int ROTX = 6;
    const int ROTY = 7;
    const int ROTZ = 8;
    const int ROTW = 9;
    const int POSX = 10;
    const int POSY = 11;
    const int POSZ = 12;
    const int DIRTY = 13;

    public Coord_UnityEvent unityExportNewDataEvent;

    public Int_UnityEvent onClientChage;

    public Text clientCount;

    const int NUMBER_OF_POSITION_UPDATE_FIELDS = 14;
    const int MAX_NUMBER_OF_ENTITIES = 50;
    float[] network_data = new float[NUMBER_OF_POSITION_UPDATE_FIELDS * MAX_NUMBER_OF_ENTITIES];

    const int NUMBER_OF_INTERACTION_FIELDS = 7;
    int[] interaction_data = new int[NUMBER_OF_INTERACTION_FIELDS];

    // serialize coordinate struct from entity event into float array
    // using sequence counter to collate update events downstream
    public float[] SerializeCoordsStruct(int seq, Coords coords)
    {

        float[] arr = new float[NUMBER_OF_POSITION_UPDATE_FIELDS];

        arr[SEQ] = (float)seq;
        arr[SESSION_ID] = (float)session_id;
        arr[CLIENT_ID] = (float)client_id;

        arr[ENTITY_ID] = (float)coords.entityId;
        arr[ENTITY_TYPE] = (float)coords.entityType;
        arr[SCALE] = coords.scaleFactor;
        arr[ROTX] = coords.rot.x;
        arr[ROTY] = coords.rot.y;
        arr[ROTZ] = coords.rot.z;
        arr[ROTW] = coords.rot.w;
        arr[POSX] = coords.pos.x;
        arr[POSY] = coords.pos.y;
        arr[POSZ] = coords.pos.z;
        arr[DIRTY] = 1;

        return arr;
    }

    public Coords DeserializeNetworkData(float[] network_data)
    {
        Coords newCoords = new Coords();

        newCoords.clientId = (int)network_data[CLIENT_ID];
        newCoords.entityId = (int)network_data[ENTITY_ID];
        newCoords.entityType = (int)network_data[ENTITY_TYPE];
        newCoords.scaleFactor = network_data[SCALE];
        newCoords.rot.x = network_data[ROTX];
        newCoords.rot.y = network_data[ROTY];
        newCoords.rot.z = network_data[ROTZ];
        newCoords.rot.w = network_data[ROTW];
        newCoords.pos.x = network_data[POSX];
        newCoords.pos.y = network_data[POSY];
        newCoords.pos.z = network_data[POSZ];

        return newCoords;
    }

    public void Awake()
    {

#if !UNITY_EDITOR && UNITY_WEBGL
        client_id = GetClientIdFromBrowser();
        session_id = GetSessionIdFromBrowser();
        isTeacher  = GetIsTeacherFlagFromBrowser();

        // clear then populating url_list
        
        
        ClientSpawnManager.Instance.listOfObjects.url_list.Clear();
        string Assets = GrabAssets();
        Assets = Assets.Replace("},{", "|");
        Assets = Assets.Replace("}]", "");
        Assets = Assets.Replace("[{", "");
        Assets = Assets.Replace("\"","");
        string[] Assets_list = Assets.Split('|');
        for (var Assets_index = 0; Assets_index < Assets_list.Length; Assets_index += 1)
        {
            String_List.URL_Content Asset = new String_List.URL_Content();
            string[] Attri_list = Assets_list[Assets_index].Split(',');
            Asset.name = Attri_list[0].Remove(0,5);
            Asset.url = Attri_list[1].Remove(0,4);
            Asset.scale = float.Parse(Attri_list[2].Remove(0,6));
            Asset.position = Vector3.zero;
            Asset.euler_rotation = Vector3.zero;

            ClientSpawnManager.Instance.listOfObjects.url_list.Add(Asset);
        }

        mainEntityData.clientID = (uint)client_id;
        mainEntityData.sessionID = (uint)session_id;
        mainEntityData.isTeacher = (isTeacher != 0);

        // init socket.io event handlers
        InitSocketIOClientCounter();

        // set up shared memory with js context
        InitSocketIOReceive(network_data, network_data.Length);
        InitSocketIOReceiveInteraction(interaction_data, interaction_data.Length);
#endif

    }

    public void NetworkUpdate(Coords coords)
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        // socket.io with webgl
        // https://www.gamedev.net/articles/programming/networking-and-multiplayer/integrating-socketio-with-unity-5-webgl-r4365/

        float[] arr_coords = SerializeCoordsStruct(seq, coords);
        SocketIOSendPosition(arr_coords, arr_coords.Length);
#endif
    }

    public void InteractionUpdate(Interact interact)//int SourceEntityId, int TargetEntityId, INTERACTIONS interactionType)
    {
        //   Interact int = new Interact(seq, session_id, client_id, SourceEntityId, TargetEntityId, (int)interactionType);
        int[] arr_inter = new int[NUMBER_OF_INTERACTION_FIELDS];
        arr_inter[0] = seq;
        arr_inter[1] = session_id;
        arr_inter[2] = (int)client_id;
        arr_inter[3] = interact.sourceEntity_id;
        arr_inter[4] = interact.targetEntity_id;
        arr_inter[5] = (int)interact.interactionType;
        arr_inter[6] = 1; // dirty bit
#if !UNITY_EDITOR && UNITY_WEBGL
        SocketIOSendInteraction(arr_inter, arr_inter.Length);
#endif
    }

    public void Update()
    {
        // Update() iterates over the network_data heap by entity index and checks for new data.

        var slot_data = new float[NUMBER_OF_POSITION_UPDATE_FIELDS];

        for (var slot_index = 0; slot_index < network_data.Length; slot_index += NUMBER_OF_POSITION_UPDATE_FIELDS)
        {
            if (network_data[slot_index + DIRTY] != 0)
            {
                network_data[slot_index + DIRTY] = 0;

                for (var i = 0; i < NUMBER_OF_POSITION_UPDATE_FIELDS; i++)
                {
                    slot_data[i] = network_data[slot_index + i];
                }

                // unpack entity update into Coords struct
                Coords newEntityCoords = DeserializeNetworkData(slot_data);
                
                // send new network data to client spawn manager
                if (ClientSpawnManager.IsAlive != false)
                ClientSpawnManager.Instance.Client_Refresh(newEntityCoords);
            }
        }

        // checks interaction shared memory for new updates
        if (interaction_data[6] != 0) {

            Interact i_struct = new Interact
            {
                sourceEntity_id = interaction_data[3],
                targetEntity_id = interaction_data[4],
                interactionType = interaction_data[5]
            };

            if (ClientSpawnManager.IsAlive != false)
                ClientSpawnManager.Instance.Interaction_Refresh(i_struct);
            interaction_data[6] = 0;  // reset the dirty bit

        }

        seq++;
    }

    public void RegisterNewClientId(int client_id)
    {
        onClientChage.Invoke(client_id);
    }

}



using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[System.Serializable]
public class Coord_UnityEvent : UnityEvent<Coords> { }

public class MainClientUpdater : SingletonComponent<MainClientUpdater>, IUpdatable
{

    public Coord_UnityEvent coordExport;
    public Transform[] transformsNetworkOutput;

    public List<Net_Register_GameObject> entityContainers_InNetwork_OutputList = new List<Net_Register_GameObject>(); 


    private uint length;
    
    [SerializeField] private int clientID;

    [SerializeField]
    private bool isSendingUpdates;


    public static MainClientUpdater Instance
    {
        get { return ((MainClientUpdater)_Instance); }
        set { _Instance = value; }
    }
    IEnumerator Start()
    {
        yield return new WaitUntil(() => ClientSpawnManager.Instance.isSpawningFinished );
            isSendingUpdates = true;

        GameStateManager.Instance.RegisterUpdatableObject(this);

        clientID = NetworkUpdateHandler.Instance.client_id;
    }
    public void OnDisable()
    {
        if(GameStateManager.IsAlive)
        GameStateManager.Instance.DeRegisterUpdatableObject(this);
    }
    public void PlaceInNetworkUpdateList(Net_Register_GameObject nRO)
    {

        entityContainers_InNetwork_OutputList.Add(nRO);

    }
    public void RemoveFromInNetworkUpdateList(Net_Register_GameObject nRO)
    {
        //untrigger despawn -> allow to update to remove 
        if(entityContainers_InNetwork_OutputList.Contains(nRO))
          
        //trigger despawn
        //nRO.entity_data.entityID *= -1;
        StartCoroutine(DelayRemoveFromNetwork(nRO));
    }
    public IEnumerator DelayRemoveFromNetwork(Net_Register_GameObject nRO)
    {
        //should stillbe negative to allow update to work and send clientRefresh the proper negative ID
        yield return new WaitForSeconds(1.5f);

        entityContainers_InNetwork_OutputList.Remove(nRO);
        //nRO.entity_data.entityID = Mathf.Abs(nRO.entity_data.entityID);

    }
    public void RemoveALLInNetworkUpdateList(Net_Register_GameObject nRO)
    {
        entityContainers_InNetwork_OutputList.Clear();

    }



    public void OnUpdate(float realTime)
    {
        if (!isSendingUpdates)
            return;

        //HEAD
        SendUpdatesToNetwork(Entity_Type.users_head, transformsNetworkOutput[0].position, transformsNetworkOutput[0].rotation);

        //L_HAND
        SendUpdatesToNetwork(Entity_Type.users_Lhand, transformsNetworkOutput[1].position, transformsNetworkOutput[1].rotation);

        //R_HAND
        SendUpdatesToNetwork(Entity_Type.users_Rhand, transformsNetworkOutput[2].position, transformsNetworkOutput[2].rotation);

        //NETWORK_OBJECTS
        foreach (var entityContainers in entityContainers_InNetwork_OutputList)
        {
            Send_GameObject_UpdatesToNetwork(entityContainers);
        }
    }

    public void SendUpdatesToNetwork(Entity_Type entityType, Vector3 position, Quaternion rotation)
    {

        Coords coords = new Coords
        {
            clientId = this.clientID,
            entityId = (this.clientID * 10) + (int)entityType,
            entityType = (int)entityType,
            rot = rotation,
            pos = position,
        };
        coordExport.Invoke(coords);

    }


    public void Send_GameObject_UpdatesToNetwork(Net_Register_GameObject eContainer)
    {

        Coords coords = new Coords
        {
            clientId = this.clientID,//(int) eContainer.entity_data.clientID, 
            entityId = (int) eContainer.entity_data.entityID,//((int)eContainer.entity_data.clientID * 10) + (int)eContainer.entity_data.current_Entity_Type,
            entityType = (int)eContainer.entity_data.current_Entity_Type,
            rot = eContainer.transform.rotation,
            pos = eContainer.transform.position,
        };
        coordExport.Invoke(coords);

    }

  
}
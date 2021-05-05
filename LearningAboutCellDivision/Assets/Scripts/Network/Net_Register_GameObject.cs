using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class Net_Register_GameObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    //Register object to reference lists on clientspawnmanager to be refered to for synchronization
    

    public EntityData_SO entity_data;
    public int positionWithin_urlList;

    public bool isAutomatic_Register = false;

   // public bool isSkip_URLWait = false;
   //public bool isInitializeEarly
    //    public static int numberOfSceneObj= 500;
    public bool isRegistered = false;
    public void Awake()
    {
        isRegistered = false;
       
    }
    public void InitializeEarly()
    {
     
            Instantiate(ClientSpawnManager.Instance._EntityID_To_NetObject.Count);

    }
    public IEnumerator Start()
    {
        if (isAutomatic_Register)
        {
       //     if (!isSkip_URLWait)
                yield return new WaitUntil(() => ClientSpawnManager.Instance.isURL_LoadingFinished);
            //else
            //    yield return null;

            //TRY TO MAKE THIS NUMBER AFTER URL LIST TO HAVE A CONSISTING OBJECTS WITHIN LIST 1-20 etc
          //  Instantiate(++numberOfSceneObj, true);
          
          if(isRegistered == false)
            Instantiate(ClientSpawnManager.Instance._EntityID_To_NetObject.Count);
        //    yield return null;
        }
    }

    public void Instantiate(int positionWithinURL_Index)
    {
        this.positionWithin_urlList = positionWithinURL_Index;

        entity_data = ScriptableObject.CreateInstance<EntityData_SO>();
        entity_data.current_Entity_Type = Entity_Type.objects;
        entity_data.clientID = (uint)NetworkUpdateHandler.Instance.client_id;

        //// ENTITYID DERIVED EXAMPLE =  CLIENTID - 65, ENTITY TYPE - 3, Count - 1 = ENTITYID 6531
        entity_data.entityID = (111 * 1000) + ((int)Entity_Type.objects * 100) + (ClientSpawnManager.Instance._EntityID_To_NetObject.Count);//1111 + ((int)Entity_Type.objects * 10000); //Convert.ToUInt32(string.Format("{0}{1}{2}",1111, (uint)Entity_Type.objects, (uint)ClientSpawnManager.Instance._net_object_ID_List.Contains));

        ClientSpawnManager.Instance.RegisterNetWorkObject(entity_data.entityID, this);
        isRegistered = true;

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        try { NetworkUpdateHandler.Instance.InteractionUpdate(
            new Interact
            {
                interactionType = (int)INTERACTIONS.LOOK,
                sourceEntity_id = ClientSpawnManager.Instance._mainClient_entityData.entityID,
                targetEntity_id = entity_data.entityID,
            });
        
        } catch
        {
            Debug.LogWarning("Couldn't process look interaction event");
        }
     
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        try
        {
            NetworkUpdateHandler.Instance.InteractionUpdate(
           new Interact
           {
               interactionType = (int)INTERACTIONS.LOOK_END,
               sourceEntity_id = ClientSpawnManager.Instance._mainClient_entityData.entityID,
               targetEntity_id = entity_data.entityID,
           });

        }
        catch
        {
            Debug.LogWarning("Couldn't process look interaction event");
        }
    }
}

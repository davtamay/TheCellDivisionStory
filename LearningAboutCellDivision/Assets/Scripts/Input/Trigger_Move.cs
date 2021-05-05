using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_Move : Trigger_Base
{

    public float attenuation;
    public Vector3 initialPos;
    public Transform _posOfHandLaser;

    public Transform thisTransform;
    public string _interactableTag;

    InputSelection inputSelection;

    public float _magnitudeOfMove = 3;

   // public float _minScale = 0.01f;
    public override void Start()
    {
        inputSelection = transform.parent.GetComponent<InputSelection>();
        _posOfHandLaser = inputSelection.transform;
        _interactableTag = inputSelection._tagToLookFor;
        thisTransform = transform;
        initialParent = transform.parent;
    }


    public float initialOffset;
    
   // public Vector3 initialScale;
    public GameObject currentGO;
    public override void OnTriggerEnter(Collider other)
    {
        //if ui skip
        if (other.gameObject.layer == 5)
            return;

        if (other.CompareTag(_interactableTag))
        {
            //THIS IS TO AVOID LOOSING CONNECTION (INITIALPOS) TO INITIAL OBJECT - DISABLED REMOVE THE CONNECTION
            if (other.gameObject != currentGO)
            {
                currentGO = other.gameObject;
                originalObjectParent = other.transform.parent;
            }
            else
                return;


            initialPos = thisTransform.position;
            initialOffset = Vector3.Distance(_posOfHandLaser.position, initialPos);

            initialOBJECToffset = Vector3.Distance(_posOfHandLaser.position, other.transform.position);
            //initialScale = other.transform.localScale;
            initialPosOffset =  other.transform.position - _posOfHandLaser.position;

            //  other.transform.position = _posOfHandLaser.forward.normalized * initialOBJECToffset + _posOfHandLaser.position;


            //NETWORK REGISTER
            try
            {
                var currentInteractiveObject = other.GetComponent<Net_Register_GameObject>();

                NetworkUpdateHandler.Instance.InteractionUpdate(new Interact
                {
                        sourceEntity_id = ClientSpawnManager.Instance._mainClient_entityData.entityID,//GetComponent<Entity_Container>().entity_data.entityID,
                        targetEntity_id = currentInteractiveObject.positionWithin_urlList,
                        interactionType = (int)INTERACTIONS.GRAB,
                });

                MainClientUpdater.Instance.PlaceInNetworkUpdateList(currentInteractiveObject);
               // currentInteractiveObject.entity_data.isCurrentlyGrabbed = true;
            }
            catch
            {
                Debug.LogWarning("Custom Warning: " + "Could not send Interaction : ");
            }
        }



       
    }
    public float initialOBJECToffset;
    public Transform initialParent;
    public Transform originalObjectParent;
    public Vector3 initialPosOffset;
    public override void OnTriggerStay(Collider other)
    {

        if (other.gameObject.layer == 5)
            return;


        if (inputSelection.isOverObject)
        {
            attenuation = Vector3.Distance(_posOfHandLaser.position, initialPos) / initialOffset;
            //Debug.Log(attenuation - 1);
            //TAKE OUT THE ONE START AT ZERO CAN DETERMINE DIRECTION OF ROTATION FORWARD RIGHT BACK LEFT


            initialOBJECToffset = initialOBJECToffset+ (-1* ((attenuation - 1) * _magnitudeOfMove));
        
          //  Vector3 currentObj = other.transform.position + (_posOfHandLaser.forward * (attenuation - 1));
            //  initialOBJECToffset = Vector3.Distance(_posOfHandLaser.position, other.transform.position);
         //   other.transform.position = _posOfHandLaser.forward.normalized * initialOBJECToffset + _posOfHandLaser.position;
        }
    }
    public void Update()
    {
        if(currentGO !=null)
            currentGO.transform.position = _posOfHandLaser.forward.normalized * initialOBJECToffset + _posOfHandLaser.position;
    }
    public override void OnTriggerExit(Collider other)
    {
        if (currentGO == null)
            return;

        try
        {
            //Net_Register_GameObject netRegisterObj = currentRigidBody.GetComponent<Net_Register_GameObject>();
            //#if !UNITY_EDITOR && UNITY_WEBGL
            var currentInteractiveObject = currentGO.GetComponent<Net_Register_GameObject>();

            NetworkUpdateHandler.Instance.InteractionUpdate(new Interact
                {
                    sourceEntity_id = ClientSpawnManager.Instance._mainClient_entityData.entityID,
                    targetEntity_id = currentInteractiveObject.positionWithin_urlList,
                    interactionType = (int)INTERACTIONS.DROP,
                });


            MainClientUpdater.Instance.RemoveFromInNetworkUpdateList(currentInteractiveObject);
        //    currentInteractiveObject.entity_data.isCurrentlyGrabbed = false;


        }
        catch
        {
            Debug.LogWarning("Custom Warning: " + "Could not send Interaction : ");
        }


    }
    public override void OnDisable()
    {

        if (currentGO == null)
            return;

        try
        {
            //Net_Register_GameObject netRegisterObj = currentRigidBody.GetComponent<Net_Register_GameObject>();
            //#if !UNITY_EDITOR && UNITY_WEBGL
            var currentInteractiveObject = currentGO.GetComponent<Net_Register_GameObject>();

            NetworkUpdateHandler.Instance.InteractionUpdate(new Interact
            {
                sourceEntity_id = ClientSpawnManager.Instance._mainClient_entityData.entityID,
                targetEntity_id = currentInteractiveObject.positionWithin_urlList,
                interactionType = (int)INTERACTIONS.DROP,
            });


            MainClientUpdater.Instance.RemoveFromInNetworkUpdateList(currentInteractiveObject);
        //    currentInteractiveObject.entity_data.isCurrentlyGrabbed = false;


        }
        catch
        {
            Debug.LogWarning("Custom Warning: " + "Could not send Interaction : ");
        }
        //if (currentGO != null)
        //{
        //    currentGO.transform.SetParent(originalObjectParent, true);
        currentGO = null;
        //}
    }
}

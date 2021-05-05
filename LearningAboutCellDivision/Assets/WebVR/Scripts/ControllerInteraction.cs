using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using UnityEngine.XR;

public enum INTERACTIONS
{
    LOOK = 0,
    LOOK_END = 1,
    RENDERING = 2,
    NOT_RENDERING = 3,

    GRAB = 4,
    DROP = 5,

    CHANGE_SCENE = 6,

    SLICE_OBJECT = 7,

}

public class ControllerInteraction : MonoBehaviour, IUpdatable
{
    [System.Serializable]
    public class InteractionEvent : UnityEvent<Interact> { }
    public InteractionEvent onInteract;

    public UnityEvent onTriggerButtonDown;
    public UnityEvent onTriggerButtonUP;
    public UnityEvent onGripButtonDown;
    public UnityEvent onGripButtonUP;
    private FixedJoint attachJoint = null;
    public Rigidbody currentRigidBody = null;
    public static Rigidbody firstObjectGrabed;

    private static List<Rigidbody> contactRigidBodies = new List<Rigidbody>();
    private Dictionary<Rigidbody, Net_Register_GameObject> rigidB_To_NetRegObj_Dic = new Dictionary<Rigidbody, Net_Register_GameObject>();


    [SerializeField] private GameObject pointerInputObject;
    [SerializeField] private bool hasObject;
    [SerializeField] private static int currentObjectId;

    public bool isBothHandsHaveObject;

    private Animator thisAnimCont;

    WebVRController webVRController;

    public static List<GameObject> handTransform;

    void Awake()
    {

        //add bothHands
        handTransform = new List<GameObject>(2);
        handTransform.AddRange(GameObject.FindGameObjectsWithTag("Hand"));


        attachJoint = GetComponent<FixedJoint>();

        thisAnimCont = gameObject.GetComponent<Animator>();
    }

    private GameObject renderingChild;
    void Start()
    {
      

        GameStateManager.Instance.RegisterUpdatableObject(this);
        //  renderingChild = transform.GetComponentInChildren<SkinnedMeshRenderer>(true).gameObject;


        webVRController = gameObject.GetComponent<WebVRController>();

    }
    // public Coroutine startAfterAnimCoroutine;
    public void OnDestroy()
    {
        //#if !UNITY_EDITOR && UNITY_WEBGL


        try
        {
            NetworkUpdateHandler.Instance.InteractionUpdate(new Interact
            {
                sourceEntity_id = GetComponent<Entity_Container>().entity_data.entityID,
                targetEntity_id = rigidB_To_NetRegObj_Dic[currentRigidBody].positionWithin_urlList,
                interactionType = (int)INTERACTIONS.DROP,
            });
        } catch { }
        //#endif

        if (GameStateManager.IsAlive)
            GameStateManager.Instance.DeRegisterUpdatableObject(this);
    }

    //FOR Introduction instruction input grab
    public void Register_OnTriggerButton_Down_UnityAction(UnityAction ExtUnityAction) => onTriggerButtonDown.AddListener(ExtUnityAction);
    public void DeRegister_OnTriggerButton_Down_UnityAction(UnityAction ExtUnityAction) => onTriggerButtonDown.RemoveListener(ExtUnityAction);

    public void Register_OnTriggerButton_UP_UnityAction(UnityAction ExtUnityAction) => onTriggerButtonUP.AddListener(ExtUnityAction);
    public void DeRegister_OnTriggerButton_UP_UnityAction(UnityAction ExtUnityAction) => onTriggerButtonUP.RemoveListener(ExtUnityAction);

    public void Register_OnGripButton_Down_UnityAction(UnityAction ExtUnityAction) => onGripButtonDown.AddListener(ExtUnityAction);
    public void DeRegister_OnGripButton_Down_UnityAction(UnityAction ExtUnityAction) => onGripButtonDown.RemoveListener(ExtUnityAction);

    public void Register_OnGripButton_Up_UnityAction(UnityAction ExtUnityAction) => onGripButtonUP.AddListener(ExtUnityAction);
    public void DeRegister_OnGripButton_Up_UnityAction(UnityAction ExtUnityAction) => onGripButtonUP.RemoveListener(ExtUnityAction);
    
    public Vector3 initialScale;
    public float initialDistance;
    public static bool isInitialDoubleGrab;

    public void OnUpdate(float realTime)
    {
        if (isBothHandsHaveObject)
        {
            if (firstObjectGrabed == null)
            {
                isBothHandsHaveObject = false;
                return;

            }

            if (isInitialDoubleGrab == false)
            {
                isInitialDoubleGrab = true;
                initialDistance = Vector3.Distance(handTransform[0].transform.position, handTransform[1].transform.position);
                initialScale = firstObjectGrabed.transform.localScale;
                return;
            }

            var scaleRatioBasedOnDistance = Vector3.Distance(handTransform[0].transform.position, handTransform[1].transform.position) / initialDistance;

            if (float.IsNaN(firstObjectGrabed.transform.localScale.y)) return;
            firstObjectGrabed.transform.localScale = initialScale * scaleRatioBasedOnDistance;
            firstObjectGrabed.transform.position = handTransform[0].transform.position + (handTransform[1].transform.position - handTransform[0].transform.position) / 2;
        }


        float normalizedTime = webVRController.GetButton("Trigger") ? 1 : webVRController.GetAxis("Grip");
#if !UNITY_EDITOR && UNITY_WEBGL
        bool isTriggerButtonDown = webVRController.GetButtonDown("Trigger");
        bool isTriggerButtonUP = webVRController.GetButtonUp("Trigger");
        bool isGripButtonDown = webVRController.GetButtonDown("Grip");
        bool isGripButtonUP = webVRController.GetButtonUp("Grip");
#endif

#if UNITY_EDITOR || !UNITY_WEBGL
    //    gameObject.SetActive(true);
        bool isTriggerButtonDown = Input.GetKeyDown(KeyCode.G); // GetButtonDown("space");//UnityEngine.XR.
        bool isTriggerButtonUP = Input.GetKeyDown(KeyCode.G);  //webVRController.GetButtonUp("Trigger");
       bool isGripButtonDown = Input.GetKeyDown(KeyCode.H);  //webVRController.GetButtonDown("Grip");
       bool isGripButtonUP = Input.GetKeyDown(KeyCode.H);  //webVRController.GetButtonUp("Grip");
#endif


       thisAnimCont.Play("Take", -1, normalizedTime);

        
        if (isTriggerButtonDown)
        {
            StartCoroutine(SetActionAfterAnimation());
           // pointerInputObject.SetActive(true);

            onTriggerButtonDown.Invoke();
        }

        if (isTriggerButtonUP)
        {
          //  pointerInputObject.SetActive(false);
            StopCoroutine(SetActionAfterAnimation());

            onTriggerButtonUP.Invoke();
        }

        if (isGripButtonDown)
        {
            StartCoroutine(SetActionAfterAnimation());

            onGripButtonDown.Invoke();
            Pickup();
        }

        if (isGripButtonUP)
        {
          //  pointerInputObject.SetActive(false);
            StopCoroutine(SetActionAfterAnimation());

            onGripButtonUP.Invoke();
            Drop();
        }

        
    }
    public IEnumerator SetActionAfterAnimation()
    {
        thisAnimCont.Play("Take", -1, 1);
  
        yield return null;


    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Interactable")  || other.CompareTag("Hand"))//&& contactRigidBodies.Contains(other.gameObject.GetComponent<Rigidbody>()))
            return;

        var rigidB = other.gameObject.GetComponent<Rigidbody>();

        if (!contactRigidBodies.Contains(rigidB))
        {
            contactRigidBodies.Add(rigidB);
            
            try {
                rigidB_To_NetRegObj_Dic.Add(rigidB, rigidB.GetComponent<Net_Register_GameObject>());
            }
            catch { }
            
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Interactable") || other.CompareTag("Hand"))
            return;

        var rigidB = other.gameObject.GetComponent<Rigidbody>();

        if (contactRigidBodies.Contains(rigidB))
        {
            contactRigidBodies.Remove(rigidB);

            if (rigidB_To_NetRegObj_Dic.Count != 0)
                rigidB_To_NetRegObj_Dic.Remove(rigidB);
        }
        // onInteract.Invoke(webVRController.GetInstanceID(), other.gameObject.GetComponent<Rigidbody>().GetInstanceID(), 5);

        //contactRigidBodies.Remove(other.gameObject.GetComponent<Rigidbody>());

    }
    public static ControllerInteraction firstControllerInteraction;
    public static ControllerInteraction secondControllerInteraction;
    [ContextMenu("PICK UP")]
    public void Pickup()
    {
       
        if (!hasObject)
        {
            currentRigidBody = GetNearestRigidBody();

            if (!currentRigidBody)
                return;

            //ACOUNT FOR DOUBLE GRAB PICKUP
            if (firstObjectGrabed == null)
            {
                firstObjectGrabed = currentRigidBody;
                firstControllerInteraction = this;

            }
            else if (firstObjectGrabed == currentRigidBody)
            {
                isBothHandsHaveObject = true;
                secondControllerInteraction = this;
            }

            hasObject = true;

            currentRigidBody.MovePosition(transform.position);
            attachJoint.connectedBody = currentRigidBody;

            currentRigidBody.isKinematic = false;

           


            //NETWORK REGISTER
            try
            {
                //#if !UNITY_EDITOR && UNITY_WEBGL
                if (rigidB_To_NetRegObj_Dic.Count != 0)
                {
                    NetworkUpdateHandler.Instance.InteractionUpdate(new Interact
                    {
                        sourceEntity_id = GetComponent<Entity_Container>().entity_data.entityID,
                        targetEntity_id = rigidB_To_NetRegObj_Dic[currentRigidBody].positionWithin_urlList,//rigidB_To_NetRegObj_Dic[currentRigidBody].entity_data.entityID,//netRegisterObj.entity_data.entityID,
                        interactionType = (int)INTERACTIONS.GRAB,
                    });

                    MainClientUpdater.Instance.PlaceInNetworkUpdateList(rigidB_To_NetRegObj_Dic[currentRigidBody]);
                    //#endif
                }
            }
            catch
            {
                Debug.LogWarning("Custom Warning: " + "Could not send Interaction : ");
            }


           
           

        }

      
       
    }
    [ContextMenu("DROP")]
    public void Drop()
    {

        if (hasObject)
        {
            currentRigidBody.isKinematic = true;

            try
            {
                //Net_Register_GameObject netRegisterObj = currentRigidBody.GetComponent<Net_Register_GameObject>();
                //#if !UNITY_EDITOR && UNITY_WEBGL

                if (rigidB_To_NetRegObj_Dic.Count != 0)
                {
                    NetworkUpdateHandler.Instance.InteractionUpdate(new Interact
                    {
                        sourceEntity_id = GetComponent<Entity_Container>().entity_data.entityID,
                        targetEntity_id = rigidB_To_NetRegObj_Dic[currentRigidBody].positionWithin_urlList,//rigidB_To_NetRegObj_Dic[currentRigidBody].entity_data.entityID,
                        interactionType = (int)INTERACTIONS.DROP,
                    });
                }
                 MainClientUpdater.Instance.RemoveFromInNetworkUpdateList(rigidB_To_NetRegObj_Dic[currentRigidBody]);
//#endif

            }
            catch
            {
                Debug.LogWarning("Custom Warning: " + "Could not send Interaction : ");
            }

            contactRigidBodies.Clear();
            rigidB_To_NetRegObj_Dic.Clear();

            attachJoint.connectedBody = null;



            if (firstControllerInteraction == this && secondControllerInteraction == this)
            {
               // firstControllerInteraction.Drop();
                isBothHandsHaveObject = false;
                isInitialDoubleGrab = false;
                secondControllerInteraction = null;
            }
            else
            {
                firstObjectGrabed = null;
                currentRigidBody = null;
                firstControllerInteraction = null;

            }




            hasObject = false;

            
        }

        // hasObject = false;
       


        //Remove Shared Object
        //if (presentRigidBody == currentRigidBody)
  


    }

    private Rigidbody GetNearestRigidBody()
    {
        Rigidbody nearestRigidBody = null;
        float minDistance = float.MaxValue;
        float distance = 0.0f;

        foreach (Rigidbody contactBody in contactRigidBodies)
        {
            //if (contactBody.CompareTag("Hand"))
            //    continue;
      

            distance = (contactBody.gameObject.transform.position - transform.position).sqrMagnitude;

            if (distance < minDistance)
            {
                //CHECK IF OBJECT IS GRABBED BY SOMEONE ELSE
                try
                {
                    if (rigidB_To_NetRegObj_Dic[contactBody].entity_data.isCurrentlyGrabbed)
                        continue;
                   
                 }
                catch
                {

                }

                minDistance = distance;
                nearestRigidBody = contactBody;
            }
        }

        return nearestRigidBody;
    }

  
}

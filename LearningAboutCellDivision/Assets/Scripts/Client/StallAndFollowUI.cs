using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class StallAndFollowUI : MonoBehaviour
{

    [SerializeField] private Transform objectToRotate;
    [SerializeField] private int angleDistanceUntilRotateBack = 80;
    [SerializeField] private bool isUseCameraWorldUp = true;

    [Header("Camera To Orient With")]
    public Transform camTransform;

    [Header("States")]
    [SerializeField] Bool_StateCheck_SO isInVR;

    private Vector3 offset;

    Quaternion rotation;

    bool isInitialLook = false;


    Vector3 oldViewingAngle;
    Vector3 curViewingAngle;

    bool isLerping = false;


    public bool hooldLerping;

    //public void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Hand"))
    //    {
    //        hooldLerping = true;
    //    }
    //}

    //public void OnTriggerStay(Collider other)
    //{
    //    hooldLerping = true;
    //    isLerping = false;
    //}
    //public void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("Hand"))
    //    {
    //        hooldLerping = false;
    //    }
    //}

    public void SetFollowBehavior(bool follow)
    {
        if (follow)
        {
            hooldLerping = false;
            isLerping = true;

        }
        else
        {
            hooldLerping = true;
            isLerping = false;

        }


    }
    //public bool isSpawningStart = false;
    public Transform renderableObject;

    private void onVRChange(WebVRState state)
    {

        if (state == WebVRState.ENABLED)
        {
            oldViewingAngle = camTransform.forward;
        }
        else
        {
            oldViewingAngle = camTransform.parent.forward;
           // SetFollowBehavior(true);
        }

        isLerping = true;

    }
    private IEnumerator Start()
    {
        WebVRManager.Instance.OnVRChange += onVRChange;

        if (!renderableObject)
            renderableObject = transform.GetChild(0);

        renderableObject.gameObject.SetActive(false);

        yield return new WaitUntil(() => ClientSpawnManager.Instance.isSpawningFinished);


        //yield return new WaitForSeconds(5);

        if (objectToRotate == null)
        {
            objectToRotate = transform;
        }
        if (camTransform == null)
        {
            camTransform = GameObject.FindWithTag("MainCamera").transform;

            objectToRotate.position = camTransform.parent.TransformPoint(Vector3.forward * 0.5f);

            if (isUseCameraWorldUp)
                objectToRotate.LookAt(2 * objectToRotate.position - camTransform.parent.position, camTransform.parent.up);
            else
                objectToRotate.LookAt(2 * objectToRotate.position - camTransform.parent.position, Vector3.up);
        }

        StartCoroutine(Initiate());


    }

    public bool isUpdating;
    IEnumerator Initiate()
    {

        yield return null;
        renderableObject.gameObject.SetActive(true);
      

        isInitialLook = true;

       
        isUpdating = true;
        //   angleDistanceUntilRotateBack *= 5;
      
    }

    Vector3 locationToRotateTo;
    void LateUpdate()
    {
        if (hooldLerping || !isUpdating)
            return;


        if (isInitialLook)
        {
            isInitialLook = false;
            oldViewingAngle = camTransform.forward;
            curViewingAngle = camTransform.forward;
            objectToRotate.position = camTransform.parent.TransformPoint(Vector3.forward * 0.5f);
      

        }
        else
        {

 

            curViewingAngle =  camTransform.forward; 

            if (Vector3.Angle(oldViewingAngle, curViewingAngle) > angleDistanceUntilRotateBack || isLerping)
            {


               // isLerping = true;

                locationToRotateTo = default;
                Vector3 cameraOriginPosition = default;

                float DistanceSqrd = default;

                //SWITCH VIEW CAMERA FOCUS //IF IT DOES NOT CONSIDER MAIN CAMERA PUT IT ON A LIST AN SEARCH CHILDREN
                if (isInVR.value)
                {
                    locationToRotateTo = camTransform.TransformPoint(Vector3.forward * 0.5f);
                    cameraOriginPosition = camTransform.position;
                    DistanceSqrd = Vector3.Distance(objectToRotate.position, camTransform.TransformPoint(Vector3.forward * 0.5f));//camTransform.position - (camTransform.rotation * (offset * 1)));

                    if (isUseCameraWorldUp)
                        objectToRotate.LookAt(2 * objectToRotate.position - cameraOriginPosition, camTransform.up);
                    else
                        objectToRotate.LookAt(2 * objectToRotate.position - cameraOriginPosition, Vector3.up);

                }
                else
                {
                    locationToRotateTo = camTransform.parent.TransformPoint(Vector3.forward * 0.5f);
                    cameraOriginPosition = camTransform.parent.position;
                    DistanceSqrd = Vector3.Distance(objectToRotate.position, camTransform.parent.TransformPoint(Vector3.forward * 0.5f));// camTransform.parent.position - (camTransform.parent.rotation * (offset * 1)));


                    if (isUseCameraWorldUp)
                        objectToRotate.LookAt(2 * objectToRotate.position - cameraOriginPosition, camTransform.parent.up);
                    else
                        objectToRotate.LookAt(2 * objectToRotate.position - cameraOriginPosition, Vector3.up);
              
                }


                objectToRotate.position = Vector3.Lerp(objectToRotate.position, locationToRotateTo, Time.unscaledDeltaTime * 3f);  //camTransform.position - (rotation * (offset * 1)), Time.unscaledDeltaTime * 3f);


                if (DistanceSqrd < 0.1f)
                {
                    isLerping = false;
                    oldViewingAngle = curViewingAngle;
                }
       

                //if (DistanceSqrd > 0.3f )
                //{
                //    isLerping = true;
                //    //oldViewingAngle = curViewingAngle;
                //}

                //if (DistanceSqrd < 0.3f)
                //{
                //    isLerping = false;
                //    oldViewingAngle = curViewingAngle;
                //}

            }

        }


    }
}


//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class StallAndFollowUI : MonoBehaviour
//{

//    // Use this for initialization
//    //private Transform thisTransform;
//    [SerializeField] private Transform objectToRotate;
//    [SerializeField] private int angleDistanceUntilRotateBack = 80;
//    [SerializeField] private bool isUseCameraWorldUp = true;

//    //private Animator thisAnimator;

//    private Transform camTransform;
//    private GameObject stressMenu;

//    private Vector3 offset;
//    private string curSceneName;

//    Quaternion rotation;

//    bool isInitialLook = false;


//    Vector3 oldViewingAngle;
//    Vector3 curViewingAngle;

//    bool isLerping = false;


//    //bool isButtonAvailable = true;


//    void Awake()
//    {

//        //thisTransform = GetComponent<Transform> ();
//        camTransform = Camera.main.transform;

//        if (objectToRotate == null)
//            objectToRotate = camTransform;


//        offset = objectToRotate.position - camTransform.position;

//    }

//    void OnEnable()
//    {

//        isInitialLook = true;

//        rotation = Quaternion.Euler(0, camTransform.eulerAngles.y, 0);

//        objectToRotate.position = camTransform.position - (rotation * (offset * -1));

//        if (isUseCameraWorldUp)
//            objectToRotate.LookAt(2 * objectToRotate.position - camTransform.position, camTransform.up);
//        else
//            objectToRotate.LookAt(2 * objectToRotate.position - camTransform.position, Vector3.up);
//    }

//    void LateUpdate()
//    {



//        if (isInitialLook)
//        {
//            isInitialLook = false;

//            curViewingAngle = camTransform.forward;

//            //	oldViewingAngle =  Quaternion.Euler(0,90,0) * camTransform.forward ;
//            oldViewingAngle = camTransform.forward;
//            //SetUpInitialLook
//            isLerping = true;

//        }
//        else
//        {

//            curViewingAngle = camTransform.forward;

//            if (Vector3.Angle(oldViewingAngle, curViewingAngle) > angleDistanceUntilRotateBack || isLerping)
//            {

//                isLerping = true;

//                rotation = Quaternion.Euler(0, camTransform.eulerAngles.y, 0);

//                objectToRotate.position = Vector3.Lerp(objectToRotate.position, camTransform.position - (rotation * (offset * -1)), Time.unscaledDeltaTime * 3f);

//                if (isUseCameraWorldUp)
//                    objectToRotate.LookAt(2 * objectToRotate.position - camTransform.position, camTransform.up);
//                else
//                    objectToRotate.LookAt(2 * objectToRotate.position - camTransform.position, Vector3.up);


//                //controlls the smooth step of the rotation;
//                if (Vector3.Distance(objectToRotate.position, camTransform.position - (rotation * (offset * -1))) < 0.1f)
//                {
//                    isLerping = false;
//                    oldViewingAngle = curViewingAngle;
//                }
//            }
//            //Debug.Log (Vector3.Angle (oldViewingAngle, curViewingAngle));
//        }


//    }
//}




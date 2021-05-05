using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InputSelection : MonoBehaviour
{
 //TODO error on handL and handR collider emiting an error on webgl builds (matrix transformation collider issues)
 //TODO animation is what is causing the line renderer to flicker not keep position??

    private LineRenderer lineRenderer;
    public Collider colliderInput;


    public string _tagToLookFor = "UIInteractable";


    public Vector3? newPlayerPos;
    public Vector3? newPlayerEulerRot;

    public bool isFloorDetector;
    public GameObject floorObj;

    //No Interaction for ignore and interaction layer
    int layerMaskIgnore = ~(1 << 2 | 1<<10);
    
    int layerMaskWater = 1 << 9;

    public bool isIgnoreUILayer = false;
    private LayerMask layerIgnoreUI = (1 << 5); //UI IGNORE


    public void OnEnable()
    {
        //if (floorObj)
        //    floorObj.SetActive(true);


        colliderInput.enabled = true;
        
    }
    public void OnDisable()
    {
        if (floorObj)
            floorObj.SetActive(false);

        colliderInput.enabled = false;

    }
    bool isKeepLocation;
    Vector3 locationToKeep;


    public void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        colliderInput = transform.GetComponentInChildren<Collider>(true);
        //   colliderInput = transform.parent.GetComponentInChildren<Collider>(true);

        if (floorObj)
            floorObj.SetActive(false);

        if (isIgnoreUILayer)
            layerMaskIgnore = layerMaskIgnore | layerIgnoreUI;

    
    }
   

    public bool isOverObject;
    public bool isKeepCollision;
    public Button currentButton;
    public void Update()
    {
        if (colliderInput.enabled == false)
            return;

        Vector3 pos = transform.position + (transform.forward * 20);
        lineRenderer.SetPosition(0, transform.position);

        lineRenderer.SetPosition(1, pos);

      //  colliderInput.transform.position = pos;


        if (!isFloorDetector)
        {



            //PLACING THIS HERE AFFECTS WORLD COLLIDER INTERACTIONS WITH LINE RENDERER?
            if (Physics.Linecast(transform.position, pos, out RaycastHit hit, layerMaskIgnore))
            {
                pos = hit.point;

                if (hit.collider.CompareTag(_tagToLookFor) )
                {
                   // isKeepCollision = true;
                    isOverObject = true;

                    colliderInput.transform.position = hit.point;

                }
                else
                {
                    colliderInput.transform.position = hit.point;
                    //    isKeepCollision = false;
                    isOverObject = false;
                }

                
                

            }
            else
            {
                isOverObject = false;
           //     isKeepLocation = false;
            }
            ////else
            ////    colliderInput.transform.position = pos;

            lineRenderer.SetPosition(1, pos);

        }
        else
        {
            //  colliderInput.transform.position = pos;

            //COLLIDERS ON HANDL AND HANDR ARE CAUSING ERROR IN WEBGL BUILD

            //if (floorObj.activeInHierarchy)
            //    floorObj.SetActive(false);

            //LayerMask: "Walkable"
            if (Physics.Linecast(transform.position, pos, out RaycastHit hit, layerMaskWater))//, LayerMask.GetMask("Walkable"), QueryTriggerInteraction.Collide))
            {

                pos = hit.point;


                if (!floorObj.activeInHierarchy)
                    floorObj.SetActive(true);


                floorObj.transform.position = pos;

                newPlayerPos = pos;
        

                colliderInput.transform.position = pos;

                isKeepLocation = true;
                locationToKeep = pos;



            }

            //BREAK CURRENT TELEPORTATION
            if (lineRenderer.GetPosition(1).y > 1.8f)
            {
                pos = transform.position + (transform.forward * 20);
                colliderInput.transform.position = pos;
                newPlayerPos = null;
                floorObj.SetActive(false);

                isKeepLocation = false;
                // lineRenderer.SetPosition(1, Vector3.Lerp(transform.position, pos, 0.5f));
                //  lineRenderer.SetPosition(2, pos);
            }

         
            if (isKeepLocation)
            {

                lineRenderer.SetPosition(2, locationToKeep);
            }
            else
            {
                //  lineRenderer.SetPosition(1, Vector3.Lerp(transform.position, pos, 0.5f));


                lineRenderer.SetPosition(2, pos);
            }
        }



    }
}

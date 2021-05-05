using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_Scale : Trigger_Base
{

    public float attenuation;
    public Vector3 initialPos;
    public Transform _posOfHandLaser;

    public Transform thisTransform;
    public string _interactableTag;

    InputSelection inputSelection;

    public float _magnitudeOfScaling = 3f;

    public float _minScale = 0.01f;

   // private LayerMask layerToIgnore = (1 << 5); //UI IGNORE
    public override void Start()
    {
        inputSelection = transform.parent.GetComponent<InputSelection>();
        _posOfHandLaser = inputSelection.transform;
        _interactableTag = inputSelection._tagToLookFor;
        thisTransform = transform;
    }

    public float initialOffset;
    public Vector3 initialScale;
    public GameObject currentGO;
    public override void OnTriggerEnter(Collider other)
    {
        //if ui skip
        if (other.gameObject.layer == 5)
            return;

        //THIS IS TO AVOID LOOSING CONNECTION (INITIALPOS) TO INITIAL OBJECT - DISABLED REMOVE THE CONNECTION
        if (other.gameObject != currentGO)
            currentGO = other.gameObject;
        else
            return;

        if (other.CompareTag(_interactableTag))
        {
           initialPos = thisTransform.position;
           initialOffset = Vector3.Distance(_posOfHandLaser.position, initialPos);
           initialScale = other.transform.localScale;
        }
    }

    public override void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 5)
            return;

        if (inputSelection.isOverObject)
        {
            attenuation = Vector3.Distance(_posOfHandLaser.position, initialPos) / initialOffset;
            Debug.Log(attenuation - 1);
            //RELATIVE TO CURRENT INITIAL SCALE
            //other.transform.localScale = initialScale + (initialScale * ((attenuation - 1) * _magnitudeOfScaling ));
            //ABSOLUTE?
            other.transform.localScale = initialScale + (Vector3.one * ((attenuation - 1) * _magnitudeOfScaling ));

            other.transform.localScale = new Vector3(Mathf.Max(_minScale, other.transform.localScale.x), Mathf.Max(_minScale, other.transform.localScale.y), Mathf.Max(_minScale, other.transform.localScale.z));
        }
    }
 
    public override void OnDisable()
    {
        currentGO = null;
    }
}

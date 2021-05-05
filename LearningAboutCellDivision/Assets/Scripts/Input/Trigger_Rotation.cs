using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_Rotation : Trigger_Base
{

    public float attenuation;
    public Vector3 initialPos;
    public Transform _posOfHandLaser;

    public Transform thisTransform;
    public string _interactableTag;

    InputSelection inputSelection;

    public float _magnitudeOfRotation = 3;

   // public float _minScale = 0.01f;
    public override void Start()
    {
        inputSelection = transform.parent.GetComponent<InputSelection>();
        _posOfHandLaser = inputSelection.transform;
        _interactableTag = inputSelection._tagToLookFor;
        thisTransform = transform;
    }

    public float initialOffset;
   // public Vector3 initialScale;
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
            //initialScale = other.transform.localScale;
        }
    }

    public override void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 5)
            return;

        if (inputSelection.isOverObject)
        {
            attenuation = Vector3.Distance(_posOfHandLaser.position, initialPos) / initialOffset;
            //Debug.Log(attenuation - 1);
            //TAKE OUT THE ONE START AT ZERO CAN DETERMINE DIRECTION OF ROTATION FORWARD RIGHT BACK LEFT
            other.transform.rotation *= Quaternion.AngleAxis((attenuation -1) * _magnitudeOfRotation, Vector3.up);

          
        }
    }
    public override void OnTriggerExit(Collider other)
    {

    }
    public override void OnDisable()
    {
        currentGO = null;
    }
}

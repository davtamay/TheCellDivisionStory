using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

public class AttachOnTrigger : MonoBehaviour
{
    public UnityEvent onCollisionInvoke;

    public GameObject firstObject;
    PositionConstraint positionConstraint;

    public bool isAttached;

   // public void Start() => triggerObject = GetComponent<PositionConstraint>();

    Vector3 initialOffset;
    Transform triggerObject;
    //public void Start()
    //{
    //    triggerObject = 
    //}
    public void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("MainCamera2"))
            return;
        //if (isAttached)
        //    return;

        isAttached = true;
        //if (other.gameObject == firstObject)
        //    return;
        initialOffset =  other.transform.position - transform.position;//transform.transform.TransformPoint(other.transform.position - transform.position);//transform.position - (transform.rotation * (transform.position // other.transform.TransformPoint(this.transform.position);
        triggerObject = other.transform;                                                              //firstObject = other.gameObject;

        //ConstraintSource cs = new ConstraintSource();


        //cs.sourceTransform = other.transform;
        //cs.weight = 1;

        //positionConstraint.AddSource(cs);
        // onCollisionInvoke.Invoke();
    }

    //public void OnTriggerStay(Collider other)
    //{
      
    //}

    public void Update()
    {
        
        if (isAttached)
        {
            var rotation = triggerObject.rotation * Quaternion.AngleAxis(180, Vector3.up);
            transform.position = triggerObject.position - ((rotation) * (initialOffset * -1));
            //this.transform.position = transform.TransformPoint(initialOffset);
            // this.transform.position = other.transform.TransformPoint(Vector3.forward * 3);

        }
    }
}

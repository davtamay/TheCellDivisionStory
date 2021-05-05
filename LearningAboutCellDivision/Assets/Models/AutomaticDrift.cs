using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticDrift : MonoBehaviour
{
    public bool isStartDrifting;
    public Transform thisTransform;
    public Vector3 posToMove;
    // Start is called before the first frame update
    void Start()
    {
       thisTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(isStartDrifting)
        {
            thisTransform.position = posToMove * Time.deltaTime ; 
        }
    }
}

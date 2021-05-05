using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TESTING_LOCAL_TO_WORLDCHANGES : MonoBehaviour
{
    public Transform worldToLocal;

    public void Start()
    {
        transform.position = transform.InverseTransformPoint(worldToLocal.position);
            //InverseTransformPoint(worldToLocal.position);
        Debug.Log(worldToLocal.position);
    }

}

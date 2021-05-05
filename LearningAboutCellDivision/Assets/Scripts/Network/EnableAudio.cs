using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
public class EnableAudio : MonoBehaviour
{
    Alternate_Button_Function ABF;

    [DllImport("__Internal")]
    private static extern void EnableMicrophone();

    void Start()
    {
        ABF = GetComponent<Alternate_Button_Function>();
        ABF.onFirstClick.AddListener(() => EnableMicrophone());
        ABF.onSecondClick.AddListener(() => EnableMicrophone());
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StateSwitchDependingOnVR : MonoBehaviour
{
    public UnityEvent _inVR_Event;
    public UnityEvent _outOfVR_Event;
    void Start()
    {
        _outOfVR_Event.Invoke();
        WebVRManager.Instance.OnVRChange += onVRChange;
    }
    private void onVRChange(WebVRState state)
    {
 
        if (state == WebVRState.ENABLED)
        {
            _inVR_Event.Invoke();
        }
        else
        {
            _outOfVR_Event.Invoke();
        }

    }
    
}

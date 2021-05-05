using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[System.Serializable]
public class Output_NewVector3_UnityEvent : UnityEvent<Vector3> { }

public class TriggerSelect : Trigger_Base
{
    private bool isOverButton;
    private Button currenButton;
 

    private InputSelection inputSelection;
    private string parentFlagTagMask_ForButtonUI;
    private bool isFloorDetector;

    public Coord_UnityEvent onNewTransformUpdate;


    public override void Start()
    {
        inputSelection = transform.parent.GetComponent<InputSelection>();
        isFloorDetector = inputSelection.isFloorDetector;
      
        if (!isFloorDetector)
            parentFlagTagMask_ForButtonUI = inputSelection._tagToLookFor;

    }

    public override void OnTriggerEnter(Collider other)
    {
        if (!isFloorDetector)
        {
            if (other.CompareTag(parentFlagTagMask_ForButtonUI) && !isOverButton)
            {

                currenButton = other.GetComponent<Button>();
                currenButton.OnSelect(new BaseEventData(EventSystem.current));
                EventSystem.current.SetSelectedGameObject(currenButton.gameObject);
                isOverButton = true;

            }
        }
    }
 
    public override void OnTriggerExit(Collider other)
    {
        if (!isFloorDetector)
        {
            isOverButton = false;
            
            if (currenButton)
            {
                EventSystem.current.SetSelectedGameObject(null);
                currenButton = null;

            }
        }
    }

    //THIS IS WHERE FUNCTIONS ARE INVOKED (ON RELEASE OF TRIGGER BUTTON WHICH DEACTIVATES PARENT OBJECT
    public override void OnDisable()
    {
        if (!isFloorDetector)
        {

            if (isOverButton && currenButton)
            {
                currenButton.onClick.Invoke();

            }

            isOverButton = false;
            currenButton = null;

        }
        else
        {
            if (inputSelection.newPlayerPos != null)
            {
                onNewTransformUpdate.Invoke(new Coords
                {
                
                    pos = (Vector3)inputSelection.newPlayerPos,
                   

                });


            }

        }
           
    }
}

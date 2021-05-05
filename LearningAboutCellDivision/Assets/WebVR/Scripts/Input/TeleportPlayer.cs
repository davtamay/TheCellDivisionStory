using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;



[System.Serializable]
public class TeleportPlayer : MonoBehaviour
{

    public EntityData_SO clientData;
    public float modifyHeight; 


    [Header("States")]
    public Bool_StateCheck_SO canMove;

    Transform player;

    Transform _camera;
    public void Start()
    {
        WebVRManager.Instance.OnVRChange += onVRChange;

      //  canMove.value = false;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        _camera = GameObject.FindGameObjectWithTag("MainCamera").transform;


    }

    private bool isVrActive;
    private void onVRChange(WebVRState state)
    {
        isVrActive = state == WebVRState.ENABLED;
    }


   
    public void UpdatePlayerPosition(Coords newData)
    {
        if (isVrActive && canMove.value)
        {
            var withoutY = newData.pos;

            withoutY.y = transform.position.y + modifyHeight;
            clientData.pos = withoutY;
            
            
            //hands
            player.transform.parent.GetChild(0).localPosition = withoutY;
            //Camera
            player.transform.position = withoutY;

            //Updating to send more often position
            newData.pos = player.transform.position;
           

      //     _onPlayerTransformUpdate.Invoke(newData);
   
        }

    }
    //public IEnumerator ChangePosition(Coords newData)
    //{

    //    var coordData_hand_L = new Coords
    //    {

    //        clientId = NetworkUpdateHandler.Instance.client_id,
    //        pos = hand_L.localPosition, //hand_L.position + newData.pos,
    //        rot = Quaternion.Euler(hand_L.localEulerAngles),
           
    //        entityType = (int) Entity_Type.users_Lhand,


    //    };
    //    _onPlayerTransformUpdate.Invoke(coordData_hand_L);

    //    var coordData_hand_R = new Coords
    //    {

    //        clientId = NetworkUpdateHandler.Instance.client_id,
    //        pos = hand_R.localPosition,//hand_R.position + newData.pos,
    //        rot = Quaternion.Euler(hand_R.localEulerAngles),//hand_R.rotation,
    //        entityType = (int)Entity_Type.users_Rhand,

    //    };

    //    _onPlayerTransformUpdate.Invoke(coordData_hand_R);


    //    yield return new WaitForEndOfFrame();
    //}
    

    public void OnDestroy()
    {
        WebVRManager.Instance.OnVRChange -= onVRChange;
    }
}

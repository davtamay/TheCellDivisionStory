using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class FreeFlightController : MonoBehaviour //IDragHandler, IBeginDragHandler, IEndDragHandler 
    {
    [Tooltip("Enable/disable rotation control. For use in Unity editor only.")]
    public bool rotationEnabled = true;

    [Tooltip("Enable/disable translation control. For use in Unity editor only.")]
    public bool translationEnabled = true;

    private WebVRDisplayCapabilities capabilities;

    [Tooltip("Mouse sensitivity")]
    public float mouseSensitivity = 1f;

    [Tooltip("Straffe Speed")]
    public float straffeSpeed = 5f;

    private float minimumX = -360f;
    private float maximumX = 360f;

    private float minimumY = -90f;
    private float maximumY = 90f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    Quaternion originalRotation;

    public Transform thisTransform;

    bool inDesktopLike {
        get {
            return capabilities.hasExternalDisplay;
        }
    }

    void Start()
    {
        thisTransform = transform;
        WebVRManager.Instance.OnVRChange += onVRChange;
        WebVRManager.Instance.OnVRCapabilitiesUpdate += onVRCapabilitiesUpdate;
        originalRotation = transform.localRotation;

        //TO fix non zero bug in initial input
        yQuaternion = Quaternion.identity;
        xQuaternion = Quaternion.identity;
    }

    private void onVRChange(WebVRState state)
    {
        if (state == WebVRState.ENABLED)
        {
            DisableEverything();
        }
        else
        {
            EnableAccordingToPlatform();
        }
    }

    private void onVRCapabilitiesUpdate(WebVRDisplayCapabilities vrCapabilities)
    {
        capabilities = vrCapabilities;
        EnableAccordingToPlatform();
    }
    //  Quaternion lastRot = Quaternion.identity;
    Quaternion xQuaternion;
    Quaternion yQuaternion;
    public void RotatePlayer(int rotateDirection)
    {
    

            switch (rotateDirection)
        {

            case 1:
                //LEFT
                rotationX += 45;
               // rotationX = ClampAngle(rotationX, minimumX, maximumX);
                xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
             //   thisTransform.localRotation = originalRotation * xQuaternion * yQuaternion;

                break;

            case 2:
                //RIGHT
                rotationX -= 45;
               // rotationX = ClampAngle(rotationX, minimumX, maximumX);
                xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            //    thisTransform.localRotation = originalRotation * xQuaternion* yQuaternion;

                break;

            case 3:
                //UP
                rotationY += 45 ;
                rotationY = ClampAngle(rotationY, minimumY, maximumY);
                yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);
             //   thisTransform.localRotation = originalRotation * xQuaternion * yQuaternion;

                break;

            case 4:
                //DOWN
                rotationY -= 45;
                rotationY = ClampAngle(rotationY, minimumY, maximumY);
                yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);
              //  thisTransform.localRotation = originalRotation * xQuaternion * yQuaternion;

                break;
        }
        


        thisTransform.localRotation = originalRotation * xQuaternion * yQuaternion;
    }
    float rotX = 0f;
    float rotY = 0f;
    public void RotatePlayerWithDelta(int rotateDirection)
    {
       
        var delta = Time.deltaTime * straffeSpeed;



        switch (rotateDirection)
        {

            case 1:
                //LEFT
                rotX += 45 * delta;
                // rotationX = ClampAngle(rotationX, minimumX, maximumX);
                xQuaternion = Quaternion.AngleAxis(rotX, Vector3.up);
                //   thisTransform.localRotation = originalRotation * xQuaternion * yQuaternion;

                break;

            case 2:
                //RIGHT
                rotX -= 45 * delta;
                // rotationX = ClampAngle(rotationX, minimumX, maximumX);
                xQuaternion = Quaternion.AngleAxis(rotX, Vector3.up);
                //    thisTransform.localRotation = originalRotation * xQuaternion* yQuaternion;

                break;

            case 3:
                //UP
                rotY += 45 * delta;
                rotationY = ClampAngle(rotationY, minimumY, maximumY);
                yQuaternion = Quaternion.AngleAxis(rotY, Vector3.left);
                //   thisTransform.localRotation = originalRotation * xQuaternion * yQuaternion;

                break;

            case 4:
                //DOWN
                rotY -= 45 * delta;
                rotationY = ClampAngle(rotationY, minimumY, maximumY);
                yQuaternion = Quaternion.AngleAxis(rotY, Vector3.left);
                //  thisTransform.localRotation = originalRotation * xQuaternion * yQuaternion;

                break;
        }



        thisTransform.localRotation = originalRotation * xQuaternion * yQuaternion;
    }


    public void MovePlayer(int moveDirection)
    {


        switch (moveDirection)
        {

            case 1:

                float x = 1;
                var movement = new Vector3(x, 0, 0);
                movement = thisTransform.TransformDirection(movement);
                thisTransform.position += movement;


                break;

            case 2:

                x = -1;
                movement = new Vector3(x, 0, 0);
                movement = thisTransform.TransformDirection(movement);
                thisTransform.position += movement;

                break;

            case 3:

                float z = 1;// Input.GetAxis("Vertical") * Time.deltaTime * straffeSpeed;

                //     transform.Translate(x, 0, z);
                movement = new Vector3(0, 0, z);
                movement = thisTransform.TransformDirection(movement);
                thisTransform.position += movement;

                break;

            case 4:
                z = -1;// Input.GetAxis("Vertical") * Time.deltaTime * straffeSpeed;

                //     transform.Translate(x, 0, z);
                movement = new Vector3(0, 0, z);
                movement = thisTransform.TransformDirection(movement);
                thisTransform.position += movement;


                break;
        }
    }
#if UNITY_EDITOR
    void Update()
    {


        if (translationEnabled)
        {
            var accumulatedImpactMul = Time.deltaTime * straffeSpeed;
            float x = Input.GetAxis("Horizontal") * accumulatedImpactMul;
            float z = Input.GetAxis("Vertical") * accumulatedImpactMul;

            if (Input.GetKey(KeyCode.Q)) RotatePlayerWithDelta(2);
            if (Input.GetKey(KeyCode.E)) RotatePlayerWithDelta(1);
            if (Input.GetKey(KeyCode.Alpha2)) RotatePlayerWithDelta(3);
            if (Input.GetKey(KeyCode.Alpha3)) RotatePlayerWithDelta(4);


            //     transform.Translate(x, 0, z);
            var movement = new Vector3(x, 0, z);
            movement = thisTransform.TransformDirection(movement);
            thisTransform.position += movement;

        }
       
        
       


        //if (rotationEnabled && Input.GetMouseButtonDown(0))
        //{

        //    thisTransform.localRotation *= lastRot;

        //}




        if (rotationEnabled && Input.GetMouseButton(0))
        {

        //    Input.GetButton("q") ? 

            rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
            rotationY += Input.GetAxis("Mouse Y") * mouseSensitivity;

            rotationX = ClampAngle(rotationX, minimumX, maximumX);
            rotationY = ClampAngle(rotationY, minimumY, maximumY);

            Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);

            thisTransform.localRotation = originalRotation * xQuaternion * yQuaternion;

           
        }
    
       

        //if (rotationEnabled && Input.GetMouseButtonUp(0))
        //{
            //originalRotation = thisTransform.localRotation;

     //   }


    }



#endif

    private enum DraggedDirection
    {
        Up,
        Down,
        Right,
        Left
    }

    private DraggedDirection GetDragDirection(Vector3 dragVector)
    {
        float positiveX = Mathf.Abs(dragVector.x);
        float positiveY = Mathf.Abs(dragVector.y);
        DraggedDirection draggedDir;
        if (positiveX > positiveY)
        {
            draggedDir = (dragVector.x > 0) ? DraggedDirection.Right : DraggedDirection.Left;
        }
        else
        {
            draggedDir = (dragVector.y > 0) ? DraggedDirection.Up : DraggedDirection.Down;
        }
        Debug.Log(draggedDir);
        return draggedDir;
    }

    ////It must be implemented otherwise IEndDragHandler won't work 
    //public void OnDrag(PointerEventData eventData)
    //{
    //    //thisTransform.localRotation = lastPressOrientation;
    //}
    //public void OnBeginDrag(PointerEventData eventData)
    //{

    //    originalRotation = thisTransform.localRotation;
    //   // thisTransform.LookAt(lastPressPos);
    //    //throw new System.NotImplementedException();
    //}
    //private Vector3 lastPressPos;
    //public void OnEndDrag(PointerEventData eventData)
    //{
    //    Debug.Log("Press position + " + eventData.pressPosition);
    //    Debug.Log("End position + " + eventData.position);

    //    lastPressPos = eventData.position;

    //    Vector3 dragVectorDirection = (eventData.position - eventData.pressPosition).normalized;
    //    Debug.Log("norm + " + dragVectorDirection);

    //    DraggedDirection Dir = GetDragDirection(dragVectorDirection);

    //    switch (Dir)
    //    {
    //        case DraggedDirection.Up:
    //        //rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
    //        //rotationX = ClampAngle(rotationX, minimumX, maximumX);
    //        //Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
    //        //thisTransform.localRotation = originalRotation * xQuaternion;

    //        //break;

    //        case DraggedDirection.Down:

    //            rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
    //            rotationX = ClampAngle(rotationX, minimumX, maximumX);
    //            Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
    //            thisTransform.localRotation = originalRotation * xQuaternion;
    //            //lastPressOrientation = thisTransform.localRotation;
    //            break;

    //        case DraggedDirection.Left:

    //        //   break;
    //        case DraggedDirection.Right:

    //            rotationY += Input.GetAxis("Mouse Y") * mouseSensitivity;
    //            rotationY = ClampAngle(rotationY, minimumY, maximumY);
    //            Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);
    //            thisTransform.localRotation = originalRotation * yQuaternion;
    //            //lastPressOrientation = thisTransform.localRotation;
    //            break;


    //    }
    //}

#if !UNITY_EDITOR && UNITY_WEBGL
  void Update()
    {
    

        //if (rotationEnabled && Input.GetMouseButton(0))
        //{

        //    rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
        //    rotationY += Input.GetAxis("Mouse Y") * mouseSensitivity;

        //    rotationX = ClampAngle(rotationX, minimumX, maximumX);
        //    rotationY = ClampAngle(rotationY, minimumY, maximumY);

        //    Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
        //    Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);

        //    thisTransform.localRotation = originalRotation * xQuaternion * yQuaternion;
        //}

        if (translationEnabled)
        {
            float x = Input.GetAxis("Horizontal") * Time.deltaTime * straffeSpeed;
            float z = Input.GetAxis("Vertical") * Time.deltaTime * straffeSpeed;


                 if (Input.GetKey(KeyCode.Q)) RotatePlayerWithDelta(2);
            if (Input.GetKey(KeyCode.E)) RotatePlayerWithDelta(1);
            if (Input.GetKey(KeyCode.Alpha2)) RotatePlayerWithDelta(3);
            if (Input.GetKey(KeyCode.Alpha3)) RotatePlayerWithDelta(4);


         //   transform.Translate(x, 0, z);
        var movement = new Vector3(x, 0, z);
            movement = thisTransform.TransformDirection(movement);
            thisTransform.position += movement;
        }


        // if (rotationEnabled && Input.GetMouseButtonUp(0))
        //{
        //    originalRotation = thisTransform.localRotation;

        //}
    }

#endif
    void DisableEverything()
    {
        translationEnabled = false;
        rotationEnabled = false;
    }

    /// Enables rotation and translation control for desktop environments.
    /// For mobile environments, it enables rotation or translation according to
    /// the device capabilities.
    void EnableAccordingToPlatform()
    {
        rotationEnabled = inDesktopLike || !capabilities.canPresent;
        translationEnabled = inDesktopLike || !capabilities.hasPosition;
    }

    public static float ClampAngle (float angle, float min, float max)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360f)
            angle -= 360f;
        return Mathf.Clamp (angle, min, max);
    }

   
}

using Meta.XR.BuildingBlocks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceFieldHands : MonoBehaviour
{
    [SerializeField] ParticleSystemForceField forceField;
    [SerializeField] Transform handTransform;
    [SerializeField] float fieldStrengthMultiplier;
 
    public enum Hand { Left, Right }

    [SerializeField] Hand selectedHand = Hand.Left;

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = handTransform.position;

        float controllerSpeed;

        // Get the controller speed based on the selected hand
        switch(selectedHand)
        {
            case Hand.Left:
                controllerSpeed = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch).magnitude;

                break;
            case Hand.Right:
                controllerSpeed = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).magnitude;

                break;
            default:
                controllerSpeed = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch).magnitude;

                break;
        }

        // update the force field strength based on the controller speed
        forceField.gravity = controllerSpeed * fieldStrengthMultiplier;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Register the body dimensions of the user
public class BodyLengthRecorder : MonoBehaviour
{
    [SerializeField] Transform headTransform;
    [SerializeField] Transform leftHandTransform;
    [SerializeField] Transform rightHandTransform;

    public float stretchedArmLength = 1f;
    public float standingHeadHeight = 1f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.JoystickButton2))
        {
            Debug.Log("Recording Body Data");
            setHeadHeight();
            setArmLength();
        };
    }

    void setHeadHeight()
    {
        standingHeadHeight = headTransform.position.y;
    }

    void setArmLength()
    {
        stretchedArmLength = Vector3.Magnitude(leftHandTransform.position - rightHandTransform.position);
    }
}

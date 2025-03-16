using InteractML;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GestureController : MonoBehaviour
{
    [PullFromIMLGraph]
    public int SetLastRecordedGesture;

    [SerializeField] PitchDetectDemo audioController;
    [SerializeField] TMP_Text debugText;
    [SerializeField] float handControllerSpeedSmoothingCoef = 0.01f;
    [SerializeField] float volumeSmoothingCoef = 0.01f;
    [SerializeField] float modelTriggerSpeed = 1.1f;
    [SerializeField] float modelTriggerVolume = 0.1f;

    public string[] gestureNames;

    public int currentActiveGesture;

    public float smoothedHandControllerSpeed = 0f;
    public float smoothedVolume = 0f;

    void FixedUpdate()
    {
        updateSmoothedHandControllerSpeed();
        updateSmoothedVolume();

        if (smoothedHandControllerSpeed >= modelTriggerSpeed && smoothedVolume >= modelTriggerVolume)
        {
            debugText.text = gestureNames[SetLastRecordedGesture];
            currentActiveGesture = SetLastRecordedGesture;
        }
        else
        {
            currentActiveGesture = 0;
        }
    }

    private void updateSmoothedHandControllerSpeed()
    {
        smoothedHandControllerSpeed += handControllerSpeedSmoothingCoef * (averageHandControllerSpeed() - smoothedHandControllerSpeed);
    }

    private void updateSmoothedVolume()
    {
        smoothedVolume += volumeSmoothingCoef * (currentVolume() - smoothedVolume);
    }

    private float averageHandControllerSpeed()
    {
        float leftHandControllerSpeed = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch).magnitude;
        float rightHandControllerSpeed = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).magnitude;

        return (leftHandControllerSpeed + rightHandControllerSpeed) / 2f;
    }

    private float currentVolume()
    {
        return audioController.volume;
    }
}

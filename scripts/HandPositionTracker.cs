using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class handPositionTracker : MonoBehaviour
{
    [SerializeField] Transform handTransform;
    [SerializeField] Transform headTransform;
    [SerializeField] BodyLengthRecorder bodyLength;

    // Update is called once per frame
    void FixedUpdate()
    {
            transform.position = headTransform.InverseTransformPoint(handTransform.position)/bodyLength.stretchedArmLength;
    }
}

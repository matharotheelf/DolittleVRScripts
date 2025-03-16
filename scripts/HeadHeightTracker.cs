using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadHeightTracker : MonoBehaviour
{
    [SerializeField] Transform headTransform;
    [SerializeField] BodyLengthRecorder bodyLength;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(0, headTransform.position.y/bodyLength.standingHeadHeight, 0);
    }
}

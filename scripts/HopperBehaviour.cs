using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HopperBehaviour : MonoBehaviour
{
    public float hopHeight = 1.0f;
    public float baseHeight = 1.0f;
    public float currentAnimationHeight = 0f;
    [SerializeField] Transform bodyTransform;
    [SerializeField] float hopHeightSmoothingCoefficient = 0.4f;

    private float smoothedHopHeight = 1.0f;


    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateSmoothedHopHeight();
        bodyTransform.localPosition = new Vector3(0, smoothedHopHeight * currentAnimationHeight + baseHeight);
    }

    private void UpdateSmoothedHopHeight()
    {
        smoothedHopHeight += hopHeightSmoothingCoefficient * (hopHeight - smoothedHopHeight);
    }
}

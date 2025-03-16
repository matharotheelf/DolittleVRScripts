using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SnakeBehaviorScript : MonoBehaviour
{
    [SerializeField] GameObject[] snakeBody;
    public float wiggleDuration = 1f;
    public float wiggleMagnitude = 1f;
    public float snakeHeight = 2f;
    public float snakeHeightSmoothingCoefficient = 0.4f;
    public float wiggleMagnitudeSmoothingCoefficient = 0.4f;

    private float phase = 0f;
    private float smoothedSnakeHeight = 2f;
    private float smoothedWiggleMagnitude = 1f;

    void FixedUpdate()
    {
        Wiggle();
        UpdateSmoothedSnakeHeight();
        UpdateSmoothedWiggleMagnitude();

    }
    private void Wiggle()
    {
        phase = (Time.fixedTime/wiggleDuration) % 1;

        for (int snakeBodyIndex = 0; snakeBodyIndex < snakeBody.Length; snakeBodyIndex++)
        {
            float partialBodyPosition = (float)snakeBodyIndex / snakeBody.Length;

            snakeBody[snakeBodyIndex].transform.localPosition = new Vector3(snakeBody[snakeBodyIndex].transform.localPosition.x, WiggleHeight(partialBodyPosition), WiggleWidth(partialBodyPosition));
        }
    }

    private float WiggleWidth(float partialBodyPosition)
    {
        return smoothedWiggleMagnitude * Mathf.Sin(2f * Mathf.PI * (phase + partialBodyPosition));
    }

    private float WiggleHeight(float partialBodyPosition)
    {
        return (smoothedSnakeHeight / (2 - Mathf.Pow(partialBodyPosition, 2))) - smoothedSnakeHeight / 2;
    }

    private void UpdateSmoothedSnakeHeight()
    {
        smoothedSnakeHeight += snakeHeightSmoothingCoefficient * (snakeHeight - smoothedSnakeHeight);
    }

    private void UpdateSmoothedWiggleMagnitude()
    {
        smoothedWiggleMagnitude += wiggleMagnitudeSmoothingCoefficient * (wiggleMagnitude - smoothedWiggleMagnitude);
    }

}

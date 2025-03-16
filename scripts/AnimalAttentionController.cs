using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

// Controls the attention the user has of each inidividual animal
public class AnimalAttentionController : MonoBehaviour
{
    [SerializeField] GestureController gestureController;
    [SerializeField] Transform playerTransform;
    [SerializeField] AnimalMovement animalMovement;
    [SerializeField] int gestureActivationIndex;
    [SerializeField] float attentionChangeSpeed = 0.01f;
    [SerializeField] float attentionGainedThreshold = 0.2f;
    [SerializeField] float attentionLostThreshold = 0.05f;
    [SerializeField] float attentionLossDivisor = 7.5f;
    [SerializeField] float attentionStayDuration = 45f;
    [SerializeField] string runningAnimationState = "isRunning";

    public float attentionParameter = 0f;
    public enum AttentionState { Wandering, AttractedTowards, FullAttention, LostAttention, LosingAttention };
    private AttentionState currentAttentionState = AttentionState.Wandering;
    private float fieldOfView = 60.0f;
    private float callRange = 20.0f;
    private RaycastHit hit;


    void FixedUpdate()
    {
        // if the animal is in a state where the user can attract its attention
        // call the functions which update the attention state
        switch (currentAttentionState)
        {
            case AttentionState.Wandering:
                attentionUpdate();

                triggerAttractedTowards();
                break;
            case AttentionState.LosingAttention:
                attentionUpdate();

                triggerAttentionLost();
                triggerAttractedTowards();
                break;
            default:
                break;
        }
    }
    private void attentionUpdate()
    {
        // either gain or lose animal attention depending on the active gesture
        if (gestureActivationIndex == gestureController.currentActiveGesture)
        {
            attentionParameter += attentionChangeSpeed;
        }
        else
        {
            attentionParameter -= attentionChangeSpeed / attentionLossDivisor;
        }

        attentionParameter = Math.Clamp(attentionParameter, 0f, 1f);
    }

    private void triggerAttractedTowards()
    {
        // attract animal if within range and if has gained attention
        if (attentionParameter >= attentionGainedThreshold && isInAttractionRange())
        {
            currentAttentionState = AttentionState.AttractedTowards;
            animalMovement.runToUser();
        }
    }

    private void triggerAttentionLost()
    {
        // lose animal attention if attention is below a threshold
        if (attentionParameter <= attentionLostThreshold)
        {
            currentAttentionState = AttentionState.LostAttention;
            animalMovement.runAwayFromUser();
        }
    }
    public void runTowardsComplete()
    {
        // animal has run towards the user so can start playing with the user
        currentAttentionState = AttentionState.FullAttention;
        animalMovement.startPlaying();
        StartCoroutine(startAttentionLoss());
    }

    public void runAwayComplete()
    {
        // after the animal has completed playing it runs away
        currentAttentionState = AttentionState.Wandering;
        animalMovement.startWandering();
    }

    public AttentionState getAttentionState()
    {
        return currentAttentionState;
    }

    public bool isAttracted()
    {
        return currentAttentionState == AttentionState.AttractedTowards;
    }

    public bool isLostAttention()
    {
        return currentAttentionState == AttentionState.LostAttention;
    }

    public bool isFullAttention()
    {
        return currentAttentionState == AttentionState.FullAttention;
    }

    IEnumerator startAttentionLoss()
    {
        // after the animal has played with the user for a while it starts to lose attention
        yield return new WaitForSeconds(attentionStayDuration);

        attentionParameter = attentionGainedThreshold / 2f;
        currentAttentionState = AttentionState.LosingAttention;
    }

    private bool isInAttractionRange() {
        // detects whether animal is within range to start gaining attention
        if (Vector3.Angle(transform.position - playerTransform.position, playerTransform.forward) <= fieldOfView) {
            if (Physics.Raycast(playerTransform.position, transform.position - playerTransform.position, out hit, callRange)) {
                if (ReferenceEquals(hit.collider.gameObject, gameObject))
                {
                    return true;
                }
            }
        }
        return false;
    }
}

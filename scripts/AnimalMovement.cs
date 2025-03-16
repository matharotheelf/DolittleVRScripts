using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;
using static AnimalAttentionController;

// script to determine the movement of animal both for dance and locomotion
public class AnimalMovement : MonoBehaviour
{
    [SerializeField] AnimalAttentionController attentionController;
    [SerializeField] NavMeshAgent navMeshAgent;
    [SerializeField] GameObject navMeshSurface;
    [SerializeField] Transform userTransform;
    [SerializeField] Transform animalHeadTransform;
    [SerializeField] Transform userLeftHandTransform;
    [SerializeField] Transform userRightHandTransform;

    [SerializeField] float stepRange = 5f;
    [SerializeField] float interactionRange = 2f;
    [SerializeField] float runAwayRange = 10f;
    [SerializeField] float pathEndThreshold = 0.1f;

    [SerializeField] string runningAnimationState = "isRunning";
    [SerializeField] float runningTurnSpeed = 120;
    [SerializeField] float runningMovementSpeed = 5;

    [SerializeField] string walkingAnimationState = "isWalking";
    [SerializeField] float walkingTurnSpeed = 120;
    [SerializeField] float walkingMovementSpeed = 1;

    [SerializeField] string playingAnimationState = "isIdling";
    [SerializeField] float playingTurnSpeed = 120;
    [SerializeField] float playingMovementSpeed = 0.1f;

    [SerializeField] string interactionAnimationState = "isIdling";
    [SerializeField] float interactionTurnSpeed = 120;
    [SerializeField] float interactionMovementSpeed = 1;

    [SerializeField] float circlingRadius = 120;
    [SerializeField] float currentCircleAngle = 0;
    [SerializeField] float circlingAngularSpeed = 5f;
    [SerializeField] float dippingThreshold = 1f;
    [SerializeField] float dippingAnimationMultiple = 1f;

    [SerializeField] float smoothedHandControllerSpeed = 0f;
    [SerializeField] float handControllerSpeedSmoothingCoef = 0.4f;

    [SerializeField] float snakeHeightMultiple = 5f;
    [SerializeField] float snakeWiggleMultiple = 1;
    [SerializeField] float snakeWiggleDurationMultiple = 1f;

    [SerializeField] float hoppingMultiple = 5f;
    [SerializeField] float hoppingThreshold = 1f;
    [SerializeField] float hoppingRadius = 4f;

    [SerializeField] ParticleSystem swarmParticleSystem;

    [SerializeField] Animator animalAnimator;
    [SerializeField] SnakeBehaviorScript snakeBehavior;
    [SerializeField] HopperBehaviour hopperBehaviour;

    private Vector3 nextPoint;
    private Vector3 homePoint;
    private bool hasPath = false;

    private bool isSwarming = false;

    public enum InteractionBehaviour { Circling, Hopping, Swarming, Snaking, None };
    [SerializeField] InteractionBehaviour selectedInteraction = InteractionBehaviour.Circling;

    // Random point generator within the range of the shark for shark random path
    bool RandomPoint(Vector3 center, float sRange, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.Range(sRange / 2, sRange) * transform.forward + Random.Range(-sRange, sRange) * transform.right;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 4.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }

    // Bool which returns whether a shark has reached the end of its path
    bool AtEndOfPath()
    {
        hasPath |= navMeshAgent.hasPath;
        if (hasPath && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + pathEndThreshold)
        {
            // Arrived
            hasPath = false;

            return true;
        }

        return false;
    }

    // Bool which returns whether shark is next to the navmesh edge
    bool AtEdgeOfMesh()
    {
        UnityEngine.AI.NavMeshHit hit;
        float distanceToEdge = 1;

        if (UnityEngine.AI.NavMesh.FindClosestEdge(transform.position, out hit, NavMesh.AllAreas))
        {
            distanceToEdge = hit.distance;
        }

        return distanceToEdge < 1f;
    }

    // Generates next destination for animal
    void GenerateDestination()
    {
        if (AtEdgeOfMesh())
        {
            nextPoint = homePoint;
        }
        else
        {
            // If the animal is at the adge of the navmesh
            RandomPoint(transform.position, stepRange, out nextPoint);
        }

        navMeshAgent.destination = nextPoint;
        hasPath = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        startWandering();
        homePoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        updateSmoothedHandControllerSpeed();

        // if animal has reached the end of its path generate the next point
        if (AtEndOfPath() || !hasPath)
        {
            if (attentionController.isAttracted())
            {
                attentionController.runTowardsComplete();
            } else if(attentionController.isLostAttention())
            {
                attentionController.runAwayComplete();
            }

            if (attentionController.isFullAttention())
            {
                GenerateInteractionDestination();
            } else
            {
                GenerateDestination();
            }
        }

        // if the animal is in the correct state generate interaction with the user
        if (attentionController.isFullAttention())
        {
            GenerateInteraction();
        } else if(attentionController.isLostAttention())
        {
            EndInteraction();
        }
    }

    // when the animal has gained the users attention it runs towards the user
    public void runToUser()
    {
        startRunningAnimation();

        RandomPoint(userTransform.position + interactionRange*userTransform.forward, interactionRange, out nextPoint);

        navMeshAgent.SetDestination(nextPoint);
    }

    //When the animal has lost the users attention it runs away from the user
    public void runAwayFromUser()
    {
        startRunningAnimation();

        RandomPoint(userTransform.position + runAwayRange*transform.forward, interactionRange, out nextPoint);

        navMeshAgent.SetDestination(nextPoint);
    }

    private void startRunningAnimation()
    {
        animalAnimator.SetBool(runningAnimationState, true);
        animalAnimator.SetBool(playingAnimationState, false);
        animalAnimator.SetBool(walkingAnimationState, false);

        navMeshAgent.speed = runningMovementSpeed;
        navMeshAgent.angularSpeed = runningTurnSpeed;
    }

    public void startWandering()
    {
        animalAnimator.SetBool(runningAnimationState, false);
        animalAnimator.SetBool(playingAnimationState, false);
        animalAnimator.SetBool(walkingAnimationState, true);

        navMeshAgent.speed = walkingMovementSpeed;
        navMeshAgent.angularSpeed = walkingTurnSpeed;
    }

    public void startPlaying()
    {
        animalAnimator.SetBool(walkingAnimationState, false);
        animalAnimator.SetBool(runningAnimationState, false);
        animalAnimator.SetBool(playingAnimationState, true);

        navMeshAgent.speed = playingMovementSpeed;
        navMeshAgent.angularSpeed = playingTurnSpeed;
    }

    private void updateSmoothedHandControllerSpeed()
    {
        smoothedHandControllerSpeed += handControllerSpeedSmoothingCoef * (averageHandControllerSpeed() - smoothedHandControllerSpeed);
    }

    private float averageHandControllerSpeed()
    {
        float leftHandControllerSpeed = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch).magnitude;
        float rightHandControllerSpeed = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).magnitude;

        return (leftHandControllerSpeed + rightHandControllerSpeed) / 2f;
    }

    public void GenerateInteractionDestination()
    {
        Vector3 nextDestination;

        // generate movement for interaction depending on which behaviour the animal has
        switch (selectedInteraction)
        {
            case InteractionBehaviour.Circling:
                nextDestination = userTransform.TransformPoint(circlingRadius * ( Quaternion.AngleAxis(currentCircleAngle, Vector3.up)*Vector3.right + Vector3.forward));

                navMeshAgent.SetDestination(nextDestination);
                currentCircleAngle += circlingAngularSpeed;

                navMeshAgent.speed = interactionMovementSpeed;
                navMeshAgent.angularSpeed = interactionTurnSpeed;
                break;
            case InteractionBehaviour.Hopping:
                if (smoothedHandControllerSpeed > hoppingThreshold)
                {
                    RandomPoint(userTransform.position + hoppingRadius * userTransform.forward, hoppingRadius, out nextDestination);

                    navMeshAgent.SetDestination(nextDestination);

                    navMeshAgent.speed = interactionMovementSpeed * Mathf.Max(0.1f, hoppingMultiple * smoothedHandControllerSpeed);
                    navMeshAgent.angularSpeed = interactionTurnSpeed;
                }

                break;
            default:
                break;
        }
    }

    private void GenerateInteraction()
    {
        Vector3 horizontalLookPosition;

        // generate interaction depending on which interaction behavior the animal has
        switch (selectedInteraction)
        {
            case InteractionBehaviour.Circling:
                if (smoothedHandControllerSpeed > dippingThreshold && !animalAnimator.GetBool("isDipping"))
                {
                    animalAnimator.SetBool("isDipping", true);
                    animalAnimator.speed = smoothedHandControllerSpeed * dippingAnimationMultiple;
                }

                break;
            case InteractionBehaviour.Swarming:
                var externalForces = swarmParticleSystem.externalForces;

                if (!externalForces.enabled)
                {
                    externalForces.enabled = true;
                }

                navMeshAgent.speed = 0f;
                navMeshAgent.angularSpeed = 0f;

                break;
            case InteractionBehaviour.Snaking:
                // look at user position on xy

                horizontalLookPosition = new Vector3(userTransform.position.x, transform.position.y, userTransform.position.z);

                transform.LookAt(horizontalLookPosition);

                // head look at head transform directly

                animalHeadTransform.LookAt(userTransform.position);

                // head height if from hand height

                snakeBehavior.snakeHeight = Mathf.Max(1f, snakeHeightMultiple * (userLeftHandTransform.position.y + userRightHandTransform.position.y) / 2);

                // speed wiggle is from hand controller speed

                snakeBehavior.wiggleMagnitude = Mathf.Min(1.5f, snakeWiggleMultiple * smoothedHandControllerSpeed);
                break;
            case InteractionBehaviour.Hopping:
                // look at user position on xy

                horizontalLookPosition = new Vector3(userTransform.position.x, transform.position.y, userTransform.position.z);

                transform.LookAt(horizontalLookPosition);

                // head look at head transform directly

                animalHeadTransform.LookAt(userTransform.position);

                if (smoothedHandControllerSpeed > hoppingThreshold)
                {
                    hopperBehaviour.hopHeight = Mathf.Max(0.1f, hoppingMultiple * smoothedHandControllerSpeed);
                } else
                {

                    hopperBehaviour.hopHeight = 0.1f;
                }
                break;
            default:
                break;
        }
    }

    private void EndInteraction()
    {
        // reset behaviour to default after interaction is complete/
        switch (selectedInteraction)
        {
            case InteractionBehaviour.Circling:
                animalAnimator.SetBool("isDipping", false);

                break;
            case InteractionBehaviour.Snaking:
                snakeBehavior.snakeHeight = 1f;
                snakeBehavior.wiggleMagnitude = 1f;
                animalHeadTransform.LookAt(transform.forward);

                break;
            case InteractionBehaviour.Hopping:
                hopperBehaviour.hopHeight = 1f;
                animalHeadTransform.LookAt(transform.forward);

                break;
            case InteractionBehaviour.Swarming:
                var externalForces = swarmParticleSystem.externalForces;

                externalForces.enabled = false;
                break;
            default:
                break;
        }
    }

    public void CompleteDip()
    {
        animalAnimator.SetBool("isDipping", false);
        animalAnimator.speed = 1f;
    }
}

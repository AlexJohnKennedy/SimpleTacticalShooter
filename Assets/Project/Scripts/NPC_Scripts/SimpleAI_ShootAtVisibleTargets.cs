using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]    // The AI script needs to be able to command the agent controller, to move the Agent.
[RequireComponent(typeof(ICharacterDetector))]    // The AI script will want to listen for vision events, so it knows what the agent can 'see'.
[RequireComponent(typeof(IGunMechanics))]         // The AI script will want to be able to fire it's gun.
public class SimpleAI_ShootAtVisibleTargets : MonoBehaviour {

    public float standStillThreshold;           // How far away the target has to be for the AI to stand still to try and shoot.
    public float aimAngleThreshold_Degrees;     // How close our aim point has to be to the target for the AI to try and shoot.
    public float standingShootIntervalSeconds;  // How frequently the AI fires his gun when standing still.
    public float walkingShootIntervalSeconds;   // How frequently the AI fires his gun when walking.
    public float reactionTime;                  // How long it takes the AI to 'notice' or 'react' to a new target.

    public float timeToAimSeconds;    // Used to control how quickly the character can rotate and aim!
    public float maxTurningSpeedWhileAiming;    // Degrees per second, maximum angular velocity for the character while turning. 

    public GameObject characterBody;    // The thing which we will rotate (instead of the navagent object)

    private NavMeshAgent agent;
    private ICharacterDetector perception;
    private IGunMechanics gun;
    private Collider newTarget;    // Used so we continue shooting at old target, if we are 'reacting' to a new target.
    private Collider currentTarget;

    private float nextShootTime;
    private float nextReactTime;
    private bool reacting;

    private float currBodyAngularVelocity;

    // Use this for initialization
    void Start() {
        // Register to listen for events
        perception = GetComponent<ICharacterDetector>();
        perception.VisionUpdatedEvent += HandleNewPerception;
        gun = GetComponent<IGunMechanics>();
        agent = GetComponent<NavMeshAgent>();
        currentTarget = newTarget = null;   // Need to acquire a target(s) first.
        reacting = true;
        nextShootTime = Time.time;  // Can fire right away!
        currBodyAngularVelocity = 0f;
    }

	// Update is called once per frame - NOTE: shooting timing should probably be done in fixed update, so that gun firing is not FPS dependent.. But oh well handle that shit when you do it properly.
	void Update () {
		// If we are 'reacting', that means that we just spotted a new target. After 'reaction time' has elapsed, the newTarget will become the current target, and the 'newTarget' becomes null.
        if (reacting && Time.time >= nextReactTime) {
            //REACT!
            reacting = false;
            currentTarget = newTarget;
            newTarget = null;
        }

        // If we have a target, then rather than just walking around, we should lookat, and shoot at, our target!
        if (currentTarget) {
            AimTowardsTarget(currentTarget);
            
            // If the target is further away than the stand still threshold, then we should stop moving to aim carefully!
            if (Vector3.Distance(transform.position, currentTarget.transform.position) > standStillThreshold) {
                agent.isStopped = true; //Pause the agent's path.
            }

            AttemptToShoot(currentTarget);
        }
        else {
            agent.isStopped = false; // Resume walking if there is no target to worry about!
            AimTowardsWalkDirection();  // Turn the body back in the direction of the nav agent's facing direction!
        }
	}

    // Function to define how quickly the unity will aim towards the target.
    private void AimTowardsTarget(Collider target) {
        Vector3 targetDirection = target.transform.position - characterBody.transform.position; //Points towards the target directly.

        // Get a rotation which points in the target direction
        Quaternion pointsToTarget = Quaternion.LookRotation(targetDirection);   // Default upwards direction is Vector3.Up

        targetDirection.y = 0;  //Remove vertical component, so that the body remains upright.

        // Get a rotation which points in the target direction, but with no vertical component
        Quaternion pointsToTargetNoVertical = Quaternion.LookRotation(targetDirection);

        float angleDelta = Quaternion.Angle(characterBody.transform.rotation, pointsToTarget);

        // Calculate the interpolation factor, so we can slerp the angle and apply a smoothdamp ro
        if (angleDelta > 0.0f) {
            float interpFactor = Mathf.SmoothDampAngle(angleDelta, 0.0f, ref currBodyAngularVelocity, timeToAimSeconds, maxTurningSpeedWhileAiming);
            interpFactor = 1.0f - interpFactor / angleDelta;
            characterBody.transform.rotation = Quaternion.Slerp(characterBody.transform.rotation, pointsToTargetNoVertical, interpFactor);
        }
    }

    private void AimTowardsWalkDirection() {
        // Simply rotate smoothly back towards a local rotation of zero (to re-align with the nav agent)
        if (characterBody.transform.localRotation == Quaternion.identity) {
            return;     // We are already aligned!
        }
        else {
            float angleDelta = Quaternion.Angle(characterBody.transform.localRotation, Quaternion.identity);

            // Calculate the interpolation factor, so we can slerp the angle and apply a smoothdamp ro
            if (angleDelta > 0.0f) {
                float interpFactor = Mathf.SmoothDampAngle(angleDelta, 0.0f, ref currBodyAngularVelocity, timeToAimSeconds, maxTurningSpeedWhileAiming);
                interpFactor = 1.0f - interpFactor / angleDelta;
                characterBody.transform.localRotation = Quaternion.Slerp(characterBody.transform.localRotation, Quaternion.identity, interpFactor);
            }
        }
    }

    private void AttemptToShoot(Collider target) {
        //Shoot at a target if we are looking sufficiently 'close' to the point, and if our 'next shoot time' has been reached.
        if (CheckAimAngle(target) < aimAngleThreshold_Degrees && Time.time >= nextShootTime && gun.CanFireAgain()) {
            // Shoot at that boy!
            gun.Fire(agent.velocity.magnitude);
            
            // Reset shooting wait timer.
            if (agent.velocity.magnitude > 0.1f) {
                nextShootTime = Time.time + walkingShootIntervalSeconds;
            }
            else {
                nextShootTime = Time.time + standingShootIntervalSeconds;
            }
        }
    }

    private float CheckAimAngle(Collider target) {
        //Calculate the angle between our left right orientation, and our target.
        Vector3 pointsToTarget = target.transform.position - transform.position;
        pointsToTarget.y = 0;   //Remove the vertical component.

        Vector3 forwardNoVertical = transform.forward;
        forwardNoVertical.y = 0;

        float angle = Vector3.Angle(forwardNoVertical, pointsToTarget);
        return ((angle < 0) ? -angle : angle);  //Make sure to return positive value.
    }

    // Handler logic - will be invoked every time we get new information about what characters we can see.
    private void HandleNewPerception(object sender, List<Collider> visibleColliders) {
        // Treat all visible characters as enemies. 
        // Target the nearest visible character.
        if (visibleColliders.Count > 0) {
            Collider closest = visibleColliders[0];
            float currDist = Vector3.Distance(transform.position, closest.transform.position);
            visibleColliders.RemoveAt(0);
            foreach(Collider c in visibleColliders) {
                float d = Vector3.Distance(transform.position, c.transform.position);
                if (d < currDist) {
                    currDist = d;
                    closest = c;
                }
            }
            UpdateTargetCollider(closest);
        }
        else {
            currentTarget = newTarget = null;
        }
    }

    // Keeps the 'current target' variable up to date, and also checks whether or not the target aquired was a new one.
    private void UpdateTargetCollider(Collider c) {
        if (c == null) return;
        if (currentTarget != c) {
            // The new target is not the same as the current target!
            // If we aren't already reacting to this target, then we need to set up a 'reaction' timer for it.
            if (newTarget != c) {
                newTarget = c;
                reacting = true;
                nextReactTime = Time.time + reactionTime;
            }
        }
        else {
            // The target is the same one. No need to do anything...
        }
    }
}

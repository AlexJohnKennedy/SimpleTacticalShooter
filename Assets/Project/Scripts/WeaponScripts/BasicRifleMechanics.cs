using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HelperFunctions;

public class BasicRifleMechanics : MonoBehaviour, IGunMechanics {

    [Header("Reference points for aiming and firing")]
    public Transform pivotPoint;
    public Transform muzzlePoint;

    [Header("Projectile Attributes")]
    public LayerMask hittableLayers;
    public float bulletDamage;

    [Header("Firerate")]
    [Tooltip("How long the gun must wait after firing a shot before it can fire again!")]
    public float fireRate_waitTime;

    // --- ACCURACY AND RECOIL MODEL --- //
    // The accuracy and control of the weapon is modelled using two 'factors'.
    // 'Inaccuracy' is the cone of 'randomness' (spread) that could occur. It is represented as a range of possible angles to offset from the aim point in any direction.
    // 'Recoil' is an offset which is applied to where the player/character is 'aiming' to where the actual aim-point of the gun is.
    // Recoil is cumulative: Each time a shot is fired, the aim-point is 'shifted' by an additional offset amount. Whenever the gun is not firing, the aim-point gradually
    // moves back towards an offset of zero (until the aim-point is exactly where the user is 'pointing').
    // Each shot can also add an amount of additional inaccuracy, which accumulates and similarly gradually reduces whenever the gun is not firing.
    // Movement speed of the character scales all of the recoil and innaccuracy parameters!
    // The recoil offsets per-shot are based on a 'pattern' object, which is generic and customisable. This will determine the direction and base magnitude of the recoil
    // offset vector, for each shot in the pattern. Shooting bursts will progress the recoil system through 'the pattern', and while not firing, the pattern will scale back until
    // we get back to the start of the recoil pattern.

    [Header("Accuracy settings")]

    [Tooltip("The range in degrees that the random direction offset can be when firing")]
    public float baseInaccuracyDegrees;
    [Tooltip("The maximum inaccuracy for any shot fired, BEFORE movement scaling is applied")]
    public float maxUnscaledInaccuracyDegrees;
    [Tooltip("The absolute maximum inaccuracy for any shot fired, even if moving at infinite speed")]
    public float maxScaledInaccuracyDegrees;
    [Tooltip("Scaling for how much inaccuracy is added by the character's movement speed")]
    public float movementInaccuracyScaleFactor;
    [Tooltip("How much inaccuracy in degrees should be added per shot, due to recoil")]
    public float recoilInaccuracyAddAmount;
    [Tooltip("How quickly the current inaccuracy reduces back to base inaccuracy, in degrees per second")]
    public float inaccuracyRecoveryRate;


    [Header("Recoil and recoil pattern settings")]

    [Tooltip("Factor for scaling the magnitude of the recoil pattern offsets")]
    public float recoilOffsetBaseScaleFactor;
    [Tooltip("Base randomness (Degrees) to add to each recoil offset direction angle")]
    public float baseRecoilDirectionRandomness;
    [Tooltip("The maximum angle the recoil-aimpoint-offset can be from the base aim point")]
    public float maxRecoilOffsetAngle;
    [Tooltip("Scaling for how much the magnitude of the recoil is increased by the character's movement speed")]
    public float movementRecoilMagnitudeScaleFactor;
    [Tooltip("Scaling for how much the random direction of the recoil is increased by the character's movement speed")]
    public float movementRecoilDirectionRandomnessScaleFactor;
    [Tooltip("How quickly the recoil-aimpoint-offset reduces back to zero, in degrees per second")]
    public float recoilRecoveryRate;
    [Tooltip("How long the gun has to wait after firing until the recoil pattern starts to recover. This should generally by at least as long as the fire-rate interval, or else recoil pattern recovery may be inconsistent")]
    public float recoilPatternContinuousFireWindow;
    [Tooltip("How long it takes to step back one 'shot' in the recoil pattern, once the gun has not been fired for at least recoil-pattern-continuous-fire-window seconds")]
    public float recoilPatternRecoveryTime;
    [Tooltip("How long it takes to 'hard reset' back to the start of the recoil pattern, no matter where you were in the pattern")]
    public float recoilPatternHardResetTime;

    // STATE VARIABLES FOR THE GUN ACCURACY AND RECOIL.
    private float currAddititonalInaccuracy;    // Represents the current amount of inaccuracy in degrees.
    private Quaternion currAimpointOffset;      // Represents the offset to the 'true' aimpoint direction vector as a rotation.
    private IRecoilPattern patternObj;
    private int currPatternIndex;               // Which 'shot' we are up to in the pattern.

    // Generic functions which define how innaccuracy and recoil recovers, relative to current inaccuracy and current recoil offset
    private Func<float, float, float, float> inaccuracyRecoveryFormula;     // (float currentAdditionalInaccuracy, float recoveryRateValue, float deltaTime)
    private Func<Quaternion, float, float, float> recoilRecoveryFormula;    // (float currentOffset, float recoveryRateValue, float deltaTime)

    // Timers
    private float nextPatternStartReducingTime;
    private float nextPatternReduceTime;
    private float nextPatternResetTime;
    private float nextFireAgainTime;

    // Use this for initialization
    void Start() {
        currAddititonalInaccuracy = 0f;
        currAimpointOffset = Quaternion.identity;   // No offset!
        currPatternIndex = 0;
        nextPatternResetTime = nextPatternReduceTime = nextFireAgainTime = nextPatternStartReducingTime = Time.time;

        // TODO move to factory.
        patternObj = new NoRecoilPattern();
        inaccuracyRecoveryFormula = InaccuracyRecoveryForumlas.GetLinearlyScaledRecoveryFunction(1);   // Recovery linearly increases as the offset increases.
        recoilRecoveryFormula = RecoilRecoveryFormulas.GetLinearlyScaledRecoveryFunction(1);
    }

    // Update is called once per frame
    void Update() {
        RecoverRecoilPattern();
        RecoverAdditionalInaccuracy();
        RecoverAimpointOffset();
    }

    private void RecoverAdditionalInaccuracy() {
        currAddititonalInaccuracy -= inaccuracyRecoveryFormula(currAddititonalInaccuracy, inaccuracyRecoveryRate, Time.deltaTime);
        if (currAddititonalInaccuracy < 0) currAddititonalInaccuracy = 0;
    }

    private void RecoverAimpointOffset() {
        // Reduce the offset rotation back towards the Identity Quaternion over time.
        currAimpointOffset = Quaternion.RotateTowards(currAimpointOffset, Quaternion.identity, recoilRecoveryFormula(currAimpointOffset, recoilRecoveryRate, Time.deltaTime));
    }

    private void RecoverRecoilPattern() {
        // 'recover' the recoil pattern, aimpoint offset, and additional inaccuracy over time!
        // Every time a shot is fired, these 'reset times' will be updated to the correct interval.
        if (currPatternIndex > 0) {
            // See if we need to simply hard reset the recoil pattern
            if (Time.time > nextPatternResetTime) {
                currPatternIndex = 0;
            }
            else if (Time.time > nextPatternStartReducingTime) {
                // Keep reducing the pattern count accordingly to how much time has passed, past the start reducing time.
                while (Time.time >= nextPatternReduceTime && currPatternIndex > 0) {
                    // DebuggingHelpers.Log("start reduce time: " + nextPatternStartReducingTime + ", nextPatternReduceTime: " + nextPatternReduceTime + ", Time: " + Time.time + ", Pattern Index: " + currPatternIndex);
                    currPatternIndex -= 1;

                    // Increment the 'next reduce time' by the wait time, in case we need to reduce the pattern more than once on this tick!
                    nextPatternReduceTime += recoilPatternRecoveryTime;
                }
            }
        }
    }

    public bool AimInDirection(Vector3 direction) {
        // Rotate the pivot point to point in the direction given
        pivotPoint.forward = direction;
        return true;
    }

    public bool AimInDirection(Vector3 direction, Func<Vector3, Vector3> interpolationFunction) {
        throw new NotImplementedException();
    }

    public bool AimTowards(Vector3 position) {
        pivotPoint.LookAt(position);
        return true;
    }

    public bool AimTowards(Vector3 position, Func<Vector3, Vector3> interpolationFunction) {
        throw new NotImplementedException();
    }

    public bool CanFireAgain() {
        return Time.time >= nextFireAgainTime;
    }

    public int Fire(float movementSpeed) {
        if (!CanFireAgain()) return -1;

        // Step 1: Calculate where we should shoot, based on the current innaccuracy, recoil offsets, and recoil pattern.

        // To gerenate some innaccuracy, we will not generate a random 'rotation', but instead start with a forward facing vector and
        // add to it some random x and y direction, scaled by the inaccuracy. This has the benefit of biasing the randomness sllightly towards
        // the centre of the 'circle', and is also mathematically simpler than applying tons of random rotations in Quaternion form.
        // Then, we will rotate the resulting vector be the muzzle's transform.rotation to make it oriented with transform.forward (with innaccuracy).
        // Then, we will rotate THAT vector by the recoil offset rotation to get the final firing direction!

        // Randomly apply some 'innaccuracy' to a 'forward' vector
        if (currAddititonalInaccuracy + baseInaccuracyDegrees > maxUnscaledInaccuracyDegrees) { currAddititonalInaccuracy = maxUnscaledInaccuracyDegrees - baseInaccuracyDegrees; } 
        float inaccDeg = (baseInaccuracyDegrees + currAddititonalInaccuracy) * (1 + (movementInaccuracyScaleFactor * movementSpeed));
        inaccDeg = (inaccDeg > maxScaledInaccuracyDegrees) ? maxScaledInaccuracyDegrees : inaccDeg;
        Vector2 randcomponent = UnityEngine.Random.insideUnitCircle * inaccDeg;  // Random point inside circle where radius rep's angle
        Vector3 innaccurateVector = Quaternion.Euler(randcomponent.x, randcomponent.y, 0f) * Vector3.forward;

        // Orient the innaccurate Vector with the muzzle direction ('true' aimpoint)
        Vector3 bulletTrajectory = muzzlePoint.rotation * innaccurateVector;

        // Apply the recoil offset rotation
        bulletTrajectory = currAimpointOffset * bulletTrajectory;

        // Step 2: Actually do the projectile calculations
        FireProjectile(bulletTrajectory);

        // Step 3: Update timers, update recoil offsets, update the additional inaccuracy, and update the current pattern index
        nextFireAgainTime = Time.time + fireRate_waitTime;
        nextPatternStartReducingTime = Time.time + recoilPatternContinuousFireWindow;
        nextPatternReduceTime = nextPatternStartReducingTime + recoilPatternRecoveryTime;
        nextPatternResetTime = Time.time + recoilPatternHardResetTime;
        currAimpointOffset = patternObj.GetAimpointOffsetRotation(currPatternIndex, recoilOffsetBaseScaleFactor * (1 + (movementRecoilMagnitudeScaleFactor * movementSpeed))) * currAimpointOffset;
        if (Quaternion.Angle(currAimpointOffset, Quaternion.identity) > maxRecoilOffsetAngle) {
            currAimpointOffset = Quaternion.RotateTowards(currAimpointOffset, Quaternion.identity, Quaternion.Angle(currAimpointOffset, Quaternion.identity) - maxRecoilOffsetAngle);
        }
        currAddititonalInaccuracy += recoilInaccuracyAddAmount;
        currPatternIndex++;

        //DONE!
        return currPatternIndex;
    }

    private void FireProjectile(Vector3 trajectory) {
        // DEBUG
        DebuggingHelpers.DrawRay(muzzlePoint.position, trajectory * 100, Color.yellow);
        DebuggingHelpers.Log("FIRING GUN: Recoil pattern is " + currPatternIndex);

        // Simply fire a ray out of the muzzle, in the trajectory direction.
        Ray bulletRay = new Ray(muzzlePoint.position, trajectory);
        RaycastHit hitInfo;
        if (Physics.Raycast(bulletRay, out hitInfo, 10000, hittableLayers, QueryTriggerInteraction.Ignore)) {
            Damageable_EventInvoker damageable = hitInfo.collider.GetComponent<Damageable_EventInvoker>();
            if (damageable != null) {
                damageable.OnHit(this, new ProjectileHitEventArgs(bulletDamage, 0f, trajectory, 1000f, hitInfo.point));
            }
        }
    }

    public int ShootAtPosition(Vector3 target, float movementSpeed) {
        AimTowards(target);
        return Fire(movementSpeed);
    }

    public int ShootInDirection(Vector3 direction, float movementSpeed) {
        AimInDirection(direction);
        return Fire(movementSpeed);
    }
}

public static class InaccuracyRecoveryForumlas {
    // FUNCTION FACTORY FUNCTION: Returns a function with the appropriate constant offset.
    public static Func<float, float, float, float> GetConstantRecoveryFunction() {
        return (float currentAdditionalInaccuracy, float recoveryRateValue, float deltaTime) => recoveryRateValue * deltaTime;
    }

    // FUNCTION FACTORY FUNCTION: Returns a function which scales recovery linearly compared to current offset, with the appropriate constant offset.
    public static Func<float, float, float, float> GetLinearlyScaledRecoveryFunction(float yIntercept, float maxRecoveryPerSecond = Mathf.Infinity, float minRecoveryPerSecond = 0f) {
        return (float currentAdditionalInaccuracy, float recoveryRateValue, float deltaTime) => {
            if (yIntercept < 0) { yIntercept = 0; }
            float recoveryPerSec = (yIntercept + currentAdditionalInaccuracy) * recoveryRateValue;
            recoveryPerSec = Mathf.Clamp(recoveryPerSec, minRecoveryPerSecond, maxRecoveryPerSecond);
            return recoveryPerSec * deltaTime;
        };
    }

    // FUNCTION FACTORY FUNCTION: Returns a function which scaled recovery proportionally squared, with the appropriate constant constant offset.
    public static Func<float, float, float, float> GetSquareScaledRecoveryFunction(float yIntercept, float maxRecoveryPerSecond = Mathf.Infinity, float minRecoveryPerSecond = 0f) {
        return (float currentAdditionalInaccuracy, float recoveryRateValue, float deltaTime) => {
            if (yIntercept < 0) { yIntercept = 0; }
            float recoveryPerSec =  (yIntercept + currentAdditionalInaccuracy * currentAdditionalInaccuracy) * recoveryRateValue;
            recoveryPerSec = Mathf.Clamp(recoveryPerSec, minRecoveryPerSecond, maxRecoveryPerSecond);
            return recoveryPerSec * deltaTime;
        };
    }
}

public static class RecoilRecoveryFormulas {
    public static Func<Quaternion, float, float, float> GetConstantRecoveryFunction() {
        return (Quaternion currentOffset, float recoveryRateValue, float deltaTime) => recoveryRateValue * deltaTime; 
    }

    public static Func<Quaternion, float, float, float> GetLinearlyScaledRecoveryFunction(float yIntercept, float maxRecoveryPerSecond = Mathf.Infinity, float minRecoveryPerSecond = 0f) {
        return (Quaternion currentOffset, float recoveryRateValue, float deltaTime) => {
            if (yIntercept < 0) { yIntercept = 0; }

            float angle = Quaternion.Angle(currentOffset, Quaternion.identity);
            float recoveryPerSec = (yIntercept + angle) * recoveryRateValue;
            recoveryPerSec = Mathf.Clamp(recoveryPerSec, minRecoveryPerSecond, maxRecoveryPerSecond);
            return recoveryPerSec * deltaTime;
        };
    }

    public static Func<Quaternion, float, float, float> GetSquaredScaledRecoveryFunction(float yIntercept, float maxRecoveryPerSecond = Mathf.Infinity, float minRecoveryPerSecond = 0f) {
        return (Quaternion currentOffset, float recoveryRateValue, float deltaTime) => {
            if (yIntercept < 0) { yIntercept = 0; }

            float angle = Quaternion.Angle(currentOffset, Quaternion.identity);
            float recoveryPerSec = (yIntercept + angle * angle) * recoveryRateValue;
            recoveryPerSec = Mathf.Clamp(recoveryPerSec, minRecoveryPerSecond, maxRecoveryPerSecond);
            return recoveryPerSec * deltaTime;
        };
    }
}

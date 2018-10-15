using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicRifleMechanics : MonoBehaviour, IGunMechanics {

    [Header("Reference points for aiming and firing")]
    public Transform pivotPoint;
    public Transform muzzlePoint;

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
    [Tooltip("The maximum inaccuracy for any shot fired")]
    public float maxInaccuracyDegrees;
    [Tooltip("Scaling for how much inaccuracy is added by the character's movement speed")]
    public float movementInaccuracyScaleFactor;
    [Tooltip("How much inaccuracy in degrees should be added per shot, due to recoil")]
    public float recoilInaccuracyAddAmount;
    [Tooltip("How quickly the current inaccuracy reduces back to base inaccuracy, in degrees per second")]
    public float inaccuracyRecoveryRate;


    [Header("Recoil and recoil pattern settings")]

    [Tooltip("Factor for scaling the magnitude of the recoil pattern")]
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
    [Tooltip("How long it takes to step back one 'shot' in the recoil pattern")]
    public float recoilPatternRecoveryTime;
    [Tooltip("How long it takes to 'hard reset' back to the start of the recoil pattern, no matter where you were in the pattern")]
    public float recoilPatternHardResetTime;

    // STATE VARIABLES FOR THE GUN ACCURACY AND RECOIL.
    private float currAddititonalInaccuracy;    // Represents the current amount of inaccuracy in degrees.
    private Quaternion currAimpointOffset;      // Represents the offset to the 'true' aimpoint direction vector as a rotation.
    private IRecoilPattern patternObj;
    private int currPatternIndex;               // Which 'shot' we are up to in the pattern.

    // Timers
    private float nextPatternReduceTime;
    private float nextPatternResetTime;
    private float nextFireAgainTime;

    // Use this for initialization
    void Start() {
        currAddititonalInaccuracy = 0f;
        currAimpointOffset = Quaternion.identity;   // No offset!
        currPatternIndex = 0;
        nextPatternResetTime = nextPatternReduceTime = nextFireAgainTime = Time.time;
    }

    // Update is called once per frame
    void Update() {
        RecoverRecoilPattern();
        RecoverAdditionalInaccuracy();
        RecoverAimpointOffset();
    }

    private void RecoverAdditionalInaccuracy() {
        currAddititonalInaccuracy -= inaccuracyRecoveryRate * Time.deltaTime;
    }

    private void RecoverAimpointOffset() {
        // Reduce the offset rotation back towards the Identity Quaternion over time.
        currAimpointOffset = Quaternion.RotateTowards(currAimpointOffset, Quaternion.identity, recoilRecoveryRate * Time.deltaTime);
    }

    private void RecoverRecoilPattern() {
        // 'recover' the recoil pattern, aimpoint offset, and additional inaccuracy over time!
        // Every time a shot is fired, these 'reset times' will be updated to the correct interval.
        if (currPatternIndex > 0) {
            // See if we need to simply hard reset the recoil pattern
            if (Time.time > nextPatternResetTime) {
                currPatternIndex = 0;
            }
            else {
                // Keep reducing the pattern count accordingly to how much time has passed.
                while (Time.time >= nextPatternReduceTime) {
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

    public bool Fire(float movementSpeed) {
        if (!CanFireAgain()) return false;

        // Step 1: Calculate where we should shoot, based on the current innaccuracy, recoil offsets, and recoil pattern.

        // To gerenate some innaccuracy, we will not generate a random 'rotation', but instead start with a forward facing vector and
        // add to it some random x and y direction, scaled by the inaccuracy. This has the benefit of biasing the randomness sllightly towards
        // the centre of the 'circle', and is also mathematically simpler than applying tons of random rotations in Quaternion form.
        // Then, we will rotate the resulting vector be the muzzle's transform.rotation to make it oriented with transform.forward (with innaccuracy).
        // Then, we will rotate THAT vector by the recoil offset rotation to get the final firing direction!

        // Randomly apply some 'innaccuracy' to a 'forward' vector
        float inaccDeg = (baseInaccuracyDegrees + currAddititonalInaccuracy) * movementInaccuracyScaleFactor * movementSpeed;
        inaccDeg = (inaccDeg > maxInaccuracyDegrees) ? maxInaccuracyDegrees : inaccDeg;
        Vector2 randcomponent = UnityEngine.Random.insideUnitCircle * inaccDeg;  // Random point inside circle where radius rep's angle
        Vector3 innaccurateVector = Quaternion.Euler(randcomponent.x, randcomponent.y, 0f) * Vector3.forward;

        // Orient the innaccurate Vector with the muzzle direction ('true' aimpoint)
        Vector3 bulletTrajectory = muzzlePoint.rotation * innaccurateVector;

        // Apply the recoil offset rotation
        bulletTrajectory = currAimpointOffset * bulletTrajectory;


        // Step 2: Actually do the projectile calculations
        FireProjectile(bulletTrajectory);

        // Step 3: Update timers, update recoil offsets, and update the additional inaccuracy.
        nextFireAgainTime = Time.time + fireRate_waitTime;
        nextPatternReduceTime = Time.time + recoilPatternRecoveryTime;
        nextPatternResetTime = Time.time + recoilPatternHardResetTime;

        currAimpointOffset = patternObj.GetAimpointOffsetRotation(currPatternIndex, recoilOffsetBaseScaleFactor * movementRecoilMagnitudeScaleFactor * movementSpeed) * currAimpointOffset;
        if (Quaternion.Angle(currAimpointOffset, Quaternion.identity) > maxRecoilOffsetAngle) {
            currAimpointOffset = Quaternion.RotateTowards(currAimpointOffset, Quaternion.identity, Quaternion.Angle(currAimpointOffset, Quaternion.identity) - maxRecoilOffsetAngle);
        }

        currAddititonalInaccuracy += recoilInaccuracyAddAmount;

        //DONE!
        return true;
    }

    public bool ShootAtPosition(Vector3 target, float movementSpeed) {
        AimTowards(target);
        return Fire(movementSpeed);
    }

    public bool ShootInDirection(Vector3 direction, float movementSpeed) {
        AimInDirection(direction);
        return Fire(movementSpeed);
    }
}

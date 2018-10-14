using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicRifleMechanics : MonoBehaviour, IGunMechanics {

    [Header("Reference points for aiming and firing")]
    public Transform pivotPoint;
    public Transform muzzlePoint;

    [Header("Accuracy and Recoil settings")]

    [Tooltip("The range in degrees that the random direction offset can be when firing")]
    public float baseInaccuracyDegrees;
    [Tooltip("The maximum inaccuracy for any shot fired")]
    public float maxInaccuracyDegrees;
    [Tooltip("The maximum distance the recoil-aimpoint-offset can be from the base aim point")]
    public float maxRecoilOffsetDistance;
    [Tooltip("Scaling for how much inaccuracy is added by the character's movement speed")]
    public float movementInaccuracyScaleFactor;
    [Tooltip("How much inaccuracy in degrees should be added per shot, due to recoil")]
    public float recoilInaccuracyAddAmount;
    [Tooltip("Scaling for how much the magnitude of the recoil is increased by the character's movement speed")]
    public float movementRecoilMagnitudeScaleFactor;
    [Tooltip("Scaling for how much the random direction of the recoil is increased by the character's movement speed")]
    public float movementRecoilDirectionScaleFactor;
    [Tooltip("How quickly the current inaccuracy reduces back to base inaccuracy")]
    public float inaccuracyRecoveryRate;
    [Tooltip("How quickly the recoil-aimpoint-offset reduces back to zero")]
    public float recoilRecoveryRate;

    
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }


    public bool AimInDirection(Vector3 direction) {
        throw new NotImplementedException();
    }

    public bool AimInDirection(Vector3 direction, Func<Vector3, Vector3> interpolationFunction) {
        throw new NotImplementedException();
    }

    public bool AimTowards(Vector3 position) {
        throw new NotImplementedException();
    }

    public bool AimTowards(Vector3 position, Func<Vector3, Vector3> interpolationFunction) {
        throw new NotImplementedException();
    }

    public bool CanFireAgain() {
        throw new NotImplementedException();
    }

    public bool Fire(float movementSpeed) {
        throw new NotImplementedException();
    }

    public bool ShootAtPosition(Vector3 target, float movementSpeed) {
        throw new NotImplementedException();
    }

    public bool ShootInDirection(Vector3 direction, float movementSpeed) {
        throw new NotImplementedException();
    }
}

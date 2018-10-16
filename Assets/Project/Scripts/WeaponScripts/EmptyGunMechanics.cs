using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyGunMechanics : MonoBehaviour, IGunMechanics {
    public bool AimAtTarget(Vector3 targetPosition, float angularInterpolationFactor) {
        return false;
    }

    public bool AimInDirection(Vector3 direction) {
        return false;
    }

    public bool AimInDirection(Vector3 direction, Func<Vector3, Vector3> interpolationFunction) {
        return false;
    }

    public bool AimTowards(Vector3 position) {
        return false;
    }

    public bool AimTowards(Vector3 position, Func<Vector3, Vector3> interpolationFunction) {
        return false;
    }

    public bool CanFireAgain() {
        return false;
    }

    public int Fire(float movementSpeed) {
        return -1;
    }

    public int ShootAtPosition(Vector3 target, float movementSpeed) {
        return -1;
    }

    public int ShootInDirection(Vector3 direction, float movementSpeed) {
        return -1;
    }
}

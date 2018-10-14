using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyGunMechanics : MonoBehaviour, IGunMechanics {
    bool IGunMechanics.AimInDirection(Vector3 direction) {
        return false;
    }

    bool IGunMechanics.AimInDirection(Vector3 direction, Func<Vector3, Vector3> interpolationFunction) {
        return false;
    }

    bool IGunMechanics.AimTowards(Vector3 position) {
        return false;
    }

    bool IGunMechanics.AimTowards(Vector3 position, Func<Vector3, Vector3> interpolationFunction) {
        return false;
    }

    bool IGunMechanics.CanFireAgain() {
        return true;
    }

    bool IGunMechanics.Fire(float movementSpeed) {
        return true;
    }

    bool IGunMechanics.ShootAtPosition(Vector3 target, float movementSpeed) {
        return true;
    }

    bool IGunMechanics.ShootInDirection(Vector3 direction, float movementSpeed) {
        return true;
    }
}

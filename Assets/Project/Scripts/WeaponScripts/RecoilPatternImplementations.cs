using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoRecoilPattern : IRecoilPattern {

    public Quaternion GetAimpointOffsetRotation(int shotInPattern) {
        return Quaternion.identity;
    }

    public Quaternion GetAimpointOffsetRotation(int shotInPattern, float scaleFactor) {
        return Quaternion.identity;
    }

    public Quaternion GetAimpointOffsetRotation(int shotInPattern, float scaleFactor, float randomness) {
        return Quaternion.identity;
    }
}

public class SimpleConstantVerticalRecoilPattern : IRecoilPattern {

    private static Quaternion offset = Quaternion.Euler(-3f, 0f, 0f);    // This spray pattern simply provides the same vertical recoil for every shot.

    public Quaternion GetAimpointOffsetRotation(int shotInPattern) {
        return GetAimpointOffsetRotation(shotInPattern, 1f);
    }

    public Quaternion GetAimpointOffsetRotation(int shotInPattern, float scaleFactor) {
        return Quaternion.SlerpUnclamped(Quaternion.identity, offset, scaleFactor);
    }

    public Quaternion GetAimpointOffsetRotation(int shotInPattern, float scaleFactor, float directionRandomnessRangeDegrees) {
        // Original
        Quaternion noRand = GetAimpointOffsetRotation(shotInPattern, scaleFactor);

        // Apply a random 'direction' rotation by rotating the rotation along the z world space axis.
        Quaternion oneDegree = Quaternion.Euler(0f, 0f, 1f);
        float amount = Random.Range(-directionRandomnessRangeDegrees, directionRandomnessRangeDegrees);

        // Return the original offset, rotated by the z direction rotation scaled by the amount.
        return Quaternion.SlerpUnclamped(Quaternion.identity, oneDegree, amount) * noRand;
    }
}

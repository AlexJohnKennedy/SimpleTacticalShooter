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
}

public class SimpleConstantVerticalRecoilPattern : IRecoilPattern {

    private static Quaternion offset = Quaternion.Euler(-3f, 0f, 0f);    // This spray pattern simply provides the same vertical recoil for every shot.

    public Quaternion GetAimpointOffsetRotation(int shotInPattern) {
        return GetAimpointOffsetRotation(shotInPattern, 1f);
    }

    public Quaternion GetAimpointOffsetRotation(int shotInPattern, float scaleFactor) {
        return Quaternion.SlerpUnclamped(Quaternion.identity, offset, scaleFactor);
    }
}

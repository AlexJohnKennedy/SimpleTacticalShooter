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

﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public interface ICharacterDetector {
    event EventHandler<List<Collider>> VisionUpdatedEvent;
}

public interface IGunMechanics {
    bool CanFireAgain();

    int Fire(float movementSpeed);     // Returns the 'pattern index' to let the caller know how many shots into a burst they are.

    bool AimInDirection(Vector3 direction);
    bool AimTowards(Vector3 position);
    bool AimInDirection(Vector3 direction, Func<Vector3, Vector3> interpolationFunction);   //Version which allows you to pass is a function which will handle interpolation of aim direction
    bool AimTowards(Vector3 position, Func<Vector3,Vector3> interpolationFunction);         //Version which allows you to pass in a function which will handle interpolation of aim position
    bool AimAtTarget(Vector3 targetPosition, float angularInterpolationFactor);

    //AIMING AND SHOOTING IN ONE CALL - Usually better to handle aiming and firing separately
    int ShootInDirection(Vector3 direction, float movementSpeed);   //Possibly should pass in a handler for hit info?
    int ShootAtPosition(Vector3 target, float movementSpeed);       //Possibly should pass in a handler for hit info?
}

public interface IRecoilPattern {
    Quaternion GetAimpointOffsetRotation(int shotInPattern);     // gets the offset rotation for the 'nth' shot in the recoil pattern.
    Quaternion GetAimpointOffsetRotation(int shotInPattern, float scaleFactor);     // gets the offset, but scales the magnitude of the recoil rotation.
    Quaternion GetAimpointOffsetRotation(int shotInPattern, float scaleFactor, float directionRandomnessRangeDegrees);
}

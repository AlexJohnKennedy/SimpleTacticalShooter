using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public interface ICharacterDetector {
    event EventHandler<List<TargetInformation>> VisionUpdatedEvent;
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
    Quaternion GetAimpointOffsetRotation(int shotInPattern);                        // gets the offset rotation for the 'nth' shot in the recoil pattern.
    Quaternion GetAimpointOffsetRotation(int shotInPattern, float scaleFactor);     // gets the offset, but scales the magnitude of the recoil rotation.
    Quaternion GetAimpointOffsetRotation(int shotInPattern, float scaleFactor, float directionRandomnessRangeDegrees);
}

public class TargetInformation {
    public bool IsCharacter { get; }

    public ICharacter character;
    public Collider collider;
    public Vector3 aimPosition;    // Position that is the estimated centre of the visible part of the target collider

    public TargetInformation(Collider collider, Vector3 centralVisiblePoint) {
        this.collider = collider;
        this.aimPosition = centralVisiblePoint;

        // We can automatically determine the 'character' object by getting it.
        character = collider.GetComponent<ICharacter>();
        if (character == null) {
            IsCharacter = false;
        }
        else {
            IsCharacter = true;
        }
    }
}

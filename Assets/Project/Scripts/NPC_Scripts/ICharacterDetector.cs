using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public interface ICharacterDetector {
    event EventHandler<List<Collider>> VisionUpdatedEvent;
}

public interface IGunMechanics {
    bool CanFireAgain();

    bool Fire(float movementSpeed);     //Possibly should pass in a handler for hit info?

    bool AimInDirection(Vector3 direction);
    bool AimTowards(Vector3 position);
    bool AimInDirection(Vector3 direction, Func<Vector3, Vector3> interpolationFunction);   //Version which allows you to pass is a function which will handle interpolation of aim direction
    bool AimTowards(Vector3 position, Func<Vector3,Vector3> interpolationFunction);         //Version which allows you to pass in a function which will handle interpolation of aim position

    //AIMING AND SHOOTING IN ONE CALL - Usually better to handle aiming and firing separately
    bool ShootInDirection(Vector3 direction, float movementSpeed);   //Possibly should pass in a handler for hit info?
    bool ShootAtPosition(Vector3 target, float movementSpeed);       //Possibly should pass in a handler for hit info?
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Camera))]
public class WASDCamera : MonoBehaviour {

    public float accelAmount;
    public float friction;
    public float zoomSpeed;
    public float minFov;
    public float maxFov;

    private Vector3 currVelocity;
    private Vector3 direction;
    private float scrollDelta;
    private CharacterController characterController;
    private Camera thisCamera;

	// Use this for initialization
	void Start () {
        characterController = GetComponent<CharacterController>();
        thisCamera = GetComponent<Camera>();
        currVelocity = Vector3.zero;
	}

    void LateUpdate () {
        direction = new Vector3(0f, 0f, 0f);
        Vector3 forward = this.gameObject.transform.forward;
        forward.y = 0;
        forward.Normalize();
        Vector3 right = this.gameObject.transform.right;
        right.y = 0;
        right.Normalize();

        if (Input.GetKey(KeyCode.W)) {
            direction += forward;
        }
        if (Input.GetKey(KeyCode.S)) {
            direction -= forward;
        }
        if (Input.GetKey(KeyCode.D)) {
            direction += right;
        }
        if (Input.GetKey(KeyCode.A)) {
            direction -= right;
        }
        direction.Normalize();

        if (Input.GetKey(KeyCode.Q)) {
            direction += Vector3.up;
        }
        if (Input.GetKey(KeyCode.E)) {
            direction -= Vector3.up;
        }

        //ZOOM.
        scrollDelta = Input.GetAxis("Mouse ScrollWheel");

        Vector3 accelerationVector;     // Will store the resulting acceleration based on the movement command
        Vector3 velocityVector;         // Will store the resulting velocity for this frame based on the acceleration vector, time, the previous velocity, and jump actions

        accelerationVector = direction * accelAmount;
        accelerationVector -= currVelocity.normalized * this.friction * currVelocity.magnitude;  //Apply speed-dependent friction factor
                                                                                                     //Apply static friction factor
        // Calculate new velocity.
        velocityVector = currVelocity + accelerationVector * Time.deltaTime;

        //Okay, now we can apply a transform on the character controller using the 'Move' method, based on the absolute velocity we just calcuated
        this.currVelocity = velocityVector;
        characterController.Move(velocityVector * Time.deltaTime);

        thisCamera.fieldOfView -= zoomSpeed * scrollDelta;
        if (thisCamera.fieldOfView < minFov) thisCamera.fieldOfView = minFov;
        else if (thisCamera.fieldOfView > maxFov) thisCamera.fieldOfView = maxFov;
    }
}

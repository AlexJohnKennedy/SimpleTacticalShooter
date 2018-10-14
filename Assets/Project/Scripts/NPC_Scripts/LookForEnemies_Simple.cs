using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class LookForEnemies_Simple : MonoBehaviour, ICharacterDetector {

    // Public fields
    public LayerMask characterLayerMask;    //Used to only look for colliders which are CHARACTERS (i.e. on the character layer)
    public LayerMask sightLayerMask;
    public float maxVisionDistance;
    public float checkFrequencySeconds;
    public Transform eyesPosition;
    public int numHorizontalChecks;
    public int numVerticalChecks;

    [HideInInspector]
    public event EventHandler<List<Collider>> VisionUpdatedEvent;   // Interested parteis can receive updates when we do vision updates.

    private List<Collider> selfColliders;
    private float nextCheckTime;

	// Use this for initialization
	void Start () {
        // Make sure we don't detect ourselves, by keeping track of which colliders are our own!
        selfColliders = new List<Collider>();
        Collider c = this.gameObject.GetComponent<Collider>();
        if (c != null) selfColliders.Add(this.gameObject.GetComponent<Collider>());
        nextCheckTime = Time.time + checkFrequencySeconds;

        if (numHorizontalChecks <= 1) { numHorizontalChecks = 2; }
        if (numVerticalChecks <= 1) { numVerticalChecks = 2; }
	}
	
	// Update is called once per frame
	void Update () {
		// If we need to do another check, then do the check!
        if (Time.time >= nextCheckTime) {
            nextCheckTime = Time.time + checkFrequencySeconds;

            List<Collider> visibleCharacterColliders = new List<Collider>();
            // Find all colliders in the character layer that are within the search radius of us!
            foreach (Collider potentialTarget in Physics.OverlapSphere(transform.position, maxVisionDistance, characterLayerMask, QueryTriggerInteraction.Ignore)) {
                if (!selfColliders.Contains(potentialTarget) && CanSee(potentialTarget)) {
                    visibleCharacterColliders.Add(potentialTarget);
                }
            }

            // Notify interested parties of the update!
            VisionUpdatedEvent?.Invoke(this, visibleCharacterColliders);
        }
	}

    private bool CanSee(Collider target) {
        if (LineCastCheck(target, target.transform.position)) { return true; }

        // Perform a 'numHorizontalChecks' by 'numVerticalChecks' number of line casts to try to see the character in the case he is partially obscured
        // First, figure out the maximum 'width' of where the target could be, as the widest range to be the hypotenuse of the Axis-aligned-bounding-box of the target collider.
        Bounds b = target.bounds;
        float width = Mathf.Sqrt(b.size.x * b.size.x + b.size.z * b.size.z);
        float height = b.size.y;
        float horSpacing = (width * 0.95f)  / (numHorizontalChecks - 1);
        float verSpacing = (height * 0.95f) / (numVerticalChecks - 1);

        // Find a normalised vector perpendicular to the line between us and the target, which is also perpendiculr to vertical (parallel to the ground).
        Vector3 horizontalOffsetDirection = Vector3.Cross(target.transform.position - eyesPosition.position, Vector3.up).normalized;

        // Find a normalised vector perpendicular to the line between us and the target, which is also perpendicular to the ground (parallel to vertical).
        Vector3 verticalOffsetDirection = Vector3.Cross(target.transform.position - eyesPosition.position, Vector3.right).normalized;

        // Start at the 'top right' of the checking grid.
        Vector3 pointToCheck = target.transform.position - (horizontalOffsetDirection * (width * 0.95f / 2)) - (verticalOffsetDirection * (height * 0.95f / 2));

        if (LineCastCheck(target, pointToCheck)) { return true; }

        for (int i=0; i < numVerticalChecks; i++) {
            for (int j=0; j < numHorizontalChecks; j++) {
                // Check this spot!
                if (LineCastCheck(target, pointToCheck)) { return true; }

                // Move horizontally
                pointToCheck += horizontalOffsetDirection * horSpacing;
            }

            // Move vertically, and flip the direction of the horizontal offset direction vector, so it scans back the opposite direction!
            pointToCheck += verticalOffsetDirection * verSpacing;
            horizontalOffsetDirection = -horizontalOffsetDirection;
        }

        return false;
    }

    private bool LineCastCheck(Collider target, Vector3 point) {
        RaycastHit hitInfo;

        if (Physics.Linecast(eyesPosition.position, point, out hitInfo, sightLayerMask, QueryTriggerInteraction.Ignore)) {
            if (hitInfo.transform == target.transform) {
                // We can see the target!
                return true;
            }
        }
        return false;
    }
}

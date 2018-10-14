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
        RaycastHit hitInfo;
        
        if (Physics.Linecast(eyesPosition.position, target.transform.position, out hitInfo, sightLayerMask, QueryTriggerInteraction.Ignore)) {
            if (hitInfo.transform == target.transform) {
                // We can see the target!
                return true;
            }
        }
        // Perform a 'numHorizontalChecks' by 'numVerticalChecks' number of line casts to try to see the character in the case he is partially obscured
        

        return false;
    }
}

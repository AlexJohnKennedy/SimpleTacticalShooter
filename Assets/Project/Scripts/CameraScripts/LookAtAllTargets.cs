using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtAllTargets : MonoBehaviour {

    public List<Transform> targets;

	// Use this for initialization
	void Start () {
        //targets = new List<Transform>(); //DO NOT DO THIS COZ UNITY AUTO MAKES THE LIST! Doing this replaces the editor-constructed one :s
	}
	
	// Update is called once per frame
	void LateUpdate () {
        if (targets == null || targets.Count == 0) return;
        this.transform.LookAt(GetCenterPoint());
	}

    private Vector3 GetCenterPoint() {
        if (targets.Count == 1) {
            return targets[0].position;
        }
        else {
            Bounds bounds = new Bounds();
            foreach (Transform t in targets) {
                bounds.Encapsulate(t.position);
            }
            return bounds.center;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Camera))]
public class UnitCommander : MonoBehaviour {
    private NavMeshAgent currentlySelectedAgent;

	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButtonDown(0)) {
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo)) {
                NavMeshAgent agent = hitInfo.collider.gameObject.GetComponent<NavMeshAgent>();
                if (agent != null) currentlySelectedAgent = agent;
            }
        }
		if (currentlySelectedAgent != null && Input.GetMouseButtonDown(1)) {
            //Cast a ray from the camera to determine what was clicked on
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo) && hitInfo.collider.gameObject.GetComponent<NavMeshAgent>() == null) {
                currentlySelectedAgent.SetDestination(hitInfo.point);
                currentlySelectedAgent.isStopped = false;   // Overwrite a stopped unit (to be able to move the unit again)
            }
        }
	}
}

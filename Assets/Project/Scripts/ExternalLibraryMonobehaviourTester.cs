using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExternalLibraryTEST1_NETv4_6_1;  // IMPORT TEST LIBRARIES, WHICH ARE IN THE PROJECT DIRECTORY AS DLL FILES. 

public class ExternalLibraryMonobehaviourTester : MonoBehaviour {

	// Use this for initialization
	void Start () {
        // Simply try to print the string obtained by the external library
        Debug.Log(TestLibraryFunctions.GetTestString());
	}
}

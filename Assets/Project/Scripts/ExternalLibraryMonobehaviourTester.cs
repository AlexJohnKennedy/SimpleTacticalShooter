using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExternalLibraryTEST1_NETv4_7_1;  // IMPORT TEST LIBRARIES, WHICH ARE IN THE PROJECT DIRECTORY AS DLL FILES
using ClassLibTest_NET_STANDARD_2_0;

public class ExternalLibraryMonobehaviourTester : MonoBehaviour {

	// Use this for initialization
	void Start () {
        // Simply try to print the string obtained by the external library
        Debug.Log(UnityExternalLibraryTEST1_NETv4_7_1.TestLibraryFunctions.GetTestString());
        Debug.Log(UnityExternalLibraryTEST1_NETv4_7_1.TestLibraryFunctions.GetTestStringUsingCS7TupleFeature());
        Debug.Log(UnityExternalLibraryTEST1_NETv4_7_1.NetStandard2_WrapperTestLibraryFunctions.PrintDoubleNum(5));
        Debug.Log(ClassLibTest_NET_STANDARD_2_0.ExternalLibrary_NET_STANDARD_2_0_Test.GetTestStringUsingCS7TupleFeature());
    }
}

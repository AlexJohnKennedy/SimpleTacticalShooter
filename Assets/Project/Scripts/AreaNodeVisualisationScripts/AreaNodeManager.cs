using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** This class is responsible for managing the highlighting of area visualisation nodes in a central location by tracking all characters in the scene,
 *  and tracking all 'area nodes' in the scene, and mapping which characters are currently within each area. This will also track which character is 
 *  considered the 'main' character, and will work out how to highlight each areaNodeVisualisation according to the perspective and knowledge of the
 *  main character. (This class will  therefore listen to enemy spotted/engaged events from the main character). This controller class will change the
 *  highlights of the area node visualisations which it governs as it sees fit by invoking the setter methods on the visualiser class, and will expect
 *  the areaNodeVisualisations to register themselves to THIS object when they initiate (OnAwake). */
public class AreaNodeManager : MonoBehaviour {

    [Tooltip("Determines from whose perspective the visualisations should be from")]
    public SimpleAI_ShootAtVisibleTargets mainCharacter;    // Which perspective should we visualise from?


    private List<AreaNodeVisualisation> areaNodeVisualisations;                                         // Keeps a reference to all areas which register to this manager.

    public void RegisterAreaNode(AreaNodeVisualisation a) {
        if (areaNodeVisualisations == null) { areaNodeVisualisations = new List<AreaNodeVisualisation>(); }
        areaNodeVisualisations.Add(a);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

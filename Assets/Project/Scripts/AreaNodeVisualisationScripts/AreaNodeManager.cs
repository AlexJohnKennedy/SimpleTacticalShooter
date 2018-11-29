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
    public ICharacter mainCharacter;    // Which perspective should we visualise from?

    // In order to highlight things effectively, this will keep a mapping of which area every character is in, and what state that character has with respect to the main character.
    private Dictionary<ICharacter, AreaNodeVisualisation> characterAreaMap;
    private Dictionary<ICharacter, AreaNodeVisualisationStates> characterStateMap;
    private List<AreaNodeVisualisation> areaNodeVisualisations;

    // Used by AreaNodeVisualisation objects at initialisation time to register themselves as being-managed by this manager object.
    public void RegisterAreaNode(AreaNodeVisualisation a) {
        if (areaNodeVisualisations == null) { areaNodeVisualisations = new List<AreaNodeVisualisation>(); }
        areaNodeVisualisations.Add(a);
        a.CharacterEnteredZone += HandleCharacterEnterAreaEvent;
        a.CharacterExitedZone += HandleCharacterExitAreaEvent;
    }

    // Handler functions for when characters move from zone to zone.
    private void HandleCharacterEnterAreaEvent(object sender, ICharacter character) {
        AreaNodeVisualisation area = (AreaNodeVisualisation)sender;

        // Whatever the state of the area the Character was PREVIOUSLY in should become the state of the new area, unless the entered area already has a state which supersedes it.
        AreaNodeVisualisationStates state = characterStateMap[character];
        if ((int)state >= (int)area.CurrentState) {
            area.CurrentState = state;
        }
 
        // ok! we better make sure we keep everything up to date.
        characterAreaMap[character] = area;
    }
    private void HandleCharacterExitAreaEvent(object sender, ICharacter character) {
        AreaNodeVisualisation area = (AreaNodeVisualisation)sender;

        if (characterAreaMap[character] == area) {
            characterAreaMap[character] = null;
        }

        // The state of the area the character just left (the sender) should become the highest ranking state of the ones associated to any characters still remaining in the area, else default.
        AreaNodeVisualisationStates state = AreaNodeVisualisationStates.UNCONTROLLED;
        foreach (ICharacter c in area.AgentsInZone) {
            if ((int)characterStateMap[c] >= (int)state) {
                state = characterStateMap[c];
            }
        }

        area.CurrentState = state;
    }

    private void Awake() {
        characterAreaMap = new Dictionary<ICharacter, AreaNodeVisualisation>();
        characterStateMap = new Dictionary<ICharacter, AreaNodeVisualisationStates>();
        // Attain all objects with the character tag in roder to keep track of them.
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Character");
        for (int i=0; i < objs.Length; i++) {
            ICharacter ch = objs[i].GetComponent<ICharacter>();
            if (ch == null) {
                throw new System.Exception("A GameObject with the tag 'Character' did not have an ICharacter component attached. This is NOT ALLOWED! :O");
            }
            else {
                characterAreaMap.Add(ch, null);    // For now we don't know the starting Area.
                characterStateMap.Add(ch, AreaNodeVisualisationStates.NULL);

                // If this character is the main character, we need to make sure to listen to it's perception events!!
                if (ch == mainCharacter) {
                    RegisterListenersToMainCharacter(ch);
                    characterStateMap[ch] = AreaNodeVisualisationStates.MAIN_AGENT_IN_AREA;
                }
            }
        }
    }
    private void RegisterListenersToMainCharacter(ICharacter ch) {
        IPerceptionEventInvoker p = ch.RequestPerceptionEventListeningRights();

        // Register our handler delegates.
        p.EnemyEngagedEvent += 
        p.EnemyDisengagedEvent +=
        p.EnemySpottedEvent +=
        p.EnemyLostEvent += 
    }

    // Enumerated possible visualisation states, and their associated priority level. If there is more than one enemy/event in an area, which imply different states, the state with the higher
    // priority will supersede, and be the one displayed.
    public enum AreaNodeVisualisationStates {
        MAIN_AGENT_IN_AREA = 4,
        CONFIRMED_ENEMIES = 2,
        COMBAT_CONTACT= 3,
        ENEMY_CONTROLLED = 1,
        UNCONTROLLED = 1,
        DANGER = 1,
        CONTROLLED = 1,
        NULL = -1
    }
}

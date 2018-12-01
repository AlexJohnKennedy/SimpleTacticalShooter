using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HelperFunctions;

/** This class is responsible for managing the highlighting of area visualisation nodes in a central location by tracking all characters in the scene,
 *  and tracking all 'area nodes' in the scene, and mapping which characters are currently within each area. This will also track which character is 
 *  considered the 'main' character, and will work out how to highlight each areaNodeVisualisation according to the perspective and knowledge of the
 *  main character. (This class will  therefore listen to enemy spotted/engaged events from the main character). This controller class will change the
 *  highlights of the area node visualisations which it governs as it sees fit by invoking the setter methods on the visualiser class, and will expect
 *  the areaNodeVisualisations to register themselves to THIS object when they initiate (OnAwake). */
public class AreaNodeManager : MonoBehaviour {

    [Header("This object must be an ICharacter, as in have a component attached which implements ICharacter")]
    [Tooltip("Determines from whose perspective the visualisations should be from. THIS GAMEOBJECT MUST BE AN ICHARACTER!")]
    public GameObject mainCharacterGameObject;    // Which perspective should we visualise from?
    private ICharacter mainCharacter;

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
        a.AreaRequestsUpdate += (area) => UpdateAreaStateBasedOnCharactersWithinIt(area);
    }

    // Handler functions for when characters move from zone to zone.
    private void HandleCharacterEnterAreaEvent(object sender, ICharacter character) {
        // DebuggingHelpers.PrintCurrentMethodName();
        AreaNodeVisualisation area = (AreaNodeVisualisation)sender;

        // Whatever the state of the area the Character was PREVIOUSLY in should become the state of the new area, unless the entered area already has a state which supersedes it.
        AreaNodeVisualisationStates state = characterStateMap[character];
        if (state.Priority() >= area.CurrentState.Priority()) {
            area.CurrentState = state;
        }
 
        // ok! we better make sure we keep everything up to date.
        characterAreaMap[character] = area;
    }
    private void HandleCharacterExitAreaEvent(object sender, ICharacter character) {
        // DebuggingHelpers.PrintCurrentMethodName();
        AreaNodeVisualisation area = (AreaNodeVisualisation)sender;

        if (characterAreaMap[character] == area) {
            characterAreaMap[character] = null;
        }

        // The state of the area the character just left (the sender) should become the highest ranking state of the ones associated to any characters still remaining in the area, else default.
        UpdateAreaStateBasedOnCharactersWithinIt(area);
    }

    private void Awake() {
        mainCharacter = mainCharacterGameObject.GetComponent<ICharacter>();
        characterAreaMap = new Dictionary<ICharacter, AreaNodeVisualisation>();
        characterStateMap = new Dictionary<ICharacter, AreaNodeVisualisationStates>();
    }
    private void Start() {
        // Attain all objects with the character tag in roder to keep track of them.
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Character");
        for (int i = 0; i < objs.Length; i++) {
            ICharacter ch = objs[i].GetComponent<ICharacter>();
            if (ch == null) {
                throw new System.Exception("A GameObject with the tag 'Character' did not have an ICharacter component attached. This is NOT ALLOWED! :O");
            }
            else {
                characterAreaMap.Add(ch, null);    // For now we don't know the starting Area.
                characterStateMap.Add(ch, AreaNodeVisualisationStates.NULL);
                ch.CharacterKilledEvent += HandleCharacterDeath;

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
        p.EnemyEngagedEvent += MainCharacterEngagedEnemy;
        p.EnemyDisengagedEvent += MainCharacterDisengagedEnemy;
        p.EnemySpottedEvent += MainCharacterSpottedEnemy;
        p.EnemyLostEvent += MainCharacterLostEnemy;
    }

    // Handler function for when any character is killed. We will simply remove them from all of our tracking systems.
    private void HandleCharacterDeath(ICharacter c) {
        characterAreaMap.Remove(c);
        characterStateMap.Remove(c);

        // If the character which died was the main character, then we should just remove everything, and set everything back to default.
        foreach (AreaNodeVisualisation a in areaNodeVisualisations) {
            a.CurrentState = AreaNodeVisualisationStates.UNCONTROLLED;
            Destroy(a);
        }
        Destroy(this);
    }

    // Handler functions for when our main character percieves things about other characters.
    private void MainCharacterEngagedEnemy(object sender, ICharacter enemy) {
        // Check if this character has even registerd or has not been considered 'destroyed'
        if (!characterStateMap.ContainsKey(enemy) || !characterAreaMap.ContainsKey(enemy)) { return; }

        // DebuggingHelpers.PrintCurrentMethodName();
        characterStateMap[enemy] = AreaNodeVisualisationStates.COMBAT_CONTACT;
        UpdateAreaStateBasedOnCharactersWithinIt(characterAreaMap[enemy]);
    }
    private void MainCharacterDisengagedEnemy(object sender, ICharacter enemy) {
        // Check if this character has even registerd or has not been considered 'destroyed'
        if (!characterStateMap.ContainsKey(enemy) || !characterAreaMap.ContainsKey(enemy)) { return; }

        // DebuggingHelpers.PrintCurrentMethodName();
        if (characterStateMap[enemy] == AreaNodeVisualisationStates.COMBAT_CONTACT) {
            characterStateMap[enemy] = AreaNodeVisualisationStates.CONFIRMED_ENEMIES;
            UpdateAreaStateBasedOnCharactersWithinIt(characterAreaMap[enemy]);
        }
    }
    private void MainCharacterSpottedEnemy(object sender, ICharacter enemy) {
        // Check if this character has even registerd or has not been considered 'destroyed'
        if (!characterStateMap.ContainsKey(enemy) || !characterAreaMap.ContainsKey(enemy)) { return; }

        // DebuggingHelpers.PrintCurrentMethodName();
        if (AreaNodeVisualisationStates.CONFIRMED_ENEMIES.Priority() > characterStateMap[enemy].Priority()) {
            characterStateMap[enemy] = AreaNodeVisualisationStates.CONFIRMED_ENEMIES;
            UpdateAreaStateBasedOnCharactersWithinIt(characterAreaMap[enemy]);
        }
    }
    private void MainCharacterLostEnemy(object sender, ICharacter enemy) {
        // Check if this character has even registerd or has not been considered 'destroyed'
        if (!characterStateMap.ContainsKey(enemy) || !characterAreaMap.ContainsKey(enemy)) { return; }

        // DebuggingHelpers.PrintCurrentMethodName();
        characterStateMap[enemy] = AreaNodeVisualisationStates.UNCONTROLLED;
        UpdateAreaStateBasedOnCharactersWithinIt(characterAreaMap[enemy]);
    }

    private void UpdateAreaStateBasedOnCharactersWithinIt(AreaNodeVisualisation area) {
        if (area == null) { return; }
        AreaNodeVisualisationStates state = AreaNodeVisualisationStates.UNCONTROLLED;
        foreach (ICharacter c in area.AgentsInZone) {
            if (characterStateMap[c].Priority() >= state.Priority()) {
                state = characterStateMap[c];
            }
        }

        area.CurrentState = state;
    }
}


// Enumerated possible visualisation states. If there is more than one enemy/event in an area, which imply different states, the state with the higher
// priority will supersede, and be the one displayed. The priority level is defined in the constant array below, and an extension method to the enum
// is used to aquire the priority level.
public enum AreaNodeVisualisationStates {
    MAIN_AGENT_IN_AREA = 0,
    CONFIRMED_ENEMIES = 1,
    COMBAT_CONTACT = 2,
    ENEMY_CONTROLLED = 3,
    UNCONTROLLED = 4,
    DANGER = 5,
    CONTROLLED = 6,
    NULL = 7
}
/* Priority Levels (match up the enum value as index into the array below)
MAIN_AGENT_IN_AREA = 4,
CONFIRMED_ENEMIES = 2,
COMBAT_CONTACT= 3,
ENEMY_CONTROLLED = 1,
UNCONTROLLED = 1,
DANGER = 1,
CONTROLLED = 1,
NULL = -1
*/
// Attach an extension method to this enum type, so that we can recieve the priority levels of each state easily.
public static class VisualisationStateExtensionMehtods {
    private static readonly int[] nodeStatePriorityLevels = { 4, 2, 3, 1, 1, 1, 1, -1 };
    public static int Priority(this AreaNodeVisualisationStates s) {
        return nodeStatePriorityLevels[(int)s];
    }
}
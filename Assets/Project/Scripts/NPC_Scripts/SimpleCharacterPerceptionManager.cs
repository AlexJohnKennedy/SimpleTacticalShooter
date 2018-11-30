using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HelperFunctions;

/** This class handles keeping track of what the character is aware of (for now, just in terms of other 'enemy characters'), and invoking the
 *  appropriate events in accordance to the changes in the awareness state. All it has to do is maintain a collection of the characters it is
 *  currently aware of, and, each time it recieves a detection event or a combat decision event, update it's internal collection and determine
 *  if changes are requried. */

[RequireComponent(typeof(ICharacterDetector))]
[RequireComponent(typeof(ICombatAi))]
[DisallowMultipleComponent]
public class SimpleCharacterPerceptionManager : MonoBehaviour, IPerceptionEventInvoker, ICharacterAwarenessState {

    // Events it can invoke in different situations in order to implement the Perception Event Invoker interface.
    public event EventHandler<ICharacter> EnemySpottedEvent;
    public event EventHandler<ICharacter> EnemyLostEvent;
    public event EventHandler<ICharacter> PossibleThreatDetectedEvent;
    public event EventHandler<ICharacter> EnemyEngagedEvent;
    public event EventHandler<ICharacter> EnemyDisengagedEvent;

    // Internal enumeration of possible states for the awareness of an enemy character
    private enum AwarenessStates {
        EngagedWith, AwareOf, Visible
    }

    private Dictionary<ICharacter, AwarenessStates> enemiesAwareOf;     // Any Character object appearing in this dictionary is what this Character is 'aware' of in some way.

    // Use this for initialization
    void Start () {
        // Register to recieve updates from the character detector, so we can manage the perceptions.
        GetComponent<ICharacterDetector>().VisionUpdatedEvent += HandleVisionCheckUpdate;

        // Register to recieve updates from the Combat ai so we know when we decide to engage targets.
        GetComponent<ICombatAi>().EnemyEngagedEvent += HandleEnemyEngagedEvent;

        enemiesAwareOf = new Dictionary<ICharacter, AwarenessStates>();     // We will assume that we begin without an awareness of anything.
	}

    // This function is the Event handler we register to receive updates from the CharacterDetector.
    // TODO: Currently stupidly inefficient. Make it not retarded alex.
    private void HandleVisionCheckUpdate(object sender, List<TargetInformation> visibleTargets) {
        DebuggingHelpers.Log("Checking..." + visibleTargets.Count + " things to check");
        Dictionary<ICharacter, AwarenessStates> newState = new Dictionary<ICharacter, AwarenessStates>();
        
        // Simply cycle the list and add/remove anything accordingly.
        foreach (TargetInformation t in visibleTargets) {
            DebuggingHelpers.Log("Checking a target");
            if (t.IsCharacter) {
                if (!enemiesAwareOf.ContainsKey(t.character)) {
                    // Look! a newly spotted enemy!
                    EnemySpottedEvent?.Invoke(this, t.character);
                    newState.Add(t.character, AwarenessStates.Visible);
                }
                else {
                    newState.Add(t.character, enemiesAwareOf[t.character]);
                    enemiesAwareOf.Remove(t.character);     // Remove, so that we can see what leftovers there are at the end and can signal that we 'lost' them.
                }
            }
        }
        // Any character leftover in the old state is one which we have just lost sight of.
        foreach (KeyValuePair<ICharacter, AwarenessStates> c in enemiesAwareOf) {
            if (c.Value == AwarenessStates.EngagedWith) {
                EnemyDisengagedEvent?.Invoke(this, c.Key);
            }
            EnemyLostEvent?.Invoke(this, c.Key);
        }

        enemiesAwareOf = newState;
    }

    // This function is the Event handler we register to receive updates from the Combat Ai. It is called when the combat ai decides to engage a particular enemy.
    private void HandleEnemyEngagedEvent(object sender, TargetInformation enemy) {
        if (!enemy.IsCharacter) return;

        if (!enemiesAwareOf.ContainsKey(enemy.character)) {
            enemiesAwareOf.Add(enemy.character, AwarenessStates.EngagedWith);
            EnemyEngagedEvent?.Invoke(this, enemy.character);
        }
        else if (enemiesAwareOf[enemy.character] != AwarenessStates.EngagedWith) {
            EnemyEngagedEvent?.Invoke(this, enemy.character);
        }
        else {
            // Do nothing. Our internal state seems to think we are already engaging this target. Why are we recieving this event.. ? (Check for timing bugs?)
        }
    }
}

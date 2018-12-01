using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Simple implementation for a root class which represents a character in the world with a persistent identity.

[RequireComponent(typeof(ICombatAi))]
[RequireComponent(typeof(IHealthSystem))]
[RequireComponent(typeof(IPerceptionEventInvoker))]
[RequireComponent(typeof(ICharacterAwarenessState))]
[DisallowMultipleComponent]
public class SimpleCharacter : MonoBehaviour, ICharacter {

    public event Action<ICharacter> CharacterKilledEvent;

    // This boy is just a big bag of generic components so that those components can collectively represent one 'character', being
    // attached in a central location.
    private ICombatAi combatAi;
    private IHealthSystem healthSystem;
    private IPerceptionEventInvoker perceptionEventInvoker;
    private ICharacterAwarenessState characterAwarenessState;
    // Alot of the time it will be sensible to have the same object/component implementing both the characters perception management (awareness state)
    // and the perception event invoker, because the thing managing what the character is aware of is the best position to know when to invoke certain
    // events relating to those very perceptions. However, those things are still logically distinct in regard to the USERS of those operations, namely,
    // some clients will care about controlling the parameters of the awareness logic, while others will only care about recieving updates on what the 
    // character is currently awareof/percieving. Consequently, these are two distinct interfaces!

    // Use unity engine to aquire all the dependencies needed for a character to operate. They are mandated by this script to be attached to the gameobject.
    void Awake() {
        object[] temp = GetComponents<ICombatAi>();
        if (temp.Length > 1) { throw new System.Exception("Character gameobject had more than one ICombatAi component attached! This is not allowed"); }
        combatAi = (ICombatAi)temp[0];

        temp = GetComponents<IHealthSystem>();
        if (temp.Length > 1) { throw new System.Exception("Character gameobject had more than one IHealthSystem component attached! This is not allowed"); }
        healthSystem = (IHealthSystem)temp[0];

        temp = GetComponents<IPerceptionEventInvoker>();
        if (temp.Length > 1) { throw new System.Exception("Character gameobject had more than one IPerceptionEventInvoker component attached! This is not allowed"); }
        perceptionEventInvoker = (IPerceptionEventInvoker)temp[0];

        temp = GetComponents<ICharacterAwarenessState>();
        if (temp.Length > 1) { throw new System.Exception("Character gameobject had more than one ICharacterAwarenessState component attached! This is not allowed"); }
        characterAwarenessState= (ICharacterAwarenessState)temp[0];
    }

    public IPerceptionEventInvoker RequestPerceptionEventListeningRights() {
        return perceptionEventInvoker;
    }

    // Simply way to detect if this character has died is if this gameobject is being destroyed or disabled. Note 'OnDisable' is called in both of these circumstances.
    private void OnDisable() {
        CharacterKilledEvent?.Invoke(this);
    }
}

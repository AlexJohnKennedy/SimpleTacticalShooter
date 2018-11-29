using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// Serves as generic type for instances which represent the PERSISTENT ENTITY REPRESENTING A CHARACTER/AGENT in the game world.
// An implementation of this interface will be the 'root' object of the collection of objects containing character logic, and should
// generally just be a 'bag of decomposed interfaces' which are delegated all the more specific parts of character logic.
public interface ICharacter {
    IPerceptionEventInvoker RequestPerceptionEventListeningRights();
}

// Serves as the generic type for instaces which control the Character GameObject (and relevant components) in order to make the character
// shoot/fight enemies - spotting enemies and engaging enemies
public interface ICombatAi {
    // Events which this AI can invoke, to signal to other entities when the AI makes decisions or does something.
    event EventHandler<TargetInformation> EnemySpottedEvent;
    event EventHandler<TargetInformation> EnemyEngagedEvent;
}

// Serves as teh generic type for instances which control the Character GameObject in a way which in vovles moving it to particular positions.
public interface IMovementAi {
    // UNUSED FOR NOW.
}

public interface IHealthSystem {
    float CurrentHealth { get; }
    float MaxHealth { get;  }
    event Action<IHealthSystem> DamageTakenEvent;
}

public interface IPerceptionEventInvoker {
    // Specifies a series of events which can be listened to by anyone who has access to these events.
    // These will be subject to change, as the testing environment is developed in parallel to the main AI system, and greater complexity in the framework for
    // AI detection and so forth is required to facilitate the more complicated types of perceptions that the AI system should be designed to handle.
    // For now, it's just about spotting, losing track of, and engaging in combat, enemies.
    event EventHandler<ICharacter> EnemySpottedEvent;
    event EventHandler<ICharacter> EnemyLostEvent;
    event EventHandler<ICharacter> PossibleThreatDetectedEvent;
    event EventHandler<ICharacter> EnemyEngagedEvent;
    event EventHandler<ICharacter> EnemyDisengagedEvent;
}

public interface ICharacterAwarenessState {
    // Specifies controls for parameterising and manipulating aspects of the character's awareness ('mental state') logic. 
    // Note that this is mainly for commanding, not listening to changes. The character mental states are communicated through
    // an event driven system which is listened to through the IPerceptionEventInvoker interface.

    // UNUSED FOR NOW.
}

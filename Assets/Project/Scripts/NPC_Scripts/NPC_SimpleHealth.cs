using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(Damageable_EventInvoker))]
public class NPC_SimpleHealth : MonoBehaviour, IHealthSystem {

    public float maxHealth;

    private float currentHealth;

    public float CurrentHealth {
        get {
            return currentHealth;
        }
    }

    public float MaxHealth {
        get {
            return maxHealth;
        }
    }

    public event Action<IHealthSystem> DamageTakenEvent;

    // Use this for initialization
    void Start () {
        GetComponent<Damageable_EventInvoker>().OnHitByProjectile += TakeDamage;
        currentHealth = maxHealth;
	}
	
	private void TakeDamage(object projectile, ProjectileHitEventArgs projectileStats) {
        GetComponent<Rigidbody>()?.AddForceAtPosition(projectileStats.forceDirection, projectileStats.hitPosition);
        DamageTakenEvent?.Invoke(this);

        currentHealth -= projectileStats.projectileDamage;
        if (currentHealth <= 0) {
            Destroy(gameObject);
        }
    }
}

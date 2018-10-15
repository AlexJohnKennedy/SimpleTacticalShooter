using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Damageable_EventInvoker))]
public class NPC_SimpleHealth : MonoBehaviour {

    public float maxHealth;

    private float currentHealth;

	// Use this for initialization
	void Start () {
        GetComponent<Damageable_EventInvoker>().OnHitByProjectile += TakeDamage;
        currentHealth = maxHealth;
	}
	
	private void TakeDamage(object projectile, ProjectileHitEventArgs projectileStats) {
        GetComponent<Rigidbody>()?.AddForceAtPosition(projectileStats.forceDirection, projectileStats.hitPosition);

        currentHealth -= projectileStats.projectileDamage;
        if (currentHealth <= 0) {
            Destroy(gameObject);
        }
    }
}

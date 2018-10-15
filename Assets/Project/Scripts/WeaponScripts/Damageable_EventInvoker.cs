using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/* Attach to any object which can receive damage or serve as a hittable target */
public class Damageable_EventInvoker : MonoBehaviour {

    public event EventHandler<ProjectileHitEventArgs> OnHitByProjectile;

    public void OnHit(object invoker, ProjectileHitEventArgs args) {
        OnHitByProjectile?.Invoke(invoker, args);
    }
}

public class ProjectileHitEventArgs : EventArgs {
    public float projectileDamage;
    public float projectileForce;
    public Vector3 forceDirection;
    public float projectileSpeed;
    public Vector3 hitPosition;

    public ProjectileHitEventArgs(float projectileDamage, float projectileForce, Vector3 forceDirection, float projectileSpeed, Vector3 hitPosition) {
        this.projectileDamage = projectileDamage;
        this.projectileForce = projectileForce;
        this.forceDirection = forceDirection;
        this.projectileSpeed = projectileSpeed;
        this.hitPosition = hitPosition;
    }
}

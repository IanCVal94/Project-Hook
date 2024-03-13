using System;
using A2DK.Phys;
using ASK.Core;
using UnityEngine;
using Combat;
using Helpers;
using UnityEngine.Events;

namespace Mechanics
{
    public class ExplosionEffect : MonoBehaviour
    {
        public float ExplosionRadius = 25f;
        public float KnockbackForce = 100f;

        public void Explode()
        {
            Actor[] actors = FindObjectsOfType<Actor>();
            Crate[] crates = FindObjectsOfType<Crate>();

            foreach (Actor actor in actors)
            {
                Vector3 direction = actor.transform.position - transform.position;
                // Check if the actor is within the explosion radius
                if (direction.magnitude <= ExplosionRadius)
                {
                    // Normalize the direction vector to get a unit vector
                    direction.Normalize();
                    // Apply knockback force to the actor
                    actor.SetVelocity(direction * KnockbackForce);
                }
            }

            foreach (Crate crate in crates)
            {
                Vector3 direction = crate.transform.position - transform.position;
                // Check if the crate is within the explosion radius
                if (direction.magnitude <= ExplosionRadius)
                {
                    // Normalize the direction vector to get a unit vector
                    direction.Normalize();
                    // Apply knockback force to the crate
                    crate.ReceivePunch(direction * KnockbackForce);
                }
            }
        }
    }
}

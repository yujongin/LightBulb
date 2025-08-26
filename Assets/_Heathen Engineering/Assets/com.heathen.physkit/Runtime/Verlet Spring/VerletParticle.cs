#if HE_SYSCORE

using UnityEngine;
using System;

namespace HeathenEngineering.PhysKit
{
    /// <summary>
    /// Represents a node in a physics bone tree.
    /// </summary>
    [Serializable]
    public class VerletParticle
    {
        public Transform target;
        public VerletParticle parent;
        public float damping;
        public float elasticity;
        public float stiffness;
        public float inert;
        public float falloffAngle;

        public float collisionRadius;
        public float distance;

        public Vector3 addedForce;
        public float weight;
        public Vector3 position;
        public Vector3 prevPosition;
        public Vector3 initLocalPosition;
        public Quaternion initLocalRotation;
    }
}

#endif

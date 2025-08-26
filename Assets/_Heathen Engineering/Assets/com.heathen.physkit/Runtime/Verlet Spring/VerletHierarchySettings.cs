#if HE_SYSCORE

using UnityEngine;
using System;

namespace HeathenEngineering.PhysKit
{
    [Serializable]
    public class VerletHierarchySettings
    {
        [Header("Properties")]
        public float updateRate = 1f;
        public Vector3 constantVelocity = Vector3.zero;
        public bool clearResetingVelocity = false;
        public float damping = 0.1f;
        public AnimationCurve dampingCurve = AnimationCurve.Linear(0, 1, 1, 1);
        public float elasticity = 0.1f;
        public AnimationCurve elasticityCurve = AnimationCurve.Linear(0, 1, 1, 1);
        public float stiffness = 0.1f;
        public AnimationCurve stiffnessCurve = AnimationCurve.Linear(0, 1, 1, 1);
        public float inert = 0f;
        public AnimationCurve inertCurve = AnimationCurve.Linear(0, 1, 1, 1);
        [Header("Limiters")]
        public LayerMask collisionLayers = 0;
        public float collisionRadius = 0;
        public AnimationCurve radiusCurve = AnimationCurve.Linear(0, 1, 1, 1);
        public float falloffAngle = 0;
        public AnimationCurve falloffCurve = AnimationCurve.Linear(0, 1, 1, 1);
    }
}

#endif
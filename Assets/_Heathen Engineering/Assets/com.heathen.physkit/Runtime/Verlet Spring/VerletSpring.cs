#if HE_SYSCORE

using UnityEngine;
using System.Collections.Generic;

namespace HeathenEngineering.PhysKit
{
    /// <summary>
    /// Applies physics calculations to a set of 'bone trees'
    /// </summary>
    public class VerletSpring : HeathenBehaviour
    {
        /// <summary>
        /// A list of the managed <see cref="VerletHierarchy"/>s
        /// </summary>
        public List<VerletHierarchy> transformHierarchies = new List<VerletHierarchy>();
        
        private Vector3 previousVelocity;
        private Vector3 previousPosition;
        private Vector3 velocity;

        private void Start()
        {
            previousPosition = SelfTransform.position;
            RegisterTrees();
        }

        private void FixedUpdate()
        {
            float time = Time.fixedDeltaTime;
            velocity = (SelfTransform.position - previousPosition);
            previousPosition = SelfTransform.position;

            foreach (var tree in transformHierarchies)
                tree.Update(velocity, time);
        }

        /// <summary>
        /// Registeres the <see cref="transformHierarchies"/> node objects
        /// </summary>
        [ContextMenu("Register Nodes")]
        public void RegisterTrees()
        {
            foreach(var tree in transformHierarchies)
            {
                tree.RegisterNodes();
            }
        }
        
        /// <summary>
        /// Adds a force to all trees
        /// </summary>
        /// <param name="force"></param>
        public void AddForce(Vector3 force)
        {
            foreach (var n in transformHierarchies)
                n.AddForce(force);
        }

        /// <summary>
        /// Adds a force at a given position to all trees
        /// </summary>
        /// <param name="forceMagnitude"></param>
        /// <param name="position"></param>
        public void AddForceAtPosition(float forceMagnitude, Vector3 position)
        {
            foreach (var n in transformHierarchies)
                n.AddForceAtPosition(forceMagnitude, position);
        }

        /// <summary>
        /// Adds a force to a specific bone tree based on tree index
        /// </summary>
        /// <param name="treeIndex"></param>
        /// <param name="force"></param>
        public void AddForce(int treeIndex, Vector3 force)
        {
            if (treeIndex < transformHierarchies.Count && treeIndex >= 0)
                transformHierarchies[treeIndex].AddForce(force);
            else
            {
                if (transformHierarchies == null || transformHierarchies.Count == 0)
                {
                    Debug.LogError("Attempted to add force to tree at index " + treeIndex.ToString() + "; this Physics Bone has no trees.");
                }
                else
                {
                    Debug.LogError("Attempted to add force to tree index " + treeIndex.ToString() + "; this Physics Bone contains " + transformHierarchies.Count.ToString() + "Bone Trees. Index must be a value from 0 to " + (transformHierarchies.Count - 1).ToString());
                }
            }
        }

        /// <summary>
        /// Returns the bone tree at the indicated index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public VerletHierarchy GetTree(int index)
        {
            if (index < transformHierarchies.Count && index >= 0)
                return transformHierarchies[index];
            else
            {
                if (transformHierarchies == null || transformHierarchies.Count == 0)
                {
                    Debug.LogWarning("Attempted to get tree at index " + index.ToString() + "; this Physics Bone has no trees.");
                    return null;
                }
                else
                {
                    Debug.LogWarning("Attempted to get tree at index " + index.ToString() + "; this Physics Bone contains " + transformHierarchies.Count.ToString() + "Bone Trees. Index must be a value from 0 to " + (transformHierarchies.Count - 1).ToString());
                    return null;
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!enabled)
                return;

            foreach (var tree in transformHierarchies)
            {
                if (Application.isEditor && !Application.isPlaying && transform.hasChanged)
                {
                    tree.RegisterNodes();
                }

                foreach (var n in tree.nodes)
                {
                    if (n.parent != null)
                    {
                        Gizmos.color = new Color(1f, 1f, 0.75f, 1f);
                        Vector3 direction = n.parent.target.position - n.target.position;
                        float length = direction.magnitude;

                        if (length == 0)
                            continue;

                        Matrix4x4 cTrans = Matrix4x4.TRS(n.target.position + (direction * 0.5f), Quaternion.LookRotation(direction, Vector3.up), new Vector3(Mathf.Clamp(0.05f * length, 0.01f, .1f), Mathf.Clamp(0.05f * length, 0.01f, .1f), length));
                        Matrix4x4 pTrans = Gizmos.matrix;
                        Gizmos.matrix *= cTrans;
                        Gizmos.DrawCube(Vector3.zero, Vector3.one);
                        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                        Gizmos.matrix = pTrans;

                        if (n.collisionRadius > 0)
                        {
                            Gizmos.color = new Color(.5f, 1f, 0.5f, 1f);
                            Gizmos.DrawWireSphere(n.target.position, n.collisionRadius);
                            cTrans = Matrix4x4.TRS(n.target.position + (direction * 0.5f), Quaternion.LookRotation(direction, Vector3.up), new Vector3(n.collisionRadius * 1.65f, n.collisionRadius * 1.65f, length));
                            pTrans = Gizmos.matrix;
                            Gizmos.matrix *= cTrans;
                            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                            Gizmos.matrix = pTrans;
                        }
                    }
                }
            }
        }
    }
}


#endif
#if HE_SYSCORE

using UnityEngine;
using System.Collections.Generic;
using System;

namespace HeathenEngineering.PhysKit
{
    /// <summary>
    /// Applies physics calculations to a set of 'bones' in a hierarchal structure
    /// </summary>
    [Serializable]
    public class VerletHierarchy
    {
        /// <summary>
        /// The root of the tree
        /// </summary>
        public Transform root;
        /// <summary>
        /// The settings to be applied to this tree
        /// </summary>
        public VerletHierarchySettingsReference settings = new VerletHierarchySettingsReference(new VerletHierarchySettings());
        /// <summary>
        /// The child nodes of the root of this tree
        /// </summary>
        public List<VerletParticle> nodes = new List<VerletParticle>();
        /// <summary>
        /// The list of transforms to ignore when constructing child nodes
        /// </summary>
        public List<Transform> ignoreList = new List<Transform>();

        private float scalar;
        public Vector3 restingVelocity;

        private float distance;
        
        /// <summary>
        /// Scans the transforms from the root down and constructs <see cref="VerletParticle"/>s
        /// </summary>
        public void RegisterNodes()
        {
            if (settings.Mode == VariableReferenceType.Referenced
                && settings.Variable == null)
                return;

            //copy any old nodes
            List<VerletParticle> oldNodes = new List<VerletParticle>(nodes);

            if (Application.isPlaying)
                ResetNodes();

            nodes.Clear();

            AddNodes(root, null, 0);
            SetNodeData();

            scalar = root.lossyScale.x;

            restingVelocity = root.InverseTransformDirection(settings.Value.constantVelocity);
        }

        private void AddNodes(Transform node, VerletParticle parent, float boneDistance)
        {
            VerletParticle n = new VerletParticle();
            n.target = node;
            n.parent = parent;
            if (node != null)
            {
                n.position = n.prevPosition = node.position;
                n.initLocalPosition = node.localPosition;
                n.initLocalRotation = node.localRotation;
            }

            if (parent != null)
            {
                boneDistance += (parent.target.position - n.position).magnitude;
                n.distance = boneDistance;
                distance = Mathf.Max(distance, boneDistance);
            }

            nodes.Add(n);

            if (node != null)
            {
                foreach (Transform c in node)
                {
                    if (!ignoreList.Contains(c))
                    {
                        AddNodes(c, n, boneDistance);
                    }
                }
            }
        }

        private void SetNodeData()
        {
            if (root == null || (settings.Mode == VariableReferenceType.Referenced && settings.m_variable == null))
                return;

            float weight = 0;

            foreach (var n in nodes)
            {
                weight = distance == 0 ? 0 : n.distance / distance;
                n.weight = weight;
                n.damping = Mathf.Clamp01(settings.Value.damping * settings.Value.dampingCurve.Evaluate(weight));
                n.elasticity = Mathf.Clamp01(settings.Value.elasticity * settings.Value.elasticityCurve.Evaluate(weight));
                n.stiffness = Mathf.Clamp01(settings.Value.stiffness * settings.Value.stiffnessCurve.Evaluate(weight));
                n.inert = Mathf.Clamp01(settings.Value.inert * settings.Value.inertCurve.Evaluate(weight));
                n.falloffAngle = Mathf.Clamp(settings.Value.falloffAngle * settings.Value.falloffCurve.Evaluate(weight), 0, 360);
                n.collisionRadius = Mathf.Max(0, settings.Value.collisionRadius * settings.Value.radiusCurve.Evaluate(weight));
            }
        }

        private Vector3 GetNodeCollisionResult(VerletParticle node, LayerMask layerMask)
        {
            if (Physics.SphereCast(new Ray(node.parent.position, (node.position - node.parent.position).normalized), node.collisionRadius, out RaycastHit hit, Vector3.Distance(node.position, node.parent.position), layerMask))
            {
                var pointOnLine = NearestPointOnLineSegment(node.parent.position, node.position, hit.point);
                var penitrationHeading = (hit.point - pointOnLine);
                var depenitrationShift = -penitrationHeading.normalized * (node.collisionRadius - penitrationHeading.magnitude);

                return node.position + depenitrationShift;
            }
            return node.position;
        }

        public void Update(Vector3 velocity, float time)
        {
            UnifiedUpdate(velocity, settings.Value.constantVelocity, settings.Value.clearResetingVelocity, time * settings.Value.updateRate, settings.Value.collisionLayers);
        }

        private Vector3 GetNodeNormalizedPosition(Vector3 currentPosition, Vector3 parentPosition, float length)
        {
            return parentPosition + ((currentPosition - parentPosition).normalized * length);
        }

        private void UnifiedUpdate(Vector3 velocity, Vector3 constantVelocity, bool useRestingVelocity, float time, LayerMask layerMask)
        {
            foreach (var n in nodes)
            {
                n.target.localPosition = n.initLocalPosition;
                n.target.localRotation = n.initLocalRotation;

                n.damping = Mathf.Clamp01(settings.Value.damping * settings.Value.dampingCurve.Evaluate(n.weight));
                n.elasticity = Mathf.Clamp01(settings.Value.elasticity * settings.Value.elasticityCurve.Evaluate(n.weight));
                n.stiffness = Mathf.Clamp01(settings.Value.stiffness * settings.Value.stiffnessCurve.Evaluate(n.weight));
                n.inert = Mathf.Clamp01(settings.Value.inert * settings.Value.inertCurve.Evaluate(n.weight));
                n.falloffAngle = Mathf.Clamp(settings.Value.falloffAngle * settings.Value.falloffCurve.Evaluate(n.weight), 0, 360);
                n.collisionRadius = Mathf.Max(0, settings.Value.collisionRadius * settings.Value.radiusCurve.Evaluate(n.weight));
            }

            if (useRestingVelocity)
            {
                var gravDirection = constantVelocity.normalized;
                var systemRestingVelocity = root.TransformDirection(restingVelocity);
                constantVelocity -= gravDirection * Mathf.Max(Vector3.Dot(systemRestingVelocity, gravDirection), 0);
            }

            foreach (var n in nodes)
            {
                if (n.target != root)
                {
                    var nVelocity = (constantVelocity + n.addedForce) * scalar * time;
                    n.addedForce = Vector3.zero;

                    var nodeVelocity = n.position - n.prevPosition;
                    var sourceVelocity = velocity * n.inert;
                    n.prevPosition = n.position + sourceVelocity;
                    n.position += nodeVelocity * (1f - n.damping) + (nVelocity + sourceVelocity);

                    var restLength = (n.parent.target.position - n.target.position).magnitude;

                    // Col Tap 1
                    if (n.collisionRadius > 0)
                        n.position = GetNodeCollisionResult(n, layerMask);

                    var delta = Vector3.zero;

                    if (n.stiffness > 0 || n.elasticity > 0)
                    {
                        Matrix4x4 projectionMatrix = n.parent.target.localToWorldMatrix;
                        projectionMatrix.SetColumn(3, n.parent.position);
                        var restPosition = projectionMatrix.MultiplyPoint3x4(n.initLocalPosition);

                        delta = restPosition - n.position;
                        n.position += delta * (n.elasticity * time);

                        if (n.stiffness > 0)
                        {
                            delta = restPosition - n.position;
                            var distance = delta.magnitude;
                            var limit = restLength * (1 - n.stiffness) * 2;
                            if (distance > limit)
                                n.position += delta * ((distance - limit) / distance);
                        }
                    }

                    var heading = n.parent.position - n.position;

                    if (n.falloffAngle > 0)
                    {
                        Matrix4x4 projectionMatrix = n.parent.target.localToWorldMatrix;
                        projectionMatrix.SetColumn(3, n.parent.position);
                        var restPosition = projectionMatrix.MultiplyPoint3x4(n.initLocalPosition);

                        var restingDirection = (n.parent.position - restPosition).normalized;
                        float angle = Quaternion.Angle(Quaternion.LookRotation(heading.normalized), Quaternion.LookRotation(restingDirection));
                        angle = Mathf.Clamp01(angle / n.falloffAngle);

                        n.position = Vector3.Lerp(n.position, restPosition, angle);
                                                
                        heading = n.parent.position - n.position;
                    }

                    var actualLength = heading.magnitude;
                    n.position += heading * ((actualLength - restLength) / actualLength);

                    // Col Tap 2
                    if (n.collisionRadius > 0)
                        n.position = GetNodeCollisionResult(n, layerMask);

                    //Back propigate transform by firmness
                    if (n.parent.parent != null && n.parent.stiffness != 0)
                    {
                        Vector3 difference = n.position - n.parent.target.TransformPoint(n.initLocalPosition);
                        Vector3 firmnessVelocity = -delta * n.parent.stiffness * time;

                        n.parent.position += firmnessVelocity;

                        if (n.parent.collisionRadius > 0)
                        {
                            //Check for collisions
                            n.parent.position = GetNodeCollisionResult(n.parent, layerMask);
                        }

                        //Insure we dont change our length (parent)
                        n.parent.position = GetNodeNormalizedPosition(n.parent.position, n.parent.parent.position, n.parent.initLocalPosition.magnitude);
                        n.position = GetNodeNormalizedPosition(n.position, n.parent.position, restLength);

                        var targetDirection2 = n.parent.position - n.parent.parent.position;
                        var rot2 = Quaternion.FromToRotation(n.parent.parent.target.TransformDirection(n.parent.target.localPosition), targetDirection2);
                        n.parent.parent.target.rotation = rot2 * n.parent.parent.target.rotation;
                    }

                    var targetDirection = n.position - n.parent.position;
                    var rot = Quaternion.FromToRotation(n.parent.target.TransformDirection(n.target.localPosition), targetDirection);
                    n.parent.target.rotation = rot * n.parent.target.rotation;

                    n.target.position = n.position;
                }
                else
                {
                    n.prevPosition = n.position;
                    n.position = n.target.position;
                }
            }
        }

        /// <summary>
        /// Adds a force to all nodes in the tree
        /// </summary>
        /// <param name="force"></param>
        public void AddForce(Vector3 force)
        {
            foreach (var n in nodes)
                n.addedForce += force;
        }

        /// <summary>
        /// Adds a force to all nodes in the tree originating from the position provided and dampend by the length of the node over the node distance from the force position
        /// </summary>
        /// <param name="forceMagnitude">The strength of the force</param>
        /// <param name="position">The force's point of origin</param>
        public void AddForceAtPosition(float forceMagnitude, Vector3 position)
        {
            foreach(var n in nodes)
            {
                var d = Vector3.Distance(position, n.target.position);
                var l = n.parent != null ? Vector3.Distance(n.parent.target.position, n.target.position) : 0;
                if(l > 0 && d < l)
                {
                    var force = (n.target.position - position).normalized * (forceMagnitude * (d/l));
                    n.addedForce += force;
                }
            }
        }

        /// <summary>
        /// Return all nodes to a rested state
        /// </summary>
        public void ResetNodes()
        {
            if (nodes.Count > 0)
            {
                foreach (var n in nodes)
                {
                    n.target.localPosition = n.initLocalPosition;
                    n.target.localRotation = n.initLocalRotation;
                    n.prevPosition = n.target.position;
                }
            }
        }

        private Vector3 NearestPointOnLineSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 subject)
        {
            var line = (lineEnd - lineStart);
            var length = line.magnitude;
            line.Normalize();

            var subjectHeading = subject - lineStart;
            var dot = Vector3.Dot(subjectHeading, line);
            dot = Mathf.Clamp(dot, 0f, length);
            return lineStart + line * dot;
        }
    }
}

#endif
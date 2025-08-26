#if HE_SYSCORE

using System;

namespace HeathenEngineering.PhysKit
{
    /// <summary>
    /// Enables a develoepr to create a reference field for the <see cref="VerletHierarchySettings"/> object
    /// </summary>
    [Serializable]
    public class VerletHierarchySettingsReference : VariableReference<VerletHierarchySettings>
    {
        public VerletHierarchySettingsVariable Variable;
        public override IDataVariable<VerletHierarchySettings> m_variable => Variable;

        public VerletHierarchySettingsReference(VerletHierarchySettings value) : base(value)
        { }

        public static implicit operator VerletHierarchySettings(VerletHierarchySettingsReference reference)
        {
            return reference.Value;
        }
    }
}

#endif
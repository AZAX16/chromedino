using UnityEngine;

namespace ChromeDino
{
    internal static class SetupValidation
    {
        public static bool Require(Object reference, Component owner, string fieldName)
        {
            if (reference != null)
            {
                return true;
            }

            Debug.LogError($"{owner.name} ({owner.GetType().Name}): assign {fieldName}. See Docs/UnitySetup.md.", owner);
            return false;
        }

        public static bool Check(bool condition, Component owner, string message)
        {
            if (!condition)
            {
                Debug.LogError($"{owner.name} ({owner.GetType().Name}): {message}", owner);
            }

            return condition;
        }
    }
}

using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Inspector wiring checks, run once on Awake.
    ///
    /// Every component in this feature guards its subscriptions with a null check but raises its
    /// channels unguarded — so a field left empty in the inspector does not fail when the scene
    /// loads, it fails minutes later inside whatever tried to raise it, as a bare
    /// NullReferenceException with no field name in it. This says which field, on which object,
    /// while the scene is still loading.
    /// </summary>
    public static class Wiring
    {
        public static bool Require(Object owner, Object reference, string fieldName)
        {
            if (reference != null)
            {
                return true;
            }
            Debug.LogError($"{owner.GetType().Name} on '{owner.name}': {fieldName} is not assigned.", owner);
            return false;
        }
    }
}

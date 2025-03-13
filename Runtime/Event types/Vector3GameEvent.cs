// Parameterized GameEvent

using UnityEngine;

namespace SO_Events.Runtime.Event_types
{
    [CreateAssetMenu(menuName = "Events/Vector3 Game Event")]
    public sealed class Vector3GameEvent : BaseGameEvent<Vector3>
    {
    }
}
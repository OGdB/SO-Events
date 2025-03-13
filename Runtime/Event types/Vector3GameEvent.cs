// Parameterized GameEvent

using _Scripts.Events_system;
using UnityEngine;

namespace Event_Bus_System.Event_types
{
    [CreateAssetMenu(menuName = "Events/Vector3 Game Event")]
    public sealed class Vector3GameEvent : BaseGameEvent<Vector3>
    {
    }
}
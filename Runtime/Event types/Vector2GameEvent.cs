using _Scripts.Events_system;
using UnityEngine;

namespace Event_Bus_System.Event_types
{
    [CreateAssetMenu(menuName = "Events/Vector2 Game Event")]
    public sealed class Vector2GameEvent : BaseGameEvent<Vector2>
    {
    }
}
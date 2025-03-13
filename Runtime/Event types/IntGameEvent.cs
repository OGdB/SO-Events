using _Scripts.Events_system;
using UnityEngine;

namespace Event_Bus_System.Event_types
{
    [CreateAssetMenu(menuName = "Events/Int Game Event")]
    public sealed class IntGameEvent : BaseGameEvent<int>
    {
    }
}
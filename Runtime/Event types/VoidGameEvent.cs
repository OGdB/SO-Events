// Parameterless GameEvent

using _Scripts.Events_system;
using UnityEngine;

namespace Event_Bus_System.Event_types
{
    [CreateAssetMenu(menuName = "Events/Void Game Event")]
    public sealed class VoidGameEvent : BaseGameEvent
    {
    }
}
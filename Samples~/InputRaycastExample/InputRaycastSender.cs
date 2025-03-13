using SO_Events.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Input_Raycast_System.Runtime
{
    public class InputRaycastNewInputSystem : MonoBehaviour
    {
        [Tooltip("Reference to your ScriptableObject event for RaycastHit notifications.")]
        [SerializeField] private BaseGameEvent<RaycastHit> raycastHitEvent;
    
        [Tooltip("Raycast distance.")]
        [SerializeField] private float rayDistance = 100f;
    
        [Tooltip("Optional layer mask for raycasting.")]
        [SerializeField] private LayerMask raycastLayerMask = Physics.DefaultRaycastLayers;

        private InputAction _pointerPressAction;

        void Awake()
        {
            // Initialize an InputAction that listens to pointer press (compatible with mouse and touchscreen)
            _pointerPressAction = new InputAction(
                "PointerPress",
                binding: "<Pointer>/press",
                interactions: "press"  // This makes sure the action triggers on a press
            );

            // Subscribe to the performed event.
            _pointerPressAction.performed += OnPointerPressed;
        }

        void OnEnable() => _pointerPressAction.Enable();

        void OnDisable() => _pointerPressAction.Disable();

        /// <summary>
        /// Callback for the InputAction performed event.
        /// Fires a ray from the main camera using the current pointer position.
        /// </summary>
        /// <param name="context">The callback context.</param>
        private void OnPointerPressed(InputAction.CallbackContext context)
        {
            // Get the current pointer position. With the new Input System the pointer(s) are unified.
            Vector2 screenPosition = Mouse.current?.position.ReadValue() ?? Vector2.zero;
            // If running on a mobile device, the touchscreen should also set a pointer position.
            if (screenPosition == Vector2.zero && Touchscreen.current != null)
            {
                screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            }

            // Confirm we got a valid pointer position
            if (screenPosition == Vector2.zero)
            {
                Debug.LogWarning("Pointer position is zero. No valid input detected.");
                return;
            }

            // Create a ray from the Main Camera to the pointer position.
            if (Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(screenPosition);
                if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, raycastLayerMask))
                {
                    // Raise the event to notify all listeners, if any.
                    if (raycastHitEvent != null)
                    {
                        raycastHitEvent.Raise(hit);
                    }
                    else
                    {
                        Debug.LogWarning("RaycastHit event reference is missing. Please assign it in the inspector.");
                    }
                }
                else
                {
                    Debug.Log("No collider was hit by the raycast.");
                }
            }
        }
    }
}
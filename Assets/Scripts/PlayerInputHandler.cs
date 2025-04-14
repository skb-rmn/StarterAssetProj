using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputHandler : MonoBehaviour
{
    // Assign your Input Action Asset via the Inspector.
    public InputActionAsset inputActions;

    // Input actions from the action map.
    private InputAction crouchAction;
    private InputAction moveAction;

    // Expose the current crouch state.
    public bool IsCrouching { get; private set; } = false;

    // Event to notify subscribers when crouch state changes.
    public event Action<bool> OnCrouchChanged;

    void Awake()
    {
        // Assuming your action map is named "Player"
        var playerActionMap = inputActions.FindActionMap("Player", throwIfNotFound: true);
        crouchAction = playerActionMap.FindAction("Crouch", throwIfNotFound: true);
        moveAction = playerActionMap.FindAction("Move", throwIfNotFound: true);

        // Subscribe to the performed event on the crouch action.
        crouchAction.performed += HandleCrouch;
    }

    void OnEnable()
    {
        // Enable input actions.
        crouchAction.Enable();
        moveAction.Enable();
    }

    void OnDisable()
    {
        // Unsubscribe and disable input actions.
        crouchAction.performed -= HandleCrouch;
        crouchAction.Disable();
        moveAction.Disable();
    }

    // Called when the "Crouch" input action is performed.
    private void HandleCrouch(InputAction.CallbackContext context)
    {
        // Toggle crouch state.
        IsCrouching = !IsCrouching;
        OnCrouchChanged?.Invoke(IsCrouching);
    }

    // (Optional) Expose a method for reading movement input.
    public Vector2 GetMoveInput()
    {
        return moveAction.ReadValue<Vector2>();
    }
}

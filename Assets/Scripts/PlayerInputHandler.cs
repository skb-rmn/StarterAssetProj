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
    private InputAction attackAction;

    // Expose the current crouch state.
    public bool IsCrouching { get; private set; } = false;

    // Event to notify subscribers when crouch state changes.
    public event Action<bool> OnCrouchChanged;
    public event Action OnAttack;

    void Awake()
    {
        // Assuming your action map is named "Player"
        var playerActionMap = inputActions.FindActionMap("Player", throwIfNotFound: true);
        crouchAction = playerActionMap.FindAction("Crouch", throwIfNotFound: true);
        moveAction = playerActionMap.FindAction("Move", throwIfNotFound: true);
        attackAction = playerActionMap.FindAction("Attack", throwIfNotFound: true);

        // Subscribe to the performed event on the crouch action.
        crouchAction.performed += HandleCrouch;
        attackAction.performed += HandleAttack;
        
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
        attackAction.performed -= HandleAttack;
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

    private void HandleAttack(InputAction.CallbackContext context)
    {
        Debug.Log($"Attack Clicked - {context.phase}");
        if (context.performed) OnAttack?.Invoke();
    }

    // (Optional) Expose a method for reading movement input.
    public Vector2 GetMoveInput()
    {
        return moveAction.ReadValue<Vector2>();
    }
}

using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerCrouchController : MonoBehaviour
{
    private Animator animator;
    private CharacterController characterController;
    private PlayerInputHandler inputHandler;

    [Header("Crouch Settings")]
    public float crouchHeight = 1.0f;
    public Vector3 crouchCenter = new Vector3(0, 0.5f, 0);

    [Header("Standing Settings")]
    public float standingHeight = 1.8f;
    public Vector3 standingCenter = new Vector3(0, 0.9f, 0);

    [Header("Speed")]
    public float speedDampTime = 0.1f;

    private ThirdPersonController controllerScript;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        characterController = GetComponent<CharacterController>();
        inputHandler = GetComponent<PlayerInputHandler>();
        controllerScript = GetComponent<ThirdPersonController>(); // From Starter Assets

        if (inputHandler != null)
        {
            inputHandler.OnCrouchChanged += HandleCrouchChanged;
        }
    }

    void OnDestroy()
    {
        if (inputHandler != null)
        {
            inputHandler.OnCrouchChanged -= HandleCrouchChanged;
        }
    }

    void Update()
    {
        if (inputHandler.IsCrouching)
        {
            float speed = controllerScript.GetCurrentSpeed(); // You may need to expose this in ThirdPersonController
            animator.SetFloat("Speed", speed, speedDampTime, Time.deltaTime);
        }
    }

    private void HandleCrouchChanged(bool isCrouching)
    {
        animator.SetBool("Crouch", isCrouching);

        if (characterController != null)
        {
            characterController.height = isCrouching ? crouchHeight : standingHeight;
            characterController.center = isCrouching ? crouchCenter : standingCenter;
        }
    }
}

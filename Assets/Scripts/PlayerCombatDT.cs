using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

public class PlayerCombatDT : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Drag in your Move action (Vector2) from your Input Actions asset or PlayerInput.")]
    public PlayerInputHandler inputActions;
    public ThirdPersonController thirdPersonController;
    public LayerMask layerMask;

    [Header("Debug")]
    [Tooltip("Enable to draw a ray showing your current input direction.")]
    public bool debugMode = true;
    [Tooltip("How long the debug ray should be.")]
    public float debugLength = 2f;
    [Tooltip("Height above the player pivot where the debug ray starts.")]
    public float debugHeight = 1f;
    [Tooltip("Color of the debug ray.")]
    public UnityEngine.Color debugColor = UnityEngine.Color.red;

    // Raw 2D input from the stick/keys
    private Vector2 _rawInput;
    // Final normalized world‑space direction
    private Vector3 _inputDirection;

    void OnEnable()
    {
        //if (moveAction != null)
        //    moveAction.action.Enable();
    }

    void OnDisable()
    {
        //if (moveAction != null)
        //    moveAction.action.Disable();
    }
    private void Awake()
    {
        
    }

    void Update()
    {
        ReadInput();
        CalculateDirection();
        DetectEnemy();

        if (debugMode)
            DrawDebugRay();

        // TODO: use _inputDirection for your combat aiming/attacks, rotation, etc.
    }

    RaycastHit hit;

    void DetectEnemy()
    {
        Vector3 origin = transform.position + Vector3.up * debugHeight + _inputDirection * 0.5f;
        if (Physics.Raycast(origin , _inputDirection, out hit, debugLength, layerMask))
        {
            Debug.Log($"Hit - {hit.collider.name}");
            //DebugExtension.DebugWireSphere(hit.collider.transform.position, debugWireSphere_Color, debugWireSphere_Radius);
            if(hit.collider.transform.GetComponent<EnemyBasic>() != null)
            {
                Debug.Log("Enenmy");
                DebugShapes.DebugDrawSphere
                (
                    center: hit.collider.transform.position,
                    radius: debugWireSphere_Radius,
                    color: debugColor,
                    duration: 0f,
                    depthTest: true,
                    segments: 36
                );
            }
        }
    }

    /// <summary>
    /// Reads the 2D Vector from your Move action.
    /// </summary>
    void ReadInput()
    {
        if (inputActions != null)
            _rawInput = inputActions.GetMoveInput();
        else
            _rawInput = Vector2.zero;
    }

    /// <summary>
    /// Converts raw input into a camera‑relative, normalized world direction.
    /// </summary>
    void CalculateDirection()
    {
        Transform cam = Camera.main.transform;

        // Flatten camera forward/right on the XZ plane
        Vector3 forward = cam.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cam.right;
        right.y = 0f;
        right.Normalize();

        // Combine input axes with camera orientation
        Vector3 worldInput = forward * _rawInput.y + right * _rawInput.x;

        // Normalize so magnitude is always 1 (or zero if there's no input)
        _inputDirection = worldInput.normalized;
        //Debug.Log(_inputDirection);
    }

    /// <summary>
    /// Draws a debug ray in the Scene/Game view so you can visualize _inputDirection.
    /// </summary>
    void DrawDebugRay()
    {
        Vector3 origin = transform.position + Vector3.up * debugHeight + _inputDirection * 0.5f;
        Debug.DrawRay(origin, _inputDirection * debugLength, debugColor);

    }

    [Header("Enemy Detection Sphere")]
    public UnityEngine.Color debugWireSphere_Color;
    public float debugWireSphere_Radius;
    //float
    public bool debugWireSphere;
    

    //private void OnDrawGizmos()
    //{

    //    if (hit.collider && debugWireSphere)
    //    {
    //        Gizmos.color = debugWireSphere_Color;
    //        Gizmos.DrawWireSphere(hit.collider.transform.position, debugWireSphere_Radius);
    //    }

    //}
}

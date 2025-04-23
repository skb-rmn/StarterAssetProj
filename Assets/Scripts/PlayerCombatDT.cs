using DG.Tweening;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using UnityEditor.Animations;
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
    public Animator anim;

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
        if(inputActions != null)
        {
            inputActions.OnAttack += HandleAttack;
        }
        _contactTimes = new Dictionary<int, float>();

#if UNITY_EDITOR
        // Grab the AnimatorController asset
        var controller = anim.runtimeAnimatorController as AnimatorController;

        if (controller == null)
        {
            Debug.LogError("AnimatorController not found.");
            return;
        }

        var attackClips = new List<AnimationClip>();

        foreach (var layer in controller.layers)
        {
            var stateMachine = layer.stateMachine;
            foreach (var state in stateMachine.states)
            {
                if (state.state.tag == "Attack")
                {
                    var motion = state.state.motion;

                    if (motion is AnimationClip clip)
                    {
                        attackClips.Add(clip);

                        // Try to find a contact event
                        float contactTime = clip.length;
                        foreach (var evt in clip.events)
                        {
                            if (evt.functionName == "OnContact")
                            {
                                contactTime = evt.time * 0.5f;
                                break;
                            }
                        }

                        _contactTimes[attackClips.Count - 1] = contactTime;
                    }
                }
            }
        }

        this._attackClips = attackClips.ToArray();
#else
    Debug.LogWarning("Runtime clip loading by tag is editor-only. Please assign clips manually in build.");


        // Runtime: grab all clips whose names contain "Attack"
        _attackClips = anim.runtimeAnimatorController
            .animationClips
            .Where(c => c.name.Contains("Attack"))
            .ToArray();

        if (_attackClips.Length == 0)
        {
            Debug.LogWarning("No attack clips found by name filter. Ensure clips are named with 'Attack'.");
        }

        // Extract contact-phase end times
        for (int i = 0; i < _attackClips.Length; i++)
        {
            var clip = _attackClips[i];
            float contactTime = clip.length;

            // Look for an AnimationEvent named "OnContactEnd"
            foreach (var ae in clip.events)
            {
                if (ae.functionName == nameof(OnContact))
                {
                    contactTime = ae.time;
                    break;
                }
            }

            _contactTimes[i] = contactTime;
        }
#endif
    }
    private void OnDestroy()
    {
        if(inputActions != null)
        {
            inputActions.OnAttack -= HandleAttack;
        }
    }

    [Header("Combat Anim")]
    public float moveOffsetFactor = 0.5f;
    public float combatMoveDuration = 0.7f;
    public float animatorSpeedFactor = 1.01f;
    public int attackIndex = 0;

    [Tooltip("Distance (in world units) at which to re-sync animation speed to contact timing.")]
    [SerializeField] private float nearDistanceThreshold = 1f;

    private AnimationClip[] _attackClips;
    private Dictionary<int, float> _contactTimes;
    bool canAttack = true;

    // Runtime tracking
    private float _attackStartTime;
    private bool _speedAdjusted;
    private Vector3 _attackDest;
    private float _totalAttackDistance;


    private void HandleAttack()
    {
        //if (!canAttack) return;

        //Collider target = bestCollider ?? colliders[0];
        //transform.DOMove(target.transform.position - _inputDirection * moveOffsetFactor, combatMoveDuration);
        ////GetComponentInChildren<Animator>().speed = 2f - combatMoveDuration;
        ////GetComponentInChildren<Animator>()?.SetTrigger("Attack");
        ////GetComponentInChildren<Animator>()?.SetInteger("AttackIndex", UnityEngine.Random.Range(0, 2));
        ////canAttack = false;
        //StartCoroutine(PerformAttack());

        if (!canAttack || _attackClips.Length == 0) return;

        // Choose a random attack clip
        attackIndex = UnityEngine.Random.Range(0, _attackClips.Length);
        anim.SetInteger("AttackIndex", attackIndex);

        // Compute playback speed so that contact event lines up with move completion
        float contactTime = _contactTimes[attackIndex];
        float playbackSpeed = contactTime / combatMoveDuration;
        //anim.speed = playbackSpeed * 2f;

        // Trigger Animator
        anim.SetTrigger("Attack");

        // DOTween move
        Collider target = bestCollider ?? colliders[0];
        Vector3 dest = target.transform.position - _inputDirection * moveOffsetFactor;
        transform.DOMove(dest, combatMoveDuration)
                    
                 .OnComplete(() => {
                     ResetAnimatorSpeed();
                     inputActions.MoveAction.Enable();
                     inputActions.AttackAction.Enable();
                     inputActions.OnAttack += HandleAttack;
                 } );

        canAttack = false;

        //if (!canAttack || _attackClips.Length == 0)
        //    return;

        //// Choose a random attack clip (e.g. punch/kick)
        //int attackIndex = UnityEngine.Random.Range(2, _attackClips.Length);
        //anim.SetInteger("AttackIndex", attackIndex);

        //// Trigger Animator
        //anim.SetTrigger("Attack");

        //// Prepare resync variables
        //_attackStartTime = Time.time;
        //_speedAdjusted = false;

        //// Compute move vars
        //Collider target = bestCollider ?? colliders[0];
        //_attackDest = target.transform.position - _inputDirection * moveOffsetFactor;
        //Vector3 startPos = transform.position;
        //_totalAttackDistance = Vector3.Distance(startPos, _attackDest);

        //// Begin move tween
        //transform.DOMove(_attackDest, combatMoveDuration)
        //    .OnUpdate(() =>
        //    {
        //        if (_speedAdjusted) return;

        //        float currentDistance = Vector3.Distance(transform.position, _attackDest);
        //        if (currentDistance <= moveOffsetFactor)
        //        {
        //            // Time since move started
        //            float elapsed = Time.time - _attackStartTime;
        //            float contactTime = _contactTimes[attackIndex];

        //            // Calculate new playback speed so that the contact event
        //            // (at clipTime = contactTime) aligns with now
        //            float newSpeed = contactTime / elapsed;
        //            anim.speed = newSpeed;
        //            _speedAdjusted = true;
        //        }
        //    })
        //    .OnComplete(ResetAnimatorSpeed);

        //canAttack = false;

    }

    private void ResetAnimatorSpeed()
    {
        anim.speed = 1f;
    }

    
    //int attackIndex = 0;

    public void OnContact()
    {
        canAttack = true;
        //attackIndex = 0;
        //GetComponentInChildren<Animator>().speed = 1f;
    }

    public void OnAttackAnimationEnded()
    {
        //GetComponentInChildren<Animator>().speed = 1f;
        
    }

    public void OnAnticipationEnd()
    {
        inputActions.OnAttack -= HandleAttack;
        inputActions.MoveAction.Disable();
        inputActions.AttackAction.Disable();
    }

    Collider[] colliders;
    Collider bestCollider;

    void Update()
    {
        ReadInput();
        CalculateDirection();
        DetectEnemy();

        if (debugMode)
        {
            //DrawDebugRay();
            if(_inputDirection != Vector3.zero)
            {
                DebugShapes.DebugDrawSphere
                (
                    center: bestCollider?.transform.position ?? colliders[0].transform.position,
                    radius: debugWireSphere_Radius,
                    color: debugColor,
                    duration: 0f,
                    depthTest: true,
                    segments: 36
                );
            }
        }

        // TODO: use _inputDirection for your combat aiming/attacks, rotation, etc.
    }

    RaycastHit hit;
    public ConeDetector coneDetector;

    void DetectEnemy()
    {
        if(_inputDirection == Vector3.zero)
        {
            return;
        }
        //colliders = coneDetector.GetTargetsInCone();
        //bestCollider = coneDetector.GetBestTargetInRange();
        
        //Vector3 origin = transform.position + Vector3.up * debugHeight + _inputDirection * 0.5f;
        //if (Physics.Raycast(origin , _inputDirection, out hit, debugLength, layerMask))
        //{
        //    Debug.Log($"Hit - {hit.collider.name}");
        //    //DebugExtension.DebugWireSphere(hit.collider.transform.position, debugWireSphere_Color, debugWireSphere_Radius);
        //    if(hit.collider.transform.GetComponent<EnemyBasic>() != null)
        //    {
        //        Debug.Log("Enenmy");
        //        DebugShapes.DebugDrawSphere
        //        (
        //            center: hit.collider.transform.position,
        //            radius: debugWireSphere_Radius,
        //            color: debugColor,
        //            duration: 0f,
        //            depthTest: true,
        //            segments: 36
        //        );
        //    }
        //}
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

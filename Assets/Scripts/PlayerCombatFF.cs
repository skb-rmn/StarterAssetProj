using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using StarterAssets;
using System.Linq;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerCombatFF : MonoBehaviour
{
    [Header("Combat Settings")]
    public float approachDistance = 1.5f;
    public float approachDuration = 0.3f;
    public float attackCooldown = 1f;
    public LayerMask targetLayer;
    public float targetDetectionRadius = 5f;

    private ThirdPersonController controller;
    private Animator animator;
    private PlayerInputHandler playerInputHandler;
    private bool isAttacking = false;
    private bool canAttack = true;
    private Transform currentTarget;
    [SerializeField]private int attackCount = 3;

    private void Awake()
    {
        controller = GetComponent<ThirdPersonController>();
        animator = GetComponent<Animator>();
        playerInputHandler = GetComponent<PlayerInputHandler>();
    }

    private void OnEnable()
    {
        // Subscribe to the input system's attack event
        var playerInput = GetComponent<PlayerInput>();
        playerInput.actions["Attack"].performed += OnAttackInput;
    }

    private void OnDisable()
    {
        // Unsubscribe from attack event
        var playerInput = GetComponent<PlayerInput>();
        playerInput.actions["Attack"].performed -= OnAttackInput;
    }

    private void OnAttackInput(InputAction.CallbackContext ctx)
    {
        if (canAttack && !isAttacking)
        {
            var movementVector = playerInputHandler.GetMoveInput();// controller.InputVector; // movement from Starter Assets
            FindTargetInDirection(movementVector);

            if (currentTarget != null)
            {
                StartAttackSequence();
            }
        }
    }

    private void FindTargetInDirection(Vector2 inputDir)
    {
        currentTarget = null;
        if (inputDir == Vector2.zero)
            return;

        Transform cam = Camera.main.transform;

        // Flatten camera forward/right on the XZ plane
        Vector3 forward = cam.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cam.right;
        right.y = 0f;
        right.Normalize();

        // Combine input axes with camera orientation
        Vector3 worldDir = forward * inputDir.y + right * inputDir.x;

        //Vector3 /*worldDir*/ = new Vector3(inputDir.x, 0, inputDir.y);
        Collider[] hits = Physics.OverlapSphere(transform.position, targetDetectionRadius, targetLayer);
        float bestDot = 0.5f; // threshold
        foreach (var hit in hits)
        {
            Vector3 toTarget = (hit.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(worldDir.normalized, toTarget);
            if (dot > bestDot)
            {
                bestDot = dot;
                currentTarget = hit.transform;
            }
        }
    }

    private void StartAttackSequence()
    {
        isAttacking = true;
        canAttack = false;
        controller.enabled = false;

        // Pick a random attack
        int idx = Random.Range(0, attackCount);
        animator.SetInteger("AttackIndex", idx);
        animator.Update(0f); // force controller to pick correct clip

        // Fetch the animation clip to sync timings
        var clip = animator.runtimeAnimatorController.animationClips
                     .First(c => c.name.Contains("Attack") && c.events.Any(e => e.functionName == "OnContact"));
        // Find the OnContact event time
        var contactEvent = clip.events.First(e => e.functionName == "OnContact");
        float contactTime = contactEvent.time;
        float clipLength = clip.length;

        // Compute target position slightly back so character doesn't overshoot
        Vector3 targetPos = currentTarget.position - transform.forward * approachDistance;

        // Build a tween sequence: move + play animation + wait remainder + finish
        Sequence seq = DOTween.Sequence();

        // 1) Move to target over contactTime
        seq.Append(transform.DOMove(targetPos, contactTime).SetEase(Ease.OutQuad));

        // 2) At same time, trigger the animation
        seq.JoinCallback(() => animator.SetTrigger("Attack"));

        // 3) Hold until the clip fully plays out
        seq.AppendInterval(clipLength - contactTime);

        // 4) Cleanup: re-enable movement, reset state, cooldown
        seq.AppendCallback(OnAttackComplete);
        seq.Play();
    }

    private void TriggerAttackAnimation()
    {
        int idx = Random.Range(0, attackCount);
        animator.SetInteger("AttackIndex", idx);
        animator.SetTrigger("Attack");
    }

    // Called via Animation Event
    public void OnContact()
    {
        if (currentTarget != null)
        {
            // Deal damage logic here
            Debug.Log($"Hit {currentTarget.name}");
        }
    }

    // Called via Animation Event at end of attack
    public void OnAttackComplete()
    {
        // Re-enable movement
        controller.enabled = true;
        isAttacking = false;
        currentTarget = null;

        // Cooldown
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    private void ResetAttack()
    {
        canAttack = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, targetDetectionRadius);
    }
}

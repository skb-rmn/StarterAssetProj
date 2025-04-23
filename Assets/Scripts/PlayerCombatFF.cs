using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Linq;
using StarterAssets;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerCombatFF : MonoBehaviour
{
    [Header("Combat Settings")]
    public float approachDistance = 1.5f;
    public float attackCooldown = 1f;
    public LayerMask targetLayer;
    public float targetDetectionRadius = 5f;

    private ThirdPersonController controller;
    private Animator animator;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private PlayerInputHandler playerInputHandler;
    private ConeDetector coneDetector;
    private bool isAttacking = false;
    private bool canAttack = true;
    private Transform currentTarget;
    [SerializeField] private int attackCount = 3;

    private void Awake()
    {
        controller = GetComponent<ThirdPersonController>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        playerInputHandler = GetComponent<PlayerInputHandler>();
        coneDetector = GetComponent<ConeDetector>();
    }

    private void OnEnable()
    {
        playerInput.actions["Attack"].performed += OnAttackInput;
    }

    private void OnDisable()
    {
        playerInput.actions["Attack"].performed -= OnAttackInput;
    }

    private void OnAttackInput(InputAction.CallbackContext ctx)
    {
        if (!canAttack || isAttacking) return;

        Vector2 movementVector = playerInputHandler.GetMoveInput();
        FindTargetInDirection(movementVector);

        if (currentTarget != null)
            StartAttackSequence();
    }

    private void FindTargetInDirection(Vector2 inputDir)
    {
        currentTarget = null;
        if (inputDir == Vector2.zero) return;

        Transform cam = Camera.main.transform;
        Vector3 forward = cam.forward; forward.y = 0f; forward.Normalize();
        Vector3 right = cam.right; right.y = 0f; right.Normalize();
        Vector3 worldDir = forward * inputDir.y + right * inputDir.x;

        // Try cone detection first
        var best = coneDetector.GetBestTargetInRange(worldDir);
        if (best != null)
        {
            currentTarget = best.transform;
            return;
        }

        var inCone = coneDetector.GetTargetsInCone(worldDir);
        if (inCone != null && inCone.Length > 0)
        {
            currentTarget = inCone[0].transform;
            return;
        }

        // Fallback to dot-based selection
        Collider[] hits = Physics.OverlapSphere(transform.position, targetDetectionRadius, targetLayer);
        float bestDot = 0.5f;
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

        // disable movement input, keep camera orbit active
        moveAction.Disable();

        // pick and set random attack
        int idx = Random.Range(0, attackCount);
        animator.SetInteger("AttackIndex", idx);
        animator.Update(0f);

        // fetch clip & timings
        var clip = animator.runtimeAnimatorController.animationClips
                     .First(c => c.name.Contains("Attack") && c.events.Any(e => e.functionName == "OnContact"));
        var contactEvent = clip.events.First(e => e.functionName == "OnContact");
        float contactTime = contactEvent.time;
        float clipLength = clip.length;

        // compute target position once
        Vector3 toTarget = (currentTarget.position - transform.position).normalized;
        Vector3 targetPos = currentTarget.position - toTarget * approachDistance;

        // build tween sequence
        Sequence seq = DOTween.Sequence();

        // 1) trigger animation at start
        seq.PrependCallback(() => animator.SetTrigger("Attack"));

        // 2) move to target over contactTime
        seq.Append(transform.DOMove(targetPos, contactTime).SetEase(Ease.OutQuad));

        // 3) wait remainder of clip
        seq.AppendInterval(clipLength - contactTime);

        // 4) cleanup
        seq.AppendCallback(OnAttackComplete);
        seq.Play();
    }

    // Called via Animation Event at hit frame
    public void OnContact()
    {
        if (currentTarget != null)
        {
            Debug.Log($"Hit {currentTarget.name} Dist: { Vector3.Distance(currentTarget.transform.position, this.transform.position)}");
            // TODO: apply damage
            var enemyBasic = currentTarget.GetComponent<EnemyBasic>();
            if(enemyBasic != null  && !enemyBasic.IsHit)
            {
                enemyBasic.TakeDamage(0, currentTarget.transform.position);
            }
        }
    }

    private void OnAttackComplete()
    {
        // re-enable movement input
        moveAction.Enable();

        isAttacking = false;
        currentTarget = null;

        // start cooldown
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    private void ResetAttack() => canAttack = true;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, targetDetectionRadius);
    }
}

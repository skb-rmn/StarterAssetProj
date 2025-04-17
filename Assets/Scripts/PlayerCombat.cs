using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public enum CombatState { Idle, Attacking, ComboWindow, Cooldown }

public class PlayerCombat : MonoBehaviour
{
    public AttackData basicAttack;
    public Animator animator;
    public PlayerInput input;
    public CinemachineVirtualCamera freeLookCam, lockCam;

    private CombatState state = CombatState.Idle;
    private AttackData currentAttack;
    private float comboTimer;
    private bool locked;
    private Transform target;
    private List<Transform> candidates = new List<Transform>();

    void Update()
    {
        HandleMovement();
        HandleLockOn();        // §3.2
        HandleCombat();        // §2.2
    }

    void HandleMovement()
    {
        // integrate StarterAssets movement here
    }

    void HandleLockOn()
    {
        if (input.actions["LockOn"].triggered)
        {
            locked = !locked;
            if (locked && candidates.Count > 0)
            {
                target = candidates.OrderBy(t => Vector3.Angle(Camera.main.transform.forward, t.position - transform.position)).First();
                lockCam.LookAt = target;
            }
            freeLookCam.gameObject.SetActive(!locked);
            lockCam.gameObject.SetActive(locked);
        }
    }

    void HandleCombat()
    {
        switch (state)
        {
            case CombatState.Idle:
                if (input.actions["Attack"].triggered) StartAttack(basicAttack);
                break;
            case CombatState.Attacking:
                // wait for animator event to call OnAttackComplete()
                break;
            case CombatState.ComboWindow:
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0) state = CombatState.Idle;
                break;
        }
    }

    public void OnAttackComplete()
    {
        state = CombatState.ComboWindow;
        comboTimer = currentAttack.comboWindow;
    }

    void StartAttack(AttackData data)
    {
        currentAttack = data;
        animator.Play(data.clip.name);
        state = CombatState.Attacking;
        // deal damage at animation event
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) candidates.Add(other.transform);
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy")) candidates.Remove(other.transform);
    }
}



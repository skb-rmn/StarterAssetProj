using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBasic : MonoBehaviour, IDamageable
{
    public bool IsHit => isHit;
    bool isHit = false;
    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void TakeDamage(int amount, Vector3 hitPoint)
    {
        //throw new System.NotImplementedException();\
        if(!isHit)
        {
            animator.SetTrigger("Hit");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnHitTakenEvent()
    {
        isHit = true;
    }
    public void OnHitEndEvent()
    {
        isHit =false;
    }
}

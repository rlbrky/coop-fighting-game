using UnityEngine;
using UnityEngine.InputSystem;

public class StrikerSC : CharacterBaseClass
{
    [Header("Balance")] public float skill1StunDuration = 1f;
    
    [Header("Hitboxes")]
    public GameObject skill2_Hitbox;
    public GameObject skill2_2_Hitbox;
    public GameObject skill3_Hitbox;
    public GameObject skill4_Hitbox;
    
    
    protected override void Skill1(InputAction.CallbackContext context)
    {
        if (!normalAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanMove") || !canUse_skill1) return;
        
        //animator.Play(skill1);
        animator.SetTrigger(skill1);
        Collider closestEnemy = null;
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5, whatIsPlayer);
        foreach (Collider collider in colliders)
        {
            if (closestEnemy == null && collider.gameObject != gameObject && collider.CompareTag("Player"))
            {
                closestEnemy = collider;
                continue;
            }
            if (collider.gameObject != gameObject && collider.CompareTag("Player") && Vector3.Distance(collider.transform.position, transform.position) <= Vector3.Distance(closestEnemy.transform.position, transform.position))
            {
                closestEnemy = collider;
            }
            else
                continue;
        }

        if (closestEnemy != null)
        {
            transform.LookAt(closestEnemy.transform.position, Vector3.up);
            CharacterBaseClass enemySC = closestEnemy.GetComponent<CharacterBaseClass>();
            enemySC.TakeDamage(baseDamage * skill1DamageModifier, skill1StunDuration);
        }
        canUse_skill1 = false;
        StartCoroutine(Cooldown(skill1Cooldown, 1));
    }

    protected override void Skill2(InputAction.CallbackContext context)
    {
        if (!normalAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanMove") || !canUse_skill2) return;
        animator.SetTrigger(skill2);
        
        canUse_skill2 = false;
        StartCoroutine(Cooldown(skill2Cooldown, 2));
    }

    protected override void Skill3(InputAction.CallbackContext context)
    {
        if (!normalAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanMove") || !canUse_skill3) return;
        animator.SetTrigger(skill3);
        
        canUse_skill3 = false;
        StartCoroutine(Cooldown(skill3Cooldown, 3));
    }

    protected override void Skill4(InputAction.CallbackContext context)
    {
        if (!normalAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanMove") || !canUse_skill4) return;
        animator.SetTrigger(skill4);
        
        canUse_skill4 = false;
        StartCoroutine(Cooldown(skill4Cooldown, 4));
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 5);
    }
}

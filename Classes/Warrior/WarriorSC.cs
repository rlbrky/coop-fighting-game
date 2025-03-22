using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WarriorSC : CharacterBaseClass
{
    [Header("Balance")] 
    public float pullTimer = 0.5f;
    public float skill2_HealAmount = 20;
    
    [Header("Hitboxes")]
    public GameObject skill1_Hitbox;
    public GameObject skill3_Hitbox;
    public GameObject skill4_Hitbox;
    public float skill4_ProtectionRadius;
    
    protected override void Skill1(InputAction.CallbackContext context)
    {
        if (!normalAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanMove") || !canUse_skill1) return;
        normalAnimator.Play(skill1);
        
        canUse_skill1 = false;
        StartCoroutine(Cooldown(skill1Cooldown, 1));
    }

    protected override void Skill2(InputAction.CallbackContext context)
    {
        if (!normalAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanMove") || !canUse_skill2) return;
        normalAnimator.Play(skill2);
        
        canUse_skill2 = false;
        StartCoroutine(Cooldown(skill2Cooldown, 2));
    }

    protected override void Skill3(InputAction.CallbackContext context)
    {
        if (!normalAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanMove") || !canUse_skill3) return;
        normalAnimator.Play(skill3);
        
        canUse_skill3 = false;
        StartCoroutine(Cooldown(skill3Cooldown, 3));
    }

    protected override void Skill4(InputAction.CallbackContext context)
    {
        if (!normalAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanMove") || !canUse_skill4) return;
        normalAnimator.Play(skill4);
        Collider[] colliders = Physics.OverlapSphere(transform.position, skill4_ProtectionRadius, whatIsPlayer);
        foreach (var collider in colliders)
        {
            CharacterBaseClass character = collider.GetComponent<CharacterBaseClass>();
            if (character.teamIndex == teamIndex)
            {
                character.health += 100;//FIX THIS LATER WHEN VFX AND UI COMES
            }
        }
        
        canUse_skill4 = false;
        StartCoroutine(Cooldown(skill4Cooldown, 4));
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, skill4_ProtectionRadius);
    }

    //GET THIS TO ANOTHER CHARACTER
    IEnumerator PullCoroutine(CharacterBaseClass enemySC, float pullTime)
    {
        float time = 0;
        while (time < pullTime)
        {
            enemySC.transform.position = Vector3.Lerp(enemySC.transform.position, transform.position + transform.forward, time / pullTime);
            time += Time.deltaTime;
            yield return null;
        }

    }
}

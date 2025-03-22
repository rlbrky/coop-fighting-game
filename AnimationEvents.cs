using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimationEvents : MonoBehaviour
{
    public CharacterBaseClass character;
    
    public void Activate_NormalAttack1_Hitbox()
    {
        character.normalAttack1_Hitbox.SetActive(true);
    }

    public void Deactivate_NormalAttack1_Hitbox()
    {
        character.normalAttack1_Hitbox.SetActive(false);
    }
    
    public void Activate_NormalAttack2_Hitbox()
    {
        character.normalAttack2_Hitbox.SetActive(true);
    }

    public void Deactivate_NormalAttack2_Hitbox()
    {
        character.normalAttack2_Hitbox.SetActive(false);
    }

    public void Finish_Attacking()
    {
        character.normalAnimator.SetBool(character.NormalAttack1, false);
    }
}
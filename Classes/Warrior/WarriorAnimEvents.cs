using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorAnimEvents : AnimationEvents
{
    private WarriorSC warrior;

    private void Awake()
    {
        warrior = GetComponent<WarriorSC>();
    }
    
    public void Activate_Skill1_Hitbox()
    {
        warrior.skill1_Hitbox.SetActive(true);
    }

    public void Deactivate_Skill1_Hitbox()
    {
        warrior.skill1_Hitbox.SetActive(false);
    }
    
    public void Activate_Skill3_Hitbox()
    {
        warrior.skill3_Hitbox.SetActive(true);
    }

    public void Deactivate_Skill3_Hitbox()
    {
        warrior.skill3_Hitbox.SetActive(false);
    }
    
    public void Activate_Skill4_Hitbox()
    {
        warrior.skill4_Hitbox.SetActive(true);
    }

    public void Deactivate_Skill4_Hitbox()
    {
        warrior.skill4_Hitbox.SetActive(false);
    }
}
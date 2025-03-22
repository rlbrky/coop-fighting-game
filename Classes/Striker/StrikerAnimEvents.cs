using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikerAnimEvents : AnimationEvents
{
    private StrikerSC striker;

    private void Awake()
    {
        striker = GetComponent<StrikerSC>();
    }

    public void Activate_Skill2_Hitbox()
    {
        striker.skill2_Hitbox.SetActive(true);
    }

    public void Deactivate_Skill2_Hitbox()
    {
        striker.skill2_Hitbox.SetActive(false);
    }
    
    public void Activate_Skill2_2_Hitbox()
    {
        striker.skill2_2_Hitbox.SetActive(true);
    }

    public void Deactivate_Skill2_2_Hitbox()
    {
        striker.skill2_2_Hitbox.SetActive(false);
    }
    
    public void Activate_Skill3_Hitbox()
    {
        striker.skill3_Hitbox.SetActive(true);
    }

    public void Deactivate_Skill3_Hitbox()
    {
        striker.skill3_Hitbox.SetActive(false);
    }
    
    public void Activate_Skill4_Hitbox()
    {
        striker.skill4_Hitbox.SetActive(true);
    }

    public void Deactivate_Skill4_Hitbox()
    {
        striker.skill4_Hitbox.SetActive(false);
    }
}
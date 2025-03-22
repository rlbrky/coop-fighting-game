using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRegister : MonoBehaviour
{
    private CharacterBaseClass _character;

    [Header("Attack Settings")] 
    [Tooltip("Enter the skill number to specify the damage multiplier that will apply when using this skill. \"0\" if it is a normal attack.")] public int skillNo;
    public bool shouldKnock;
    public bool shouldSlow;
    public float stunDuration;
    public float slowDuration;


    private float _damageMultiplier;
    
    private void Awake()
    {
        _character = transform.parent.GetComponent<CharacterBaseClass>();
        
        switch (skillNo)
        {
            case 1:
                _damageMultiplier = _character.skill1DamageModifier;
                break;
            case 2:
                _damageMultiplier = _character.skill2DamageModifier;
                break;
            case 3:
                _damageMultiplier = _character.skill3DamageModifier;
                break;
            case 4:
                _damageMultiplier = _character.skill4DamageModifier;
                break;
            default:
                _damageMultiplier = 1;
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterBaseClass enemy = other.GetComponent<CharacterBaseClass>();
            enemy.transform.forward = -transform.forward;
            enemy.TakeDamage(_character.baseDamage * _damageMultiplier, stunDuration, shouldSlow, slowDuration, shouldKnock);
        }
    }
}
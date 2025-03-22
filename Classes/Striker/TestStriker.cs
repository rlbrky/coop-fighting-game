using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.UI;

public class TestStriker : MonoBehaviour
{
    protected static readonly int Death = Animator.StringToHash("Death");
    protected static readonly int Idle = Animator.StringToHash("Idle");
    
    protected static readonly int IsSlowed = Animator.StringToHash("isSlowed");
    protected static readonly int IsMoving = Animator.StringToHash("isMoving");
    protected static readonly int IsStunned = Animator.StringToHash("Stunned");
    protected static readonly int Knockback = Animator.StringToHash("Knockback");

    //Needed for animation events.
    public readonly int NormalAttack1 = Animator.StringToHash("normal1");
    public readonly int NormalAttack2 = Animator.StringToHash("normal2");
    
    protected static readonly int skill1 = Animator.StringToHash("Skill1");
    protected static readonly int skill2 = Animator.StringToHash("Skill2");
    protected static readonly int skill3 = Animator.StringToHash("Skill3");
    protected static readonly int skill4 = Animator.StringToHash("Skill4");
    
    private static readonly int emote1 = Animator.StringToHash("Emote1");
    private static readonly int emote2 = Animator.StringToHash("Emote2");
    private static readonly int emote3 = Animator.StringToHash("Emote3");
    
    [Header("Camera")]
    public CinemachineVirtualCamera playerCamera;
    
    [Header("Hitboxes")] 
    public GameObject normalAttack1_Hitbox;
    public GameObject normalAttack2_Hitbox;
    
    [Header("General Stats")]
    public float health;
    public float baseDamage;
    public float speed;
    public float turnSpeed;
    public LayerMask whatIsPlayer;

    [Header("UI")]
    public Slider healthSlider;
    
    //Publics that are not shared.
    [HideInInspector]
    public Animator normalAnimator;
    
    [HideInInspector] public Vector3 _worldForward;
    [HideInInspector] public Vector3 _worldRight;

    public int teamIndex;
    
    //Needed things
    protected Rigidbody _rigidbody;
    protected PlayerInputs _playerInputs;
    protected Vector2 _playerInput;
    
    //State
    protected bool _isStunned;

    //Skills
    protected bool canUse_skill1 = true;
    protected bool canUse_skill2 = true;
    protected bool canUse_skill3 = true;
    protected bool canUse_skill4 = true;

    [Header("Balance")]
    public float skill1DamageModifier = 1.5f;
    public float skill1Cooldown = 1f;
    
    public float skill2DamageModifier = 1.5f;
    public float skill2Cooldown = 1f;
    
    public float skill3DamageModifier = 1.5f;
    public float skill3Cooldown = 1f;
    
    public float skill4DamageModifier = 1.5f;
    public float skill4Cooldown = 1f;

    private AttackRegister[] hitboxes;
    private void Awake()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = health;
            healthSlider.value = health;
        }
        _playerInputs = new PlayerInputs();
        _playerInputs.General.Enable();
        _playerInputs.General.BasicAttack.started += HandleAttacking;
        _playerInputs.General.Skill1.started += Skill1;
        _playerInputs.General.Skill2.started += Skill2;
        _playerInputs.General.Skill3.started += Skill3;
        _playerInputs.General.Skill4.started += Skill4;
        _playerInputs.General.Emote1.started += Emote1;
        _playerInputs.General.Emote2.started += Emote2;
        
        normalAnimator = GetComponent<Animator>();
        hitboxes = GetComponentsInChildren<AttackRegister>(true);
        UpdateWorldVectors();
    }

    private void OnDisable()
    {
        _playerInputs.General.Disable();
        //Destroy(_playerInputs);
    }

    public void UpdateWorldVectors()
    {
        _worldForward = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
        _worldRight = new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z);
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (normalAnimator.GetCurrentAnimatorStateInfo(0).IsTag("RemoveStun"))
        {
            _isStunned = false;
            //_normalAnimator.Play(Idle);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            TakeDamage(10);
        }
        if (_isStunned) return;
        HandleMovement();
    }

    private void FixedUpdate()
    {
        if (_isStunned) return;
        //_rigidbody.velocity = normalAnimator.deltaPosition * (speed * Time.deltaTime);
    }

    public void TakeDamage(float damage, float stunDuration = 0, bool shouldSlow = false, float slowDuration = 0, bool getKnockedback = false)
    {
        health -= damage;
        StartCoroutine(UpdateHealthbarAnim());
        
        if (shouldSlow)
            StartCoroutine(SlowCoroutine(slowDuration));
        
        if(stunDuration > 0)
            StartCoroutine(StunCoroutine(stunDuration));

        if (getKnockedback)
        {
            _isStunned = true;
            normalAnimator.SetTrigger(Knockback);
            foreach (var hitbox in hitboxes)
            {
                hitbox.gameObject.SetActive(false);
            }
        }
        
        if (health <= 0)
        {
            StartCoroutine(DeathSequence());
        }
    }

    private void HandleMovement()
    {
        _playerInput = _playerInputs.General.Movement.ReadValue<Vector2>();
        
        if ((_playerInput.x != 0 || _playerInput.y != 0) && normalAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Emote"))
        {
            normalAnimator.Play(Idle);
        }
        
        if (_playerInput.x != 0 || _playerInput.y != 0)
        {
            transform.forward = Vector3.Lerp(transform.forward, ((_worldForward * _playerInput.y) + (_worldRight * _playerInput.x)).normalized, turnSpeed * Time.deltaTime);
            normalAnimator.SetBool(IsMoving, true);
        }
        else
            normalAnimator.SetBool(IsMoving, false);
    }

    private void HandleAttacking(InputAction.CallbackContext context)
    {
        if(_isStunned) return;
        
        if(normalAnimator.GetBool(NormalAttack1))
            normalAnimator.SetTrigger(NormalAttack2);
        //Bool is turned to false in animation events.
        normalAnimator.SetBool(NormalAttack1, true);
    }

    #region Skills

    protected void Skill1(InputAction.CallbackContext context)
    {
        if (!normalAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanMove") || !canUse_skill1) return;
        
        //normalAnimator.Play(skill1);
        normalAnimator.SetTrigger(skill1);
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

    protected void Skill2(InputAction.CallbackContext context)
    {
        if (!normalAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanMove") || !canUse_skill2) return;
        normalAnimator.SetTrigger(skill2);
        
        canUse_skill2 = false;
        StartCoroutine(Cooldown(skill2Cooldown, 2));
    }

    protected void Skill3(InputAction.CallbackContext context)
    {
        if (!normalAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanMove") || !canUse_skill3) return;
        normalAnimator.SetTrigger(skill3);
        
        canUse_skill3 = false;
        StartCoroutine(Cooldown(skill3Cooldown, 3));
    }

    protected void Skill4(InputAction.CallbackContext context)
    {
        if (!normalAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanMove") || !canUse_skill4) return;
        normalAnimator.SetTrigger(skill4);
        
        canUse_skill4 = false;
        StartCoroutine(Cooldown(skill4Cooldown, 4));
    }
    #endregion

    #region Emotes

    private void Emote1(InputAction.CallbackContext context)
    {
        normalAnimator.SetTrigger(emote1);
    }
    
    private void Emote2(InputAction.CallbackContext context)
    {
        normalAnimator.SetTrigger(emote2);
    }
    
    private void Emote3(InputAction.CallbackContext context)
    {
        normalAnimator.SetTrigger(emote3);
    }

    #endregion

    IEnumerator StunCoroutine(float stunTime)
    {
        _isStunned = true;
        normalAnimator.SetTrigger(IsStunned);
        foreach (var hitbox in hitboxes)
        {
            hitbox.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(stunTime);
        normalAnimator.Play(Idle);
        _isStunned = false;
    }
    
    IEnumerator DeathSequence()
    {
        _isStunned = true;
        normalAnimator.SetTrigger(Death);
        yield return new WaitForSeconds(5f);
        _isStunned = false;
        //normalAnimator.Play(Idle);;----------CARE-------------------------
    }

    IEnumerator SlowCoroutine(float slowDuration)
    {
        normalAnimator.SetBool(IsSlowed, true);
        yield return new WaitForSeconds(slowDuration);
        normalAnimator.SetBool(IsSlowed, false);
    }

    protected IEnumerator Cooldown(float cooldownTime, int skillNo)
    {
        yield return new WaitForSeconds(cooldownTime);
        switch (skillNo)
        {
            case 1:
                canUse_skill1 = true;
                break;
            case 2:
                canUse_skill2 = true;
                break;
            case 3:
                canUse_skill3 = true;
                break;
            case 4:
                canUse_skill4 = true;
                break;
        }
    }

    #region UI_Coroutines

    IEnumerator UpdateHealthbarAnim()
    {
        while(healthSlider.value != health)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, health, 1 * Time.deltaTime);
            yield return null;
        }

        yield return null;
    }
    

    #endregion
    
    [Header("Balance")] public float skill1StunDuration = 1f;
    
    [Header("Hitboxes")]
    public GameObject skill2_Hitbox;
    public GameObject skill2_2_Hitbox;
    public GameObject skill3_Hitbox;
    public GameObject skill4_Hitbox;
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 5);
    }
}

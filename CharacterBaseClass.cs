using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.UI;

public abstract class CharacterBaseClass : NetworkBehaviour
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

    private readonly NetworkVariable<float> serverHealth = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
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
    [HideInInspector] public Animator normalAnimator;
    [HideInInspector] public NetworkAnimator animator;
    
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
    
    public override void OnNetworkSpawn()
    {
        //base.OnNetworkSpawn();
        if (IsOwner)
        {
            playerCamera = FindObjectOfType<CinemachineVirtualCamera>();
            playerCamera.Follow = transform;
            switch (teamIndex)
            {
                case 0:
                    playerCamera.transform.eulerAngles = new Vector3(41.7f, -40, 0);
                    playerCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = new Vector3(20, 30, -20);
                    break;
                case 1:
                    playerCamera.transform.eulerAngles = new Vector3(41.7f, -210, 0);
                    playerCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = new Vector3(-20, 30, 20);
                    break;
            }
        }
        else
        {
            GetComponent<CharacterBaseClass>().enabled = false;
        }
    }
    
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
        animator = GetComponent<NetworkAnimator>();
        hitboxes = GetComponentsInChildren<AttackRegister>(true);
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
        if (!IsClient) return;
        
        if (normalAnimator.GetCurrentAnimatorStateInfo(0).IsTag("RemoveStun"))
        {
            _isStunned = false;
            //_animator.Play(Idle);
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
        _rigidbody.velocity = normalAnimator.deltaPosition * (speed * Time.deltaTime);
    }

    public void TakeDamage(float damage, float stunDuration = 0, bool shouldSlow = false, float slowDuration = 0, bool getKnockedback = false)
    {
        health -= damage;
        StartCoroutine(UpdateHealthbarAnim());
        UpdateHealthServerRpc(-damage);
        
        if (shouldSlow)
            StartCoroutine(SlowCoroutine(slowDuration));
        
        if(stunDuration > 0)
            StartCoroutine(StunCoroutine(stunDuration));

        if (getKnockedback)
        {
            _isStunned = true;
            animator.SetTrigger(Knockback);
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
            //animator.Play(Idle);
        }
        
        if (_playerInput.x != 0 || _playerInput.y != 0)
        {
            //This could be a little expensive.
            UpdateWorldVectors();
            transform.forward = Vector3.Lerp(transform.forward, ((_worldForward * _playerInput.y) + (_worldRight * _playerInput.x)).normalized, turnSpeed * Time.deltaTime);
            normalAnimator.SetBool(IsMoving, true);
        }
        else
            normalAnimator.SetBool(IsMoving, false);
    }

    private void HandleAttacking(InputAction.CallbackContext context)
    {
        if(_isStunned || !base.IsOwner) return;
        
        if(normalAnimator.GetBool(NormalAttack1))
            animator.SetTrigger(NormalAttack2);
        //Bool is turned to false in animation events.
        normalAnimator.SetBool(NormalAttack1, true);
    }

    #region Skills
    protected virtual void Skill1(InputAction.CallbackContext context){}
    protected virtual void Skill2(InputAction.CallbackContext context){}
    protected virtual void Skill3(InputAction.CallbackContext context){}
    protected virtual void Skill4(InputAction.CallbackContext context){}
    #endregion

    #region Emotes

    private void Emote1(InputAction.CallbackContext context)
    {
        animator.SetTrigger(emote1);
    }
    
    private void Emote2(InputAction.CallbackContext context)
    {
        animator.SetTrigger(emote2);
    }
    
    private void Emote3(InputAction.CallbackContext context)
    {
        animator.SetTrigger(emote3);
    }

    #endregion

    IEnumerator StunCoroutine(float stunTime)
    {
        _isStunned = true;
        animator.SetTrigger(IsStunned);
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
        animator.SetTrigger(Death);
        yield return new WaitForSeconds(5f);
        _isStunned = false;
        //animator.Play(Idle);;----------CARE-------------------------
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
    
    //Server Updates
    
    [ServerRpc(RequireOwnership = false)]
    public void UpdateHealthServerRpc(float changeAmount)
    {
        serverHealth.Value = health + changeAmount;
        healthSlider.value = serverHealth.Value;
    }
}

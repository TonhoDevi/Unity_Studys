using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    [Header("Movimento")]
    public float walkSpeed = 2f;
    public float runSpeed = 3f;
    public float sprintSpeed = 6f;
    public float rotationSpeed = 10f;

    [Header("Pulo")]
    public float jumpForce = 12f;
    public float fallMultiplier = 2.5f; // Multiplica gravidade quando cai
    public float lowJumpMultiplier = 2f; // Se soltar espaço cedo, cai mais rápido

    [Header("Combate")]
    public float attackCooldown = 0.5f;
    public bool allowInputBuffer = true;
    public float inputBufferTime = 0.2f;

    [Header("Referências")]
    public Transform cameraTransform;
    public Animator animator;

    // Privadas
    private Rigidbody rb;
    private bool isGrounded = false;
    private bool isJumping = false;
    private Vector3 moveDirection;
    private float lastAttackTime = 0f;
    private bool attackBuffered = false;
    private float attackBufferTimer = 0f;
    private bool canMove = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    void Start()
    {
        rb.freezeRotation = true;

        // IMPORTANTE: Desativa a gravidade automática
        // Vamos controlar manualmente para ter gravidade variável
        rb.useGravity = false;
    }

    void Update()
    {
        HandleJumpInput();
        HandleAttackInput();
    }

    void FixedUpdate()
    {
        HandleMovement();
        ApplyCustomGravity();

    }

    void HandleJumpInput()
    {
        // Registra quando apertou espaço 
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            isJumping = true;
            isGrounded = false;
            animator.SetBool("Ground", false);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            animator.SetTrigger("Jump");
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;
        }
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Direção baseada na câmera
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0;
        cameraRight.Normalize();

        moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;

        // ===== MOVIMENTO NO CHÃO =====
        HandleGroundMovement();

        // Atualiza animações
        UpdateAnimations();
    }

    void HandleGroundMovement()
    {
        if (canMove == false) return;
        // Determina velocidade alvo
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        // Ctrl + W para andar devagar e Shift + W para correr rápido
        float targetSpeed = isSprinting ? sprintSpeed : (Input.GetKey(KeyCode.LeftControl) ? walkSpeed : runSpeed);

        if (moveDirection.magnitude > 0.1f)
        {
            // Move com controle total
            Vector3 targetVelocity = moveDirection * targetSpeed;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

            // Rotaciona
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // Parado
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void ApplyCustomGravity()
    {
        // GRAVIDADE VARIÁVEL - O segredo do pulo bom!

        float gravity = Physics.gravity.y; // -9.81 padrão

        if (rb.linearVelocity.y < 0)
        {
            // CAINDO: Aplica gravidade mais forte
            rb.linearVelocity += Vector3.up * gravity * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            // SUBINDO mas soltou espaço: Cai mais rápido (pulo curto)
            rb.linearVelocity += Vector3.up * gravity * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }

        // Aplica gravidade base
        rb.linearVelocity += Vector3.up * gravity * Time.fixedDeltaTime;
    }

    void UpdateAnimations()
    {
        if (animator == null) return;
        if (isGrounded == false) return;

        // Velocidade horizontal para blend tree
        float horizontalSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        float animSpeed = 0f;

        if (horizontalSpeed >= sprintSpeed * 0.8f)
            animSpeed = 5.1f; // Sprint
        else if (horizontalSpeed >= runSpeed * 0.8f)
            animSpeed = 2.2f; // Run
        else if (horizontalSpeed < (runSpeed * 0.8f) && horizontalSpeed > 0.2f)
            animSpeed = 1.2f; // Walk

        float currentAnimSpeed = animator.GetFloat("Speed");
        animator.SetFloat("Speed", Mathf.Lerp(currentAnimSpeed, animSpeed, 10f * Time.fixedDeltaTime));
    }

    void HandleAttackInput()
    {
        if (Input.GetMouseButtonDown(0) && attackBuffered == false) // Botão esquerdo do mouse
        {
            stopMovement();
            float timeSinceLastAttack = Time.time - lastAttackTime;
            if (timeSinceLastAttack >= attackCooldown)
            {
                animator.SetTrigger("AttackState");
                lastAttackTime = Time.time;
            }
            else if (allowInputBuffer)
            {
                attackBuffered = true;
                attackBufferTimer = Time.time;
            }
        }
        if (attackBuffered)
        {
            stopMovement();
            float timeSinceLastAttack = Time.time - lastAttackTime;
            float timeSinceBuffer = Time.time - attackBufferTimer;
            if (timeSinceLastAttack >= attackCooldown)
            {
                animator.SetTrigger("AttackState");
                lastAttackTime = Time.time;
                attackBuffered = false;
            }
            else if (timeSinceBuffer >= inputBufferTime)
            {
                attackBuffered = false;
            }
        }
    }

    // quando a animação de ataque termina, chamada por um evento de animação
    public void OnAttackAnimationEnd()
    {
        canMove = true;
    }

    // ===== DETECÇÃO DE CHÃO =====
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            animator.SetBool("Ground", true);
            isGrounded = true;
        }
    }


    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
    private void stopMovement()
    {
        canMove = false;
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        animator.SetFloat("Speed", 0f);
    }
}
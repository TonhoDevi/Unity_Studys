using UnityEngine;

public class AdvancedJumpSystem : MonoBehaviour
{
    [Header("Movimento no Chão")]
    public float walkSpeed = 2f;
    public float runSpeed = 3f;
    public float sprintSpeed = 6f;
    public float rotationSpeed = 10f;
    
    [Header("Movimento no Ar")]
    public float airControlMultiplier = 0.3f; // 30% do controle normal
    public float airDrag = 0.95f; // Resistência do ar (0-1, menor = mais drag)
    
    [Header("Pulo")]
    public float jumpForce = 12f;
    public float jumpDelay = 0.5f;
    public float fallMultiplier = 2.5f; // Multiplica gravidade quando cai
    public float lowJumpMultiplier = 2f; // Se soltar espaço cedo, cai mais rápido
    
    [Header("Coyote Time & Buffer")]
    public float coyoteTime = 0.15f; // Pode pular após sair da borda
    public float jumpBufferTime = 0.2f; // Apertar espaço antes de pousar
    
    [Header("Referências")]
    public Transform cameraTransform;
    public Animator animator;
    // Privadas
    private Rigidbody rb;
    private bool isGrounded = false;
    private bool isJumping = false;
    private float lastGroundedTime = 0f; // Último momento que estava no chão
    private float lastJumpPressTime = 0f; // Último momento que apertou espaço
    private Vector3 moveDirection;
    
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
    }
    
    void FixedUpdate()
    {
        HandleMovement();
        ApplyCustomGravity();
    }
    
    void HandleJumpInput()
    {
        // Registra quando apertou espaço (jump buffer)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            lastJumpPressTime = Time.time;
        }
        
        // Atualiza tempo desde último chão (coyote time)
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
        }
        
        // Condições para pular:
        // 1. Apertou espaço recentemente (buffer)
        // 2. Estava no chão recentemente (coyote)
        // 3. Não está já pulando
        bool canCoyoteJump = Time.time - lastGroundedTime <= coyoteTime;
        bool hasJumpBuffer = Time.time - lastJumpPressTime <= jumpBufferTime;
        
        if (hasJumpBuffer && canCoyoteJump && !isJumping)
        {
            StartCoroutine(JumpWithDelay());
            lastJumpPressTime = 0f; // Reseta buffer
        }
    }
    
    System.Collections.IEnumerator JumpWithDelay()
    {
        isJumping = true;
        
        if (animator != null)
        {
            animator.SetTrigger("Jump");
        }
        
        // Espera o delay
        yield return new WaitForSeconds(jumpDelay);
        
        // Executa o pulo
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        
        // Pode pular novamente após um tempo
        yield return new WaitForSeconds(0.3f);
        isJumping = false;
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
    
    // ===== DETECÇÃO DE CHÃO =====
    
    void OnCollisionEnter(Collision collision)
    {
        if (IsGroundSurface(collision))
        {
            isGrounded = true;
        }
    }
    
    void OnCollisionStay(Collision collision)
    {
        if (IsGroundSurface(collision))
        {
            isGrounded = true;
        }
    }
    
    void OnCollisionExit(Collision collision)
    {
        if (IsGroundSurface(collision))
        {
            isGrounded = false;
        }
    }
    
    bool IsGroundSurface(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Angle(contact.normal, Vector3.up) < 45f)
            {
                return true;
            }
        }
        return false;
    }
    
    // Visualização de debug
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // Desenha vetor de velocidade
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, rb.linearVelocity);
        
        // Desenha controle de ar
        if (!isGrounded)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
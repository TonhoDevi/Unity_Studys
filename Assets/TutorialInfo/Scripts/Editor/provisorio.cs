using UnityEngine;

public class PlayerWithRunningAnimation : MonoBehaviour
{
    [Header("Movimento")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float jumpForce = 7f;
    public float rotationSpeed = 10f;
    
    [Header("Referências")]
    public Transform cameraTransform;
    public Animator animator; // ← NOVO!
    
    private Rigidbody rb;
    private bool isGrounded = false;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Tenta pegar o Animator automaticamente se não foi definido manualmente
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }
    
    void Start()
    {
        rb.freezeRotation = true;
        
        // Verifica se o Animator foi encontrado
        if (animator == null)
        {
            Debug.LogError("FALTA ANIMATOR! Adicione um componente Animator ao Player.");
        }
    }
    
    void Update()
    {
        // Pulo
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }
    }
    
    void FixedUpdate()
    {
        HandleMovement();
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
        
        Vector3 moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;
        
        // ===== CONTROLE DE VELOCIDADE E ANIMAÇÃO =====
        
        float currentSpeed = 0f;
        
        if (moveDirection.magnitude > 0.1f)
        {
            // Está se movendo
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float targetSpeed = isRunning ? runSpeed : walkSpeed;
            
            // Aplica movimento
            Vector3 targetVelocity = moveDirection * targetSpeed;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
            
            // Rotaciona na direção do movimento
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            
            // Define velocidade para animação
            // Se andando normal: Speed = 0.5
            // Se correndo: Speed = 1
            currentSpeed = isRunning ? 1f : 0.5f;
        }
        else
        {
            // Parado
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            currentSpeed = 0f;
        }
        
        // ===== ATUALIZA O ANIMATOR =====
        
        if (animator != null)
        {
            // Suaviza a transição do valor de Speed
            float currentAnimSpeed = animator.GetFloat("Speed");
            float smoothSpeed = Mathf.Lerp(currentAnimSpeed, currentSpeed, 10f * Time.fixedDeltaTime);
            
            animator.SetFloat("Speed", smoothSpeed);
        }
    }
    
    // ===== DETECÇÃO DE CHÃO =====
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || IsGroundSurface(collision))
        {
            isGrounded = true;
        }
    }
    
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || IsGroundSurface(collision))
        {
            isGrounded = true;
        }
    }
    
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || IsGroundSurface(collision))
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
}
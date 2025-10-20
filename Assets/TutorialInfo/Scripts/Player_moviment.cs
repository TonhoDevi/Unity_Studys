using UnityEngine;

public class PlayerMovementFixed : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 7f;
    
    [Header("Rotação")]
    public Transform cameraTransform;
    public float rotationSpeed = 10f;

    [Header("Animação")]
    public Animator animator;

    public float animationSpeed = 0f;

    private Rigidbody rb;
    private bool isGrounded = false;
    private int groundContactCount = 0; // Conta quantos objetos de chão estão tocando
    
    void Awake()
    {   

        rb = GetComponent<Rigidbody>();
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
    }
    
    void Update()
    {
        // Pulo
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            isGrounded = false;
            // ativa o trigger Jump
            animator.SetTrigger("Jump");
        }
    }
    
    void FixedUpdate()
    {
        // Input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // Direção baseada na câmera (sem componente Y)
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        
        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0;
        cameraRight.Normalize();
        
        // Calcula direção do movimento
        Vector3 moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;



        // Rotaciona o player na direção do movimento
        if (moveDirection.magnitude > 0.2f)
        {
            // Velocidade (shift para correr)
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float currentSpeed = isRunning ? runSpeed : moveSpeed;

            // Aplica movimento
            Vector3 targetVelocity = moveDirection * currentSpeed;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
            // Rotaciona na direção do movimento
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            if(rb.linearVelocity.magnitude > 0.8f)
            {
                animationSpeed = isRunning ? 5f : 2f;
            } else
            {
                animationSpeed = 1f;
            }
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            animationSpeed = 0f;
        }
        if (animator != null)
        {
            // Suaviza a transição do valor de Speed
            float currentAnimSpeed = animator.GetFloat("Speed");
            float smoothSpeed = Mathf.Lerp(currentAnimSpeed, animationSpeed, 10f * Time.fixedDeltaTime);
            
            animator.SetFloat("Speed", smoothSpeed);
        }
        
    }
    
    // ===== DETECÇÃO DE CHÃO CONFIÁVEL =====
    
    void OnCollisionEnter(Collision collision)
    {
        // Verifica se é chão pela tag OU pela normal da superfície
        if (collision.gameObject.CompareTag("Ground") || IsGroundSurface(collision))
        {

            animator.SetTrigger("Land");
            groundContactCount++;
            isGrounded = true;

        }
    }

    void OnCollisionExit(Collision collision)
    {
       // if (collision.gameObject.CompareTag("Ground") || IsGroundSurface(collision))
     //   {
     //       groundContactCount--;
      //      if (groundContactCount <= 0)
        //    {
      //          groundContactCount = 0;
      //          isGrounded = false;
     //       }
       // }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || IsGroundSurface(collision))
        {
            animator.SetTrigger("Land");
            isGrounded = true;
        }
    }

    // Verifica se a superfície é "chão" baseado na inclinação
    bool IsGroundSurface(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // Se a normal aponta para cima (ângulo com Vector3.up menor que 45 graus)
            if (Vector3.Angle(contact.normal, Vector3.up) < 45f)
            {
                return true;
            }
        }
        return false;
    }
}
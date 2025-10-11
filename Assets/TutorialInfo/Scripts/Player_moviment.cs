using Unity.Mathematics;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.AI;

public class Player_moviment : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float rotationSpeed = 10f;

    private Rigidbody rb;
    private bool isGround = true;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {   
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) == true && isGround == true)
        {
            jump();
        }
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(horizontal, 0f, vertical);
        if (movement.magnitude > 1f)
        {
            movement.Normalize();
        }
        Vector3 newVelociy = movement * moveSpeed;
        newVelociy.y = rb.linearVelocity.y;
        rb.linearVelocity = newVelociy;

        if(movement.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }


    void jump()
    {   
        
        isGround = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        
        isGround = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {   
            isGround = true;
        }
    }

}

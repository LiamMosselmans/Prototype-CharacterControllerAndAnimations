using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCC_Movement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float sprintSpeed;
    public float walkSpeed;
    // public float wallRunSpeed;
    // public float groundDrag;

    [Space(10)]
    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public bool canJump = true;

    [Space(10)]
    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public LayerMask whatIsLedge;
    public bool isGrounded;

    [Space(10)]
    public Transform orientation;
    private Vector3 moveDirection;
    private Vector3 velocity;
    public Transform playerModel;

    private CharacterController controller;
    private PlayerCC_Controller playerController;
    public float gravity = -9.81f;
    private bool isJumping;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        if (!controller)
        {
            Debug.LogError("CharacterController component is missing!");
        }

        playerController = GetComponent<PlayerCC_Controller>();
    }

    private void Update()
    {
        GroundedCheck();
        RotatePlayerModel();
    }

    public void MovePlayer(float horizontalInput, float verticalInput, bool isHanging)
    {
        if(isHanging)
        {
            velocity = Vector3.zero;
        }

        if(playerController.playerState == PlayerCC_Controller.PlayerState.hanging)
        {
            moveDirection = Vector3.zero;
            velocity = Vector3.zero;
            return;
        }
        
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        moveDirection.Normalize();  

        float currentSpeed = isGrounded ? moveSpeed : moveSpeed * airMultiplier;

        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    private void RotatePlayerModel()
    {
        playerModel.rotation = Quaternion.Euler(0, orientation.eulerAngles.y, 0);
    }

    private void GroundedCheck()
    {
        isGrounded = Physics.CheckSphere(transform.position - Vector3.up * (playerHeight / 2), 0.2f, whatIsGround)
                     || Physics.CheckSphere(transform.position - Vector3.up * (playerHeight / 2), 0.2f, whatIsLedge);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    public void Jump()
    {
        Debug.Log("Jump initiated!");
        if (canJump && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            canJump = false;
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    public void ResetJump()
    {
        canJump = true;
    }

    void OnDrawGizmos()
    {
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawSphere(transform.position - Vector3.up * (playerHeight / 2), 0.2f);

        // Debug.DrawRay(transform.position, orientation.forward,Color.blue);
    }
}

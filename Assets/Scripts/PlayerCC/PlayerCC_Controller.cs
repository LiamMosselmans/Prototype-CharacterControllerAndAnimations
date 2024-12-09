using System.Collections;
using UnityEngine;

public class PlayerCC_Controller : MonoBehaviour
{
    private PlayerCC_Movement playerMovement;
    private PlayerCC_Input playerInput;
    private PlayerWallRun playerWallRun;
    private PlayerCC_Vault playerVault;
    private PlayerCC_LedgeGrab playerLedgeGrab;
    private CharacterController controller;
    private PlayerCamera playerCamera;
    private PlayerAnimationHandler playerAnimationHandler;
    public PlayerState playerState;
    
    public enum PlayerState
    {
        idle,
        walking,
        sprinting,
        jumping,
        wallrunning,
        vaulting,
        hanging,
        air
    }
    
    private bool isJumping;
    private float groundedBufferTimer = 0f;
    private bool _vaulting;
    public bool isVaulting
    {
        get { return _vaulting; }
        set
        {
            // Debug.Log("vaulting set to: " + value);
            _vaulting = value;
        }
    }
    // private bool isWallrunning;
    private bool isHanging;
    public bool isPerformingAction = false;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerCamera>();
        playerMovement = GetComponent<PlayerCC_Movement>();
        playerInput = GetComponent<PlayerCC_Input>();
        playerVault = GetComponent<PlayerCC_Vault>();
        playerAnimationHandler = GetComponent<PlayerAnimationHandler>();
        playerLedgeGrab = GetComponent<PlayerCC_LedgeGrab>();
        // playerWallRun = GetComponent<PlayerWallRun>();

        playerVault.OnVaultEnd += StopPerformingAction;
        playerLedgeGrab.OnPullUpEnd += StopPerformingAction;
        // playerWallRun.OnWallRunEnd += StopPerformingAction;
    }

    void Update()
    {    
        // isWallrunning = playerWallRun.WallrunningStateMachine(playerInput.VerticalInput(), isWallrunning);
        isVaulting = playerVault.VaultingStateMachine(playerInput.VerticalInput(), playerInput.IsJumping(), playerMovement.isGrounded);
        isHanging = playerLedgeGrab.HangingStateValue();
        
        Debug.Log("The player is in the state: " + playerState + " and is currently performing an action: " + isPerformingAction);

        if (!isPerformingAction)
        {
            StateHandler();
        }
        else if (playerState == PlayerState.vaulting)
        {
            playerVault.VaultingAction();
        }
        else if (playerState == PlayerState.hanging)
        {
            playerMovement.gravity = 0;
            if(Input.GetKeyDown(playerInput.dropDownKey))
            {
                PlayerDropDown();
            }
            if(Input.GetKeyDown(playerInput.jumpKey))
            {
                playerCamera.isCameraInputEnabled = false;
                playerLedgeGrab.StartPlayerPullUp();
                StartCoroutine(ResetHangAfterCooldown());
            }
        }
        // else if (playerState == PlayerState.wallrunning)
        // {
        //     if(playerInput.IsJumping() && playerMovement.canJump)
        //     {
        //         Debug.Log("Attempting wall jump.");
        //         PlayerJump();
        //     }
        // }
        
    }

    private void FixedUpdate()
    {
        if(playerState != PlayerState.hanging)
        {
            playerMovement.MovePlayer(playerInput.HorizontalInput(), playerInput.VerticalInput(), isHanging);
        }

        // if (isWallrunning)
        // {
        //     playerWallRun.WallrunningMovement(playerInput.HorizontalInput());
        // }
    }

    private void PlayerJump()
    {
        playerMovement.Jump();
        StartCoroutine(ResetJumpAfterCooldown());
    }

    IEnumerator ResetJumpAfterCooldown()
    {
        yield return new WaitForSeconds(playerMovement.jumpCooldown);
        playerMovement.ResetJump();
    }

    private void ResetJumpState()
    {
        if (isJumping)
        {
            isJumping = false;
        }
    }

    private void PlayerDropDown()
    {
        isPerformingAction = false;
        playerLedgeGrab.isHanging = false;
        playerLedgeGrab.canHang = false;
        playerInput.isInputEnabled = true;
        StartCoroutine(ResetHangAfterCooldown());
    }

    IEnumerator ResetHangAfterCooldown()
    {
        yield return new WaitForSeconds(playerLedgeGrab.hangCooldown);
        playerLedgeGrab.ResetHang();
    }

    private void StateHandler()
    {
        if(isHanging)
        {
            ChangeState(PlayerState.hanging);
            playerInput.isInputEnabled = false;
            isPerformingAction = true;

            return;
        }

        if (isJumping)
        {
            ChangeState(PlayerState.jumping);
            if (playerMovement.isGrounded)
            {
                if (groundedBufferTimer > 0f)
                {
                    groundedBufferTimer -= Time.deltaTime;
                }
                else 
                {
                    ResetJumpState();
                }
            }

            return;
        }

        if (isVaulting)
        {
            ChangeState(PlayerState.vaulting);
            isPerformingAction = true;
            playerCamera.isCameraInputEnabled = false;
        }
        // else if (isWallrunning)
        // {
        //     rb.useGravity = false;
        //     playerState = PlayerState.wallrunning;
        //     playerMovement.moveSpeed = playerMovement.wallRunSpeed;
        //     isPerformingAction = true;
        //     playerWallRun.wallRunTimer = 0;
        // }
        else if (playerInput.IsJumping() && !isJumping && playerMovement.isGrounded)
        {
            isJumping = true;
            groundedBufferTimer = playerMovement.jumpCooldown;
            ChangeState(PlayerState.jumping);
            PlayerJump();
        }
        else if(playerInput.HorizontalInput() > 0 || playerInput.VerticalInput() > 0 && playerMovement.isGrounded)
        {
            if (playerMovement.isGrounded && Input.GetKey(playerInput.sprintKey))
            {
                ChangeState(PlayerState.sprinting);
                playerMovement.moveSpeed = playerMovement.sprintSpeed;
            }
            else
            {
                ChangeState(PlayerState.walking);
                playerMovement.moveSpeed = playerMovement.walkSpeed;
            }
        }
        else if (playerMovement.isGrounded)
        {
            ChangeState(PlayerState.idle);
        }
        else 
        {
            ChangeState(PlayerState.air);
        }
    }

    private void ChangeState(PlayerState newState)
    {
        if (playerState != newState)
        {
            playerState = newState;
            playerAnimationHandler.UpdateAnimator(playerState);
        }
    }

    private void StopPerformingAction()
    {
        playerMovement.gravity = -9.81f;
        isJumping = false;
        isPerformingAction = false;
        playerCamera.isCameraInputEnabled = true;
        playerInput.isInputEnabled = true;
    }
}

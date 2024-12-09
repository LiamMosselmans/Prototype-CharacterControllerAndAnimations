using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerInput playerInput;
    private PlayerWallRun playerWallRun;
    private PlayerVault playerVault;
    private PlayerLedgeGrab playerLedgeGrab;
    private PlayerCamera playerCamera;
    public PlayerState playerState;
    
    public enum PlayerState
    {
        walking,
        sprinting,
        jumping,
        wallrunning,
        vaulting,
        hanging,
        air
    }
    
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
    private bool isWallrunning;
    private bool isHanging;
    public bool isPerformingAction = false;
    Rigidbody rb;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerInput = GetComponent<PlayerInput>();
        playerWallRun = GetComponent<PlayerWallRun>();
        playerVault = GetComponent<PlayerVault>();
        playerLedgeGrab = GetComponent<PlayerLedgeGrab>();
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerCamera>();
        rb = GetComponent<Rigidbody>();

        playerVault.OnVaultEnd += StopPerformingAction;
        playerLedgeGrab.OnPullUpEnd += StopPerformingAction;
        playerWallRun.OnWallRunEnd += StopPerformingAction;
    }

    void Update()
    {
        playerMovement.GroundedCheck();
        playerMovement.SpeedControl();
        playerMovement.AddDragForce();        
        isWallrunning = playerWallRun.WallrunningStateMachine(playerInput.VerticalInput(), isWallrunning);
        isVaulting = playerVault.VaultingStateMachine(playerInput.VerticalInput(), playerInput.IsJumping(), playerMovement.isGrounded);
        isHanging = playerLedgeGrab.HangingStateValue();

        // Debug.Log("The player is in the state: " + playerState + " and is currently performing an action: " + isPerformingAction);

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
        else if (playerState == PlayerState.wallrunning)
        {
            if(playerInput.IsJumping() && playerMovement.canJump)
            {
                Debug.Log("Attempting wall jump.");
                PlayerJump();
            }
        }
    }

    private void FixedUpdate()
    {
        playerMovement.MovePlayer(playerInput.HorizontalInput(), playerInput.VerticalInput());

        if (isWallrunning)
        {
            playerWallRun.WallrunningMovement(playerInput.HorizontalInput());
        }
    }

    private void PlayerJump()
    {
        playerMovement.canJump = false;
        playerMovement.Jump();
        StartCoroutine(ResetJumpAfterCooldown());
    }

    IEnumerator ResetJumpAfterCooldown()
    {
        yield return new WaitForSeconds(playerMovement.jumpCooldown);
        playerMovement.ResetJump();
    }

    private void PlayerDropDown()
    {
        playerState = PlayerState.air;
        isPerformingAction = false;
        playerLedgeGrab.isHanging = false;
        playerLedgeGrab.canHang = false;
        rb.useGravity = true;
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
        if (isVaulting)
        {
            playerState = PlayerState.vaulting;
            isPerformingAction = true;
            playerCamera.isCameraInputEnabled = false;
        }
        else if(isHanging)
        {
            playerState = PlayerState.hanging;
            isPerformingAction = true;
            rb.useGravity = false;
            playerInput.isInputEnabled = false;
        }
        else if (isWallrunning)
        {
            rb.useGravity = false;
            playerState = PlayerState.wallrunning;
            playerMovement.moveSpeed = playerMovement.wallRunSpeed;
            isPerformingAction = true;
            playerWallRun.wallRunTimer = 0;
        }
        else if (playerInput.IsJumping() && playerMovement.canJump && playerMovement.isGrounded)
        {
            playerState = PlayerState.jumping;
            PlayerJump();
        }
        else if (playerMovement.isGrounded && Input.GetKey(playerInput.sprintKey))
        {
            playerState = PlayerState.sprinting;
            playerMovement.moveSpeed = playerMovement.sprintSpeed;
        }
        else if (playerMovement.isGrounded) 
        {
            playerState = PlayerState.walking;
            playerMovement.moveSpeed = playerMovement.walkSpeed;
        }
        else
        {
            playerState = PlayerState.air;
        }
    }

    private void StopPerformingAction()
    {
        isPerformingAction = false;
        playerCamera.isCameraInputEnabled = true;
        rb.useGravity = true;
        playerInput.isInputEnabled = true;
    }
}

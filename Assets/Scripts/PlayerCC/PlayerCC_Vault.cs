using System;
using UnityEngine;

public class PlayerCC_Vault : MonoBehaviour
{
    [Header("Vaulting")]
    public LayerMask whatIsObstacle;
    private float vaultTimer;
    private float vaultDuration = 1f; 
    private Vector3 vaultStartPos;
    private Vector3 vaultEndPos;
    private bool isVaulting = false;
    private float halfVaultDuration;
    public event Action OnVaultEnd;

    [Space(10)]
    [Header("Detection")]
    public float obstacleCheckDistance;
    public float vaultHeight;
    private RaycastHit obstacleHit;
    private bool obstacleFront;

    [Space(10)]
    [Header("References")]
    public Transform orientation;
    private CharacterController characterController;
    private Camera playerCamera;
    private Collider obstacleCollider;

    [Space(10)]
    [Header("Camera")]
    public float cameraTiltAmount = 10f;
    private Quaternion initialCameraRotation;
    private Quaternion targetTiltRotation;
    private float cameraTiltTimer;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    void Update()
    {
        CheckForObstacle();

        if (isVaulting)
        {
            vaultTimer += Time.deltaTime;
            cameraTiltTimer += Time.deltaTime;

            if (vaultTimer <= halfVaultDuration)
            {
                // First half of the vault: tilt to maximum rotation
                float t = cameraTiltTimer / (halfVaultDuration / 2);
                playerCamera.transform.localRotation = Quaternion.Slerp(initialCameraRotation, targetTiltRotation, t);
            }
            else if (vaultTimer <= vaultDuration)
            {
                // Second half of the vault: tilt back to initial rotation
                float t = (cameraTiltTimer - (halfVaultDuration / 2)) / (halfVaultDuration / 2);
                playerCamera.transform.localRotation = Quaternion.Slerp(targetTiltRotation, initialCameraRotation, t);
            }
        }
    }

    void CheckForObstacle()
    {
        obstacleFront = Physics.Raycast(transform.position, orientation.forward, out obstacleHit, obstacleCheckDistance, whatIsObstacle);

        // Color rayColor = obstacleFront ? Color.red : Color.green;
        // Debug.DrawRay(transform.position, orientation.forward * obstacleCheckDistance, rayColor);
    }

    public bool VaultingStateMachine(float verticalInput, bool isJumping, bool isGrounded)
    {
        if (isJumping && obstacleFront && verticalInput > 0 && isGrounded)
        {
            if (!isVaulting)
            {
                StartVault();
            }
        }
        return isVaulting;
    }

    public void StartVault()
    {
        isVaulting = true;
        vaultStartPos = transform.position;
        Vector3 behindObstacle = obstacleHit.point - obstacleHit.normal * 3f;
        vaultEndPos = behindObstacle;

        // Disable the obstacle's collider to allow for the player to move to the intended end position = behindObstacle
        obstacleCollider = obstacleHit.collider;
        if (obstacleCollider != null)
        {
            obstacleCollider.enabled = false;
        }

        vaultTimer = 0;
        cameraTiltTimer = 0;

        initialCameraRotation = playerCamera.transform.localRotation;
        targetTiltRotation = initialCameraRotation * Quaternion.Euler(0, 0, cameraTiltAmount);
        halfVaultDuration = vaultDuration / 2;
    }

    public void VaultingAction()
    {
        vaultTimer += Time.deltaTime / vaultDuration;

        // Calculate movement between vault positions
        Vector3 nextPosition = Vector3.Lerp(vaultStartPos, vaultEndPos, vaultTimer);
        Vector3 movement = nextPosition - transform.position;
        characterController.Move(movement);

        if (vaultTimer >= 1)
        {
            StopVault();
        }
    }

    public void StopVault()
    {
        isVaulting = false;

        // Reset camera rotation to ensure it's back to original rotation after vault
        playerCamera.transform.localRotation = initialCameraRotation;

        // Re-enable the obstacle's collider
        if (obstacleCollider != null)
        {
            obstacleCollider.enabled = true;
            obstacleCollider = null;
        }

        // Trigger event when vaulting ends
        OnVaultEnd?.Invoke();
    }
}
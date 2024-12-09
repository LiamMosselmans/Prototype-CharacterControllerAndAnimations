using System;
using UnityEngine;

public class PlayerVault : MonoBehaviour
{
    [Header("Vaulting")]
    public LayerMask whatIsObstacle;
    private float vaultTimer;
    private float vaultDuration = 1f; 
    private Vector3 vaultStartPos;
    private Vector3 vaultEndPos;
    private bool isVaulting = false;
    private float halfVaultDuration;
    
    // Event to notify when vaulting ends
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
    private Rigidbody rb;
    private Animator animator;
    private Camera camera;

    [Space(10)]
    [Header("Camera")]
    public float cameraTiltAmount = 10f;
    private Quaternion initialCameraRotation;
    private Quaternion targetTiltRotation;
    private float cameraTiltTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    void Update()
    {
        CheckForObstacle();

        // Camera tilt while vaulting
        if (isVaulting)
        {
            vaultTimer += Time.deltaTime;

            // Manage camera tilt separately
            cameraTiltTimer += Time.deltaTime;

            if (vaultTimer <= halfVaultDuration)
            {
                // First half of the vault: tilt to maximum rotation
                float t = cameraTiltTimer / (halfVaultDuration / 2); // Normalize for tilt
                camera.transform.localRotation = Quaternion.Slerp(initialCameraRotation, targetTiltRotation, t);
            }
            else if (vaultTimer <= vaultDuration)
            {
                // Second half of the vault: tilt back to initial rotation
                float t = (cameraTiltTimer - (halfVaultDuration / 2)) / (halfVaultDuration / 2); // Normalize for tilt
                camera.transform.localRotation = Quaternion.Slerp(targetTiltRotation, initialCameraRotation, t);
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
        vaultTimer = 0;
        cameraTiltTimer = 0;

        initialCameraRotation = camera.transform.localRotation;
        targetTiltRotation = initialCameraRotation * Quaternion.Euler(0, 0, cameraTiltAmount);
        halfVaultDuration = vaultDuration / 2;

        if (animator != null)
        {
            animator.Play("PlayerVaultAnimation");
        }
    }

    public void VaultingAction()
    {
        vaultTimer += Time.deltaTime / vaultDuration;
        transform.position = Vector3.Lerp(vaultStartPos, vaultEndPos, vaultTimer);

        if (vaultTimer >= 1)
        {
            StopVault();
        }
    }

    public void StopVault()
    {
        isVaulting = false;
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        // Reset camera rotation to ensure it's back to original rotation after vault
        camera.transform.localRotation = initialCameraRotation;

        // Trigger event when vaulting ends
        OnVaultEnd?.Invoke();
    }
}

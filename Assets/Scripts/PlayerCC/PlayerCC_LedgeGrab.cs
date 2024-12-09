using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerCC_LedgeGrab : MonoBehaviour
{
    [Header("Ledge Hanging")]
    [SerializeField]
    private LayerMask whatIsLedge;
    private Vector3 pullUpStartPos;
    private Vector3 pullUpEndPos;
    private float pullUpTimer;
    public float pullUpDuration = 0.8f; 
    public bool isHanging = false;
    public bool canHang = true;
    public float hangCooldown;
    private Vector3 snappingPoint;
    private Vector3 aboveLedge;

    // Event to notify when pull up action ends
    public event Action OnPullUpEnd;
    

    [Header("Detection")]
    private RaycastHit verticalLedgeHit;
    private bool ledgeTop;
    [SerializeField]
    private float verticalRayDistance;
    private Vector3 verticalRayOffset;
    private Vector3 verticalRayOrigin;
    [SerializeField]
    private float verticalOffsetPosX;
    [SerializeField]
    private float verticalOffsetPosY;
    [SerializeField]
    private float verticalOffsetPosZ;
    private RaycastHit horizontalLedgeHit;
    private bool ledgeFront;
    [SerializeField]
    private float horizontalRayDistance;

    [Space(10)]
    [Header("References")]
    [SerializeField]
    private Transform orientation;
    private PlayerCC_Movement playerMovement;

    void Start()
    {
        playerMovement = GetComponent<PlayerCC_Movement>();
    }

    void Update()
    {
        if(canHang)
        {  
            CheckForLedge();
            HangingStateMachine();
        }
    }

    void ShootVerticalRay()
    {
        verticalRayOffset = (orientation.right * verticalOffsetPosX) + (orientation.up * verticalOffsetPosY) + (orientation.forward * verticalOffsetPosZ);
        verticalRayOrigin = transform.position + verticalRayOffset;
        ledgeTop = Physics.Raycast(verticalRayOrigin, Vector3.down, out verticalLedgeHit, verticalRayDistance, whatIsLedge);
        Color verticalRayColor = ledgeTop ? Color.red : Color.green;
        Debug.DrawRay(verticalRayOrigin, Vector3.down * verticalRayDistance, verticalRayColor);
    }

    void ShootHorizontalRay()
    {
        ledgeFront = Physics.Raycast(transform.position, orientation.forward, out horizontalLedgeHit, horizontalRayDistance, whatIsLedge);
        Color horizontalRayColor = ledgeFront ? Color.red : Color.green;
        Debug.DrawRay(transform.position + (orientation.up * 2), orientation.forward * horizontalRayDistance, horizontalRayColor);
    }

    void CheckForLedge()
    {
        ShootVerticalRay();
        if(ledgeTop)
        {
            ShootHorizontalRay();
        } 
    }

    public bool HangingStateValue()
    {
        return isHanging;
    }

    public void HangingStateMachine()
    {
        if(!isHanging)
        {
            if(ledgeFront && ledgeTop)
            {
            snappingPoint = new Vector3(horizontalLedgeHit.point.x, verticalLedgeHit.point.y ,horizontalLedgeHit.point.z);     
            SnapPlayerToLedge();    
            }
        }
    }

    void SnapPlayerToLedge()
    {
        canHang = false;
        isHanging = true;
        // Step 1: Get the direction the player should face (opposite of the ledge normal)
        Vector3 facingDirection = -horizontalLedgeHit.normal;

        // Step 2: Calculate the rotation based on the facing direction
        Quaternion targetRotation = Quaternion.LookRotation(facingDirection, Vector3.up);

        // Step 3: Smoothly rotate the player towards the target rotation (optional, for smoothness)
        transform.rotation = targetRotation;
        
        // Calculate initial snapping point based on raycast info
        Vector3 awayFromLedge = (transform.position - snappingPoint).normalized;

        float backwardOffset = 0.45f; 
        float downwardOffset = 2.5f;

        Vector3 finalDestination = snappingPoint + new Vector3(awayFromLedge.x * backwardOffset, -downwardOffset, awayFromLedge.z * backwardOffset);

        StartCoroutine(SmoothSnapToLedge(finalDestination));
    }

    IEnumerator SmoothSnapToLedge(Vector3 targetPosition)
    {
        float duration = 0.2f; // Adjust duration for smoothness
        float elapsedTime = 0f;

        Vector3 startingPosition = transform.position;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Smoothly interpolate the position
            transform.position = Vector3.Lerp(startingPosition, targetPosition, t);

            yield return null; // Wait for the next frame
        }

        // Ensure the final position is exactly the target position
        transform.position = targetPosition;
    }

    public void ResetHang()
    {
        canHang = true;
    }

    public void StartPlayerPullUp()
    {
        float verticalOffset = 0.3f;
        float depthOffset = 1.2f;
        isHanging = false;
        pullUpStartPos = transform.position;
        Vector3 pointAwayFromLedge = horizontalLedgeHit.point + transform.forward * depthOffset;
        aboveLedge = verticalLedgeHit.point + verticalLedgeHit.normal.normalized * verticalOffset;
        pullUpEndPos = new Vector3(pointAwayFromLedge.x, aboveLedge.y, pointAwayFromLedge.z);
        StartCoroutine(PlayerPullUpAction());
    }

    IEnumerator PlayerPullUpAction()
    {
        pullUpTimer = 0;
        while (pullUpTimer < pullUpDuration)
        {
            pullUpTimer += Time.deltaTime / pullUpDuration;
            transform.position = Vector3.Slerp(pullUpStartPos, pullUpEndPos, pullUpTimer);

            yield return null; // Wait for the next frame
        }

        StopPullUp();
    }

    void StopPullUp()
    {
        OnPullUpEnd?.Invoke();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (snappingPoint != Vector3.zero)
        {
            Gizmos.DrawSphere(snappingPoint, 0.1f);
        }   
        // if (pullUpStartPos != Vector3.zero)
        // {
        //     Gizmos.DrawSphere(pullUpStartPos, 0.1f);
        // }   
        // if (pullUpEndPos != Vector3.zero)
        // {
        //     Gizmos.DrawSphere(pullUpEndPos, 0.1f);
        // }   
        // Debug.DrawRay(transform.position, transform.forward, Color.blue);
        // Debug.DrawRay(verticalRayOrigin, Vector3.down * verticalRayDistance, Color.green);
        // Debug.DrawRay(transform.position, orientation.forward * horizontalRayDistance, Color.yellow);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pullUpEndPos, 0.1f);
    }
}

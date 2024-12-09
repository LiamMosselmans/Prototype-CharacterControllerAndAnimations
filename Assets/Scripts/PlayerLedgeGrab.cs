using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerLedgeGrab : MonoBehaviour
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
    private Rigidbody rb;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // TODO:  #1 Check for ledge method [ COMPLETE ]
        // TODO:  #2 Change player state dynamically based on raycasts [ COMPLETE ]
        // TODO:  #3 Calculate snapping point & snap player to snapping point [ COMPLETE ]
        // TODO:  #4 Disable gravity & player inputs [ COMPLETE ]
        // TODO:  #5 Create ledge hang animation
        // TODO:  #6 Create dropdown mechanic [ COMPLETE ]
        // TODO:  #7 Create pullup mechanic [ COMPLETE ]
        // TODO:  #8 Create pullup animation  
        if(canHang)
        {  
            CheckForLedge();
            HangingStateMachine();
        }
    }

    void ShootVerticalRay()
    {
        // Vertical raycast
        verticalRayOffset = (orientation.right * verticalOffsetPosX) + (orientation.up * verticalOffsetPosY) + (orientation.forward * verticalOffsetPosZ);
        verticalRayOrigin = transform.position + verticalRayOffset;
        ledgeTop = Physics.Raycast(verticalRayOrigin, Vector3.down, out verticalLedgeHit, verticalRayDistance, whatIsLedge);
        Color verticalRayColor = ledgeTop ? Color.red : Color.green;
        Debug.DrawRay(verticalRayOrigin, Vector3.down * verticalRayDistance, verticalRayColor);
    }

    void ShootHorizontalRay()
    {
        // Horizontal raycast
        ledgeFront = Physics.Raycast(transform.position, orientation.forward, out horizontalLedgeHit, horizontalRayDistance, whatIsLedge);
        Color horizontalRayColor = ledgeFront ? Color.red : Color.green;
        Debug.DrawRay(transform.position, orientation.forward * horizontalRayDistance, horizontalRayColor);
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
        // Step 1: Get the direction the player should face (opposite of the ledge normal)
        Vector3 facingDirection = -horizontalLedgeHit.normal;

        // Step 2: Calculate the rotation based on the facing direction
        Quaternion targetRotation = Quaternion.LookRotation(facingDirection, Vector3.up);

        // Step 3: Smoothly rotate the player towards the target rotation (optional, for smoothness)
        transform.rotation = targetRotation;
        
        // Calculate initial snapping point based on raycast info
        Vector3 awayFromLedge = (transform.position - snappingPoint).normalized;

        float backwardOffset = 0.7f; // Distance away from ledge
        float downwardOffset = 0.6f; // Amount to lower Y position

        // Move player to snapping point
        transform.position = snappingPoint + new Vector3(awayFromLedge.x * backwardOffset, -downwardOffset, awayFromLedge.z * backwardOffset);
        rb.velocity = new Vector3(0, 0, 0);

        canHang = false;
        isHanging = true;
    }

    public void ResetHang()
    {
        canHang = true;
    }

    public void StartPlayerPullUp()
    {
        Debug.Log("Pull-up initiated.");
        float verticalOffset = 1.1f;
        float depthOffset = 1.2f;
        isHanging = false;
        pullUpStartPos = transform.position;
        Vector3 pointAwayFromLedge = horizontalLedgeHit.point + transform.forward * depthOffset;
        aboveLedge = verticalLedgeHit.point + verticalLedgeHit.normal.normalized * verticalOffset;

        pullUpEndPos = new Vector3(pointAwayFromLedge.x, aboveLedge.y, pointAwayFromLedge.z);

        // Start the pull-up coroutine
        StartCoroutine(PlayerPullUpAction());
    }

    IEnumerator PlayerPullUpAction()
    {
        pullUpTimer = 0;
        while (pullUpTimer < pullUpDuration)
        {
            pullUpTimer += Time.deltaTime / pullUpDuration;
            transform.position = Vector3.Lerp(pullUpStartPos, pullUpEndPos, pullUpTimer);

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

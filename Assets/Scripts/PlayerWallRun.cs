using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerWallRun : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    
    // Event to notify when pull up action ends
    public event Action OnWallRunEnd;
    private float maxWallRunTime = 1f;
    public float wallRunTimer;

    [Space(10)]
    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Space(10)]
    [Header("References")]
    public Transform orientation;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        CheckForWall();
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGroundCheck() 
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    public bool WallrunningStateMachine(float verticalInput, bool wallrunning)
    {
        // State 1 - Wallrunning
        if((wallLeft || wallRight) && verticalInput > 0 && AboveGroundCheck())
        {
            if(!wallrunning)
            {
                wallrunning = true;
            }
        }
        // State 2 - Not wallrunning
        else
        {
            if(wallrunning)
            {
                wallrunning = false;
                StopWallRun();  
            }
        }

        return wallrunning;
    }

    public void WallrunningMovement(float horizontalInput)
    {
        wallRunTimer += Time.deltaTime;
        Debug.Log(wallRunTimer);

        if (wallRunTimer > maxWallRunTime)
        {
            rb.useGravity = true;
        }

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }

        // Forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // Push player to wall
        if(!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
        {
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
        }
    }

    public void StopWallRun()
    {
        OnWallRunEnd?.Invoke();
    }
}

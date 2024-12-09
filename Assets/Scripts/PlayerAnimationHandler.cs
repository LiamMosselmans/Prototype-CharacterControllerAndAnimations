using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{
    private PlayerCC_Controller playerController;
    private Animator animator;
    private GameObject playerArms;

    void Awake()
    {
        playerController = GetComponent<PlayerCC_Controller>();
        animator = GetComponentInChildren<Animator>();
        playerArms = GameObject.FindGameObjectWithTag("PlayerArms");
    }
    
    public void UpdateAnimator(PlayerCC_Controller.PlayerState playerState)
    {
        switch(playerState)
        {
            case PlayerCC_Controller.PlayerState.idle:
            animator.SetBool("isRunning", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isWalking", false);
            animator.SetBool("isVaulting", false);
            animator.SetBool("isHanging", false);
            break;

            case PlayerCC_Controller.PlayerState.walking:
            animator.SetBool("isRunning", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isWalking", true);
            animator.SetBool("isVaulting", false);
            animator.SetBool("isHanging", false);
            break;

            case PlayerCC_Controller.PlayerState.sprinting:
            animator.SetBool("isRunning", true);
            animator.SetBool("isVaulting", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isHanging", false);
            break;

            case PlayerCC_Controller.PlayerState.jumping:
            animator.SetBool("isJumping", true);
            break;

            case PlayerCC_Controller.PlayerState.vaulting:
            animator.SetBool("isVaulting", true);
            break;

            case PlayerCC_Controller.PlayerState.hanging:
            animator.SetBool("isHanging", true);
            break;
        }
    }
}

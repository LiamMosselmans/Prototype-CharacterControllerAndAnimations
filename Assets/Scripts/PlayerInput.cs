using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode dropDownKey = KeyCode.P;

    public bool isInputEnabled = true;
    
    public float HorizontalInput()
    {
        return isInputEnabled ? Input.GetAxisRaw("Horizontal"): 0f;
    }

    public float VerticalInput()
    {
        return isInputEnabled ? Input.GetAxisRaw("Vertical"): 0f;
    }

    public bool IsJumping()
    {
        return isInputEnabled && Input.GetKeyDown(jumpKey);
    }
}

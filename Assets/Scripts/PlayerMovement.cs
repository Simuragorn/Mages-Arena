using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float movementSpeed = 5f;

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        float xAxis = Input.GetAxisRaw("Horizontal");
        float yAxis = Input.GetAxisRaw("Vertical");

        if (xAxis != 0f || yAxis != 0f)
        {
            animator.SetInteger(Player.PlayerAnimationStateName, (int)PlayerAnimationState.Walking);
        }
        else
        {
            animator.SetInteger(Player.PlayerAnimationStateName, (int)PlayerAnimationState.Idle);
        }

        float xMovement = xAxis * movementSpeed * Time.deltaTime;
        float yMovement = yAxis * movementSpeed * Time.deltaTime;

        transform.Translate(xMovement, yMovement, 0, Space.World);
    }
}

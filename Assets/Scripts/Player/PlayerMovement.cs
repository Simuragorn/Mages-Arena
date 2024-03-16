using FishNet.Object;
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
        HandleMovement();
    }

    private bool CanMove(Vector2 direction)
    {
        int layerMask = LayerMask.GetMask(LayersConstants.Default, LayersConstants.Target);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 0.5f, layerMask);
        return hit.collider == null;
    }

    private void HandleMovement()
    {
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
        Vector2 movementDirection = new(xAxis, yAxis);
        if (CanMove(movementDirection))
        {
            Move(movementDirection);
        }
    }

    private void Move(Vector2 movementDirection)
    {
        float xMovement = movementDirection.x * movementSpeed * Time.deltaTime;
        float yMovement = movementDirection.y * movementSpeed * Time.deltaTime;

        transform.Translate(xMovement, yMovement, 0, Camera.main.transform);
    }
}

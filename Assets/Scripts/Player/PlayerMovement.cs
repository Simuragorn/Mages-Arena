using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float runningSpeedMultiplier = 2f;
    [SerializeField] private float maxStamina = 100;
    [SerializeField] private float runningStaminaCost = 80f;
    [SerializeField] private float staminaRegeneration = 50f;
    [SerializeField] private float minStaminaForRunning = 5f;
    private bool isRunning = false;

    public float ActualStamina { get; private set; }
    public float MaxStamina => maxStamina;

    private void Awake()
    {
        ActualStamina = maxStamina;
    }

    private void ChangeStamina(float change)
    {
        ActualStamina += change;
        ActualStamina = Mathf.Max(ActualStamina, 0);
        ActualStamina = Mathf.Min(ActualStamina, maxStamina);
    }

    public void Refresh()
    {
        ActualStamina = maxStamina;
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        HandleMovement();
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }
        HandleStaminaRegeneration();
    }

    private void HandleStaminaRegeneration()
    {
        if (isRunning)
        {
            ChangeStamina(-runningStaminaCost * Time.fixedDeltaTime);
        }
        else
        {
            ChangeStamina(staminaRegeneration * Time.fixedDeltaTime);
        }
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
        bool isTryingSprint = Input.GetKey(KeyCode.LeftShift);
        bool runningAllowed = isTryingSprint && ActualStamina >= minStaminaForRunning;
        if (runningAllowed)
        {
            isRunning = true;
        }
        else if (!isTryingSprint)
        {
            isRunning = false;
        }

        if (CanMove(movementDirection))
        {
            Move(movementDirection, runningAllowed);
        }
    }

    private void Move(Vector2 movementDirection, bool runningAllowed)
    {
        float xMovement = movementDirection.x * movementSpeed * Time.deltaTime;
        float yMovement = movementDirection.y * movementSpeed * Time.deltaTime;

        if (runningAllowed)
        {
            xMovement *= runningSpeedMultiplier;
            yMovement *= runningSpeedMultiplier;
        }

        transform.Translate(xMovement, yMovement, 0, Camera.main.transform);
    }
}

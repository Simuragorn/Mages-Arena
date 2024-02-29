using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private Rigidbody2D rigidbody;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float xAxis = Input.GetAxisRaw("Horizontal");
        float yAxis = Input.GetAxisRaw("Vertical");

        float xMovement = xAxis * movementSpeed * Time.deltaTime;
        float yMovement = yAxis * movementSpeed * Time.deltaTime;

        rigidbody.velocity += new Vector2(xMovement, yMovement);
    }
}

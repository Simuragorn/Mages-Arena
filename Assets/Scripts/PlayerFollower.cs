using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollower : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float rotationSpeed = 0.5f;
    [SerializeField] private Vector2 offset = new(0, -3.5f);
    void LateUpdate()
    {
        if (Player.LocalInstance != null)
        {
            var playerTransform = Player.LocalInstance.transform;
            Vector2 desiredPosition = (Vector2)playerTransform.position + offset;
            Vector2 smoothedPosition = Vector2.Lerp(transform.position, desiredPosition, movementSpeed);
            transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
        }
    }
}

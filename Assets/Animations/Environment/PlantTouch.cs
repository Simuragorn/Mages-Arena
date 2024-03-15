using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantTouch : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private const string touchedTrigger = "touched";
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        animator.SetTrigger(touchedTrigger);
    }
    public void UnTouched()
    {
        animator.ResetTrigger(touchedTrigger);
    }
}

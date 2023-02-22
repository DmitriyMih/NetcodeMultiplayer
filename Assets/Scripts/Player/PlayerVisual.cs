using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    private const string IsWalking = "IsWalking";
    [SerializeField] private PlayerControllerNetwork playerController;
    [SerializeField] private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerController = GetComponent<PlayerControllerNetwork>();
    }

    private void Start()
    {
        playerController.IsWalkingChanged += ChangeMoveState;
    }

    private void ChangeMoveState(bool isState)
    {
        if (animator == null)
            return;

        Debug.Log("State" % Colorize.Green);
        animator.SetBool(IsWalking, isState);
    }
}
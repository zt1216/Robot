using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class GroundedStabilizer : MonoBehaviour
{
    [Tooltip("失去地面后维持 Grounded=true 的宽限时间")]
    public float graceTime = 0.15f;

    private Animator anim;
    private CharacterController controller;
    private float graceTimer;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        anim = GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();
    }


    void LateUpdate()
    {
        if (anim == null || controller == null) return;

        if (controller.isGrounded)
        {
            graceTimer = graceTime;
            anim.SetBool("Grounded", true);
        }
        else
        {
            if (Mathf.Abs(controller.velocity.y) < 1f && graceTimer > 0f)
            {
                graceTimer -= Time.deltaTime;
                anim.SetBool("Grounded", true);
            }
        }
    }
}
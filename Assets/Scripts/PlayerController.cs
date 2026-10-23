using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float groundedGrace = 0f;

    [Header("Move Speed")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float sneakSpeed = 2f;

    [Header("Mouse")]
    public float mouseSensitivity = 1f;

    [Header("Jump")]
    public float jumpHeight = 1f;
    public float gravity = -25f;

    [Header("Anim")]
    public float animSpeedScale = 0.6f;

    private float yaw;
    private CharacterController controller;
    private float verticalVelocity = 0f;
    private Animator animator;

    [Header("Sprint FOV")]
    public Camera playerCamera;
    public float normalFOV = 60f;
    public float sprintFOV = 75f;
    public float fovSmooth = 6f;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();
        if(playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float mouseX = Input.GetAxis("Mouse X");

        bool isSneaking = Input.GetKey(KeyCode.LeftControl);
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && v > 0.1f;

        float speed = walkSpeed;
        if (isSneaking)
        {
            speed = sneakSpeed;
        }
        else if (isSprinting)
        {
            speed = sprintSpeed;
        }

        if(controller.isGrounded)
        {
            groundedGrace = 0.15f;
        }
        else if(Mathf.Abs(verticalVelocity) < 2f && groundedGrace > 0f)
        {
            groundedGrace -= Time.deltaTime;
        }
        bool grounded = controller.isGrounded || groundedGrace > 0f;

        if(grounded)
        {
            if(verticalVelocity < 0f)
            {
                verticalVelocity = -1f;
            }
            if(Input.GetKeyDown(KeyCode.Space))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                groundedGrace = 0f;
                animator.SetBool("Jump", true);
            }
            else
            {
                animator.SetBool("Jump", false);
            }
            animator.SetBool("FreeFall", false);
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
            animator.SetBool("FreeFall", true);
        }
        animator.SetBool("Grounded", grounded);

        yaw += mouseX * mouseSensitivity;
        transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        Vector3 inputDir = new Vector3(h, 0f, v);
        if (inputDir.magnitude > 1f)
        {
            inputDir = inputDir.normalized;
        }

        Vector3 moveDir = transform.TransformDirection(inputDir) * speed;
        moveDir.y = verticalVelocity;
        controller.Move(moveDir * Time.deltaTime);

        animator.SetFloat("Speed", inputDir.magnitude * speed * animSpeedScale);
        animator.SetFloat("MotionSpeed", 1f);
        bool sprintActive = isSprinting && !isSneaking;
        if (playerCamera != null)
        {
            float targetFOV = sprintActive ? sprintFOV : normalFOV;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, fovSmooth * Time.deltaTime);
        }
    }
}

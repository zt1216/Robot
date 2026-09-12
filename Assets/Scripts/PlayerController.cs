using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float mouseSensitivity = 1f;
    private float yaw;
    public float sneakSpeed = 2f;
    public float crouchSpeed = 1f;
    public Transform cam;
    private CharacterController controller;
    private float standHeight;
    private Vector3 standCenter;
    private float standCamY;
    public float jumpHeight = 1f;
    public float gravity = -25f;
    private float verticalVelocity = 0f;
    [SerializeField] private float crouchSmooth = 10f;
    void Start()
    {
        controller = GetComponent<CharacterController>();
        standHeight = controller.height;
        standCenter = controller.center;
        standCamY = cam.localPosition.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        bool isCrouching = Input.GetKey(KeyCode.LeftControl);
        bool isSneaking = Input.GetKey(KeyCode.LeftShift);
        float speed = walkSpeed;
        if(isSneaking && !isCrouching)
        {
            speed = sneakSpeed;
        }
        if(isCrouching)
        {
            speed = crouchSpeed;
        }
        float targetH = isCrouching ? 1.2f : standHeight;
        Vector3 targetC = isCrouching ? new Vector3(standCenter.x, standCenter.y - 0.4f, standCenter.z) : standCenter;
        float targetCY = isCrouching ? standCamY - 0.8f : standCamY;
        float smooth = crouchSmooth * Time.deltaTime;
        controller.height = Mathf.Lerp(controller.height, targetH, smooth);
        controller.center = Vector3.Lerp(controller.center, targetC, smooth);
        cam.localPosition = new Vector3(cam.localPosition.x, Mathf.Lerp(cam.localPosition.y, targetCY, smooth), cam.localPosition.z);
        
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float mouseX = Input.GetAxis("Mouse X");

        if(controller.isGrounded)
        {
            verticalVelocity = -1f;
            if(Input.GetKeyDown(KeyCode.Space))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        yaw += mouseX * mouseSensitivity;
        transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

        Vector3 moveDir = new Vector3(h, 0f, v);
        moveDir.y = verticalVelocity;
        controller.Move(transform.TransformDirection(moveDir) * speed * Time.deltaTime);
    }
}

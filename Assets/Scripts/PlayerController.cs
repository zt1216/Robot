using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float mouseSensitivity = 1f;

    private float yaw;

    private CharacterController controller;
    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float mouseX = Input.GetAxis("Mouse X");

        yaw += mouseX * mouseSensitivity;
        transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

        Vector3 moveDir = new Vector3(h, 0f, v);
        
        controller.Move(transform.TransformDirection(moveDir) * speed * Time.deltaTime);
    }
}

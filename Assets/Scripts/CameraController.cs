using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class CameraController : MonoBehaviour
{
    [Header("后坐力")]
    public float recoilUp = 2f;
    public float maxRecoilStack = 6f;
    public float recoilFallSpeed = 10f;

    public float mouseSensitivity = 1f;
    private float recoilPitch;
    private float pitch;
    void LateUpdate()
    {
        if (Time.timeScale == 0f)
        {
            return;
        }
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -30f, 40f);
        recoilPitch = Mathf.MoveTowards(recoilPitch, 0f, recoilFallSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(pitch - recoilPitch, 0f, 0f);
    }

    public void ApplyRecoil()
    {
        recoilPitch = Mathf.Min(recoilPitch + recoilUp, maxRecoilStack);
    }
}

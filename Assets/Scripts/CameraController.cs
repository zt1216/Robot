using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float pitch;
    public float mouseSensitivity = 1f;
    public float recoilUp = 2f;
    private float originalPitch;
    private Coroutine recoilCo;

    void LateUpdate()
    {
        if (Time.timeScale == 0f)
        {
            return;
        }
        float mouseY = Input.GetAxis("Mouse Y");
        
        pitch -= mouseY * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -60f, 60f);

        transform.localRotation = Quaternion.Euler(pitch,0f,0f);

    }

    public void ApplyRecoil()
    {
        originalPitch = pitch;
        if (recoilCo != null)
        {
            StopCoroutine(recoilCo);
        }
        StartCoroutine(RecoilRoutine());
    }

    IEnumerator RecoilRoutine()
    {
        float targetPitch = originalPitch + recoilUp;
        while (pitch < targetPitch)
        {
            if (Input.GetAxis("Mouse Y") != 0f)
            {
                yield break;
            }
            pitch = Mathf.MoveTowards(pitch, targetPitch, 0.2f);
            yield return null;
        }
        while(pitch > originalPitch)
        {
            if (Input.GetAxis("Mouse Y") != 0f)
            {
                yield break;
            }
            pitch = Mathf.MoveTowards(pitch, originalPitch, 0.05f);
            yield return null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(150)]
public class UpperBodyAim : MonoBehaviour
{
    public Transform spine;
    public Transform cam;
    public float maxPitch = 45f;

    void LateUpdate()
    {
        if(spine == null || cam == null)
        {
            return;
        }
        if(Time.timeScale == 0f)
        {
            return;
        }
        float pitch = cam.eulerAngles.x;
        if(pitch > 180f)
        {
            pitch -= 360f;
        }
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        Quaternion offset = Quaternion.AngleAxis(pitch, cam.right);
        spine.rotation = offset * spine.rotation;
    }
}

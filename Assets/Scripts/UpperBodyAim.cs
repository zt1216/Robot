using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpperBodyAim : MonoBehaviour
{
    public Transform spine;
    public Transform cam;
    public float maxPitch = 45f;

    void LateUpdate()
    {
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHoldPose : MonoBehaviour
{
    [Header("Right Arm")]
    public Vector3 rightUpperArmEuler = new Vector3(0f, 0f, 0f);
    public Vector3 rightLowerArmEuler = new Vector3(0f, 0f, 0f);

    [Header("Left Arm")]
    public bool modifyLeftArm = true;
    public Vector3 leftUpperArmEuler = new Vector3(0f, 0f, 0f);
    public Vector3 leftLowerArmEuler = new Vector3(0f, 0f, 0f);

    [Header("Spine")]
    public bool modifySpine = false;
    public Vector3 spineEuler = new Vector3(0f, 0f, 0f);

    [Header("空中处理")]
    public float poseBlendSpeed = 6f;

    private float poseWeight = 1f;
    private Animator anim;
    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError("[WeaponHoldPose]必须挂在有Animator的物体上", this);
            enabled = false;
        }
    }

    void Update()
    {
        float target = anim.GetBool("Grounded") ? 1f : 0f;
        poseWeight = Mathf.MoveTowards(poseWeight, target, poseBlendSpeed * Time.deltaTime);
    }
    void LateUpdate()
    {
        if(anim == null)
        {
            return;
        }

        SetBone(HumanBodyBones.RightUpperArm, rightUpperArmEuler);
        SetBone(HumanBodyBones.RightLowerArm, rightLowerArmEuler);

        if(modifyLeftArm)
        {
            SetBone(HumanBodyBones.LeftUpperArm, leftUpperArmEuler);
            SetBone(HumanBodyBones.LeftLowerArm, leftLowerArmEuler);
        }
        if(modifySpine)
        {
            SetBone(HumanBodyBones.Spine, spineEuler);
        }
    }

    void SetBone(HumanBodyBones bone,Vector3 euler)
    {
        Transform t = anim.GetBoneTransform(bone);
        if(t == null)
        {
            Debug.LogWarning("[WeaponHoldPose]找不到骨骼：" + bone, this);
            return;
        }
        t.localRotation = Quaternion.Slerp(t.localRotation,Quaternion.Euler(euler),poseWeight);
    }
}

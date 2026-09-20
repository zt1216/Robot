using UnityEngine;

public class WeaponPose : MonoBehaviour
{
    [Header("Gun - child of Right_Hand")]
    public Transform gun;
    [Header("Gun offset relative to Right_Hand")]
    public Vector3 gunLocalPos = new Vector3(0.05f, 0.08f, 0f);
    public Vector3 gunLocalRot = new Vector3(0f, 180f, 0f);
    [Header("Arm pose - OFF by default. Humanoid rigs fight direct bone edits")]
    public bool modifyArmBones = false;
    public Transform rightUpperArm;
    public Transform rightLowerArm;
    [Range(0f, 1f)] public float weight = 0f;
    public Vector3 upperArmOffset = Vector3.zero;
    public Vector3 lowerArmOffset = Vector3.zero;

    [Tooltip("切枪等临时姿势偏移，由 PlayerShooting 驱动")]
    public Vector3 poseOffset = Vector3.zero;

    [Tooltip("后坐力位移，由 PlayerShooting 驱动")]
    public Vector3 recoilOffset = Vector3.zero;
    [Tooltip("切枪等临时旋转偏移")]
    public Vector3 poseRotOffset = Vector3.zero;

    void LateUpdate()
    {
        ApplyGun();
        if (modifyArmBones)
        {
            ApplyArm();
        }
    }

    void ApplyGun()
    {
        if (gun == null)
        {
            return;
        }
        gun.localPosition = gunLocalPos + poseOffset + recoilOffset;
        gun.localRotation = Quaternion.Euler(gunLocalRot) * Quaternion.Euler(poseRotOffset);
    }

    void ApplyArm()
    {
        if (rightUpperArm == null || rightLowerArm == null)
        {
            return;
        }
        if (weight <= 0f)
        {
            return;
        }

        Quaternion uBase = rightUpperArm.localRotation;
        Quaternion lBase = rightLowerArm.localRotation;

        Quaternion upperTarget = uBase * Quaternion.Euler(upperArmOffset);
        Quaternion lowerTarget = lBase * Quaternion.Euler(lowerArmOffset);

        rightUpperArm.localRotation = Quaternion.Slerp(uBase, upperTarget, weight);
        rightLowerArm.localRotation = Quaternion.Slerp(lBase, lowerTarget, weight);
    }
}

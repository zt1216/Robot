using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShooting : MonoBehaviour
{
    public GameObject ball;
    public int[] targetDamage = { 1, 3 };
    public int[] enemyDamage = { 34, 90 };
    public float[] fireRate = { 0.2f, 0.8f };
    public float[] reloadTime = { 1.5f, 2.2f };
    public int[] magSize = { 30, 8 };
    public int[] currentAmmo;
    public int weaponIndex = 0;
    public Text currentAmmoText;
    public GameObject damageTextPrefabs;
    public GameObject tracerPrefab;
    public Transform muzzle;
    public RectTransform canvasRect;
    public WeaponPose weaponPose;
    private bool ammoDirty = true;
    private float lastShootTime = 0f;
    private bool isReloading = false;
    public CameraController cameraController;
    public AudioClip shootClip;
    private AudioSource sfx;
    public LayerMask hitMask;
    public Transform gunModel;
    private bool isSwitching = false;
    private Coroutine reloadRoutine;
    private Coroutine recoilPoseRoutine;
    public float gunKick = 0.06f;

    void Start()
    {
        currentAmmo = new int[magSize.Length];
        for(int i = 0;i < magSize.Length;i++)
        {
            currentAmmo[i] = magSize[i];
        }
        currentAmmoText.text = currentAmmo[weaponIndex] + "/" + magSize[weaponIndex];
        sfx = GetComponent<AudioSource>();
        ammoDirty = false;
    }
    void Update()
    {
        if(Time.timeScale == 0f)
        {
            return;
        }
        if(Input.GetKeyDown(KeyCode.Q))
        {
            if (isSwitching == false)
            {
                StartCoroutine(SwitchWeapon());
            }
        }
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        if (Input.GetMouseButton(0) && isReloading == false && currentAmmo[weaponIndex] > 0 && isSwitching == false)
        {
            if (Time.time - lastShootTime >= fireRate[weaponIndex])
            {
                if (Physics.Raycast(ray, out hit, 100f,hitMask))
                {
                    Target shoot = hit.collider.GetComponent<Target>();

                    if (shoot != null)
                    {
                        shoot.TakeDamage(targetDamage[weaponIndex]);
                    }
                    EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
                    if(enemy != null)
                    {
                        enemy.TakeDamage(enemyDamage[weaponIndex]);
                        GameObject popup = Instantiate(damageTextPrefabs);
                        popup.transform.SetParent(canvasRect, false);

                        DamagePopup dp = popup.GetComponent<DamagePopup>();
                        dp.SetWorldPos(hit.point);
                        dp.Init(enemyDamage[weaponIndex]);
                    }
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 1f);
                    GameObject ball1 = Instantiate(ball, hit.point, Quaternion.identity);
                    Destroy(ball1, 1f);
                    SpawnTracer(hit.point);
                }
                else
                {
                    SpawnTracer(ray.origin + ray.direction * 100f);
                }
                lastShootTime = Time.time;
                currentAmmo[weaponIndex]--;
                ammoDirty = true;
                if(cameraController != null)
                {
                    cameraController.ApplyRecoil();
                }
                KickGun();
                if (shootClip != null && sfx != null)
                {
                    sfx.PlayOneShot(shootClip);
                }
            }
        }
        if(isReloading == false && currentAmmo[weaponIndex] <= 0)
        {
            reloadRoutine = StartCoroutine(Reload());
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            if(isReloading == false)
            {
                reloadRoutine = StartCoroutine(Reload());
            }
        }
        
        if(ammoDirty)
        {
            currentAmmoText.text = currentAmmo[weaponIndex] + "/" + magSize[weaponIndex];
            ammoDirty = false;
        }
    }

    void SpawnTracer(Vector3 targetPos)
    {
        GameObject t = Instantiate(tracerPrefab, muzzle.position, Quaternion.identity);
        t.GetComponent<Tracer>().FlyTo(targetPos);
    }

    IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime[weaponIndex]);
        currentAmmo[weaponIndex] = magSize[weaponIndex];
        ammoDirty = true;
        isReloading = false;
    }

    IEnumerator SwitchWeapon()
    {
        isSwitching = true;

        if(reloadRoutine != null)
        {
            StopCoroutine(reloadRoutine);
            reloadRoutine = null;
            isReloading = false;
        }

        Vector3 localDown = gunModel.parent.InverseTransformDirection(Vector3.down);
        float dip = 0.3f;
        float t =  0f;
        while(t < 1f)
        {
            t += Time.deltaTime / 0.15f;
            gunModel.localPosition = localDown * (dip * t);
            weaponPose.poseRotOffset = new Vector3(-40f * t, 0f, 0f);
            yield return null;
        }
        weaponIndex = 1 - weaponIndex;
        ammoDirty = true;
        lastShootTime = Time.time;

        t = 0f;
        while(t < 1f)
        {
            t += Time.deltaTime / 0.15f;
            weaponPose.poseOffset = localDown * (dip * (1f - t));
            weaponPose.poseRotOffset = new Vector3(-40f * (1f - t), 0f, 0f);
            yield return null;
        }
        weaponPose.poseOffset = Vector3.zero;
        weaponPose.poseRotOffset = Vector3.zero;

        isSwitching = false;
    }

    void KickGun()
    {
        if (weaponPose == null || gunModel == null)
        {
            return;
        }
        Vector3 localBack = gunModel.parent.InverseTransformDirection(-gunModel.forward);
        weaponPose.recoilOffset = localBack * gunKick;

        if(recoilPoseRoutine != null)
        {
            StopCoroutine(recoilPoseRoutine);
        }
        recoilPoseRoutine = StartCoroutine(RecoilPoseRoutine());
    }

    IEnumerator RecoilPoseRoutine()
    {
        Vector3 start = weaponPose.recoilOffset;
        float t = 0f;
        while(t < 1f)
        {
            t += Time.deltaTime / 0.12f;
            weaponPose.recoilOffset = Vector3.Lerp(start, Vector3.zero, t);
            yield return null;
        }
        weaponPose.recoilOffset = Vector3.zero;
        recoilPoseRoutine = null;
    }
}

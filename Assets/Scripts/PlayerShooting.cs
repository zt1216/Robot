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
    public RectTransform canvasRect;
    private bool ammoDirty = true;
    private float lastShootTime = 0f;
    private bool isReloading = false;
    public CameraController cameraController;
    public AudioClip shootClip;
    private AudioSource sfx;
    public LayerMask hitMask;

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
            weaponIndex = 1 - weaponIndex;
            StopAllCoroutines();
            isReloading = false;
            lastShootTime = Time.time;
            ammoDirty = true;
        }
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        if (Input.GetMouseButton(0) && isReloading == false && currentAmmo[weaponIndex] > 0)
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
                    Destroy(ball1, 2f);
                }
                lastShootTime = Time.time;
                currentAmmo[weaponIndex]--;
                ammoDirty = true;
                if(cameraController != null)
                {
                    cameraController.ApplyRecoil();
                }
                if(shootClip != null && sfx != null)
                {
                    sfx.PlayOneShot(shootClip);
                }
            }
        }
        if(isReloading == false && currentAmmo[weaponIndex] <= 0)
        {
            StartCoroutine(Reload());
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            if(isReloading == false)
            {
                StartCoroutine(Reload());
            }
        }
        
        if(ammoDirty)
        {
            currentAmmoText.text = currentAmmo[weaponIndex] + "/" + magSize[weaponIndex];
            ammoDirty = false;
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime[weaponIndex]);
        currentAmmo[weaponIndex] = magSize[weaponIndex];
        ammoDirty = true;
        isReloading = false;
    }
}

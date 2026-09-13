using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShooting : MonoBehaviour
{
    public GameObject ball;
    public int targetDamage = 1;
    public int enemyDamage = 34;
    public int magSize = 30;
    public int currentAmmo;
    public Text currentAmmoText;
    private bool ammoDirty = true;
    private float lastShootTime = 0f;
    private bool isReloading = false;
    public CameraController cameraController;
    public AudioClip shootClip;
    private AudioSource sfx;
    public LayerMask hitMask;

    void Start()
    {
        currentAmmo = magSize;
        currentAmmoText.text = currentAmmo + "/" + magSize;
        sfx = GetComponent<AudioSource>();
        ammoDirty = false;
    }
    void Update()
    {
        if(Time.timeScale == 0f)
        {
            return;
        }
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        if (Input.GetMouseButton(0) && isReloading == false && currentAmmo > 0)
        {
            if (Time.time - lastShootTime >= 0.2f)
            {
                if (Physics.Raycast(ray, out hit, 100f,hitMask))
                {
                    Target shoot = hit.collider.GetComponent<Target>();

                    if (shoot != null)
                    {
                        shoot.TakeDamage(targetDamage);
                    }
                    EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
                    if(enemy != null)
                    {
                        enemy.TakeDamage(enemyDamage);
                    }
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 1f);
                    GameObject ball1 = Instantiate(ball, hit.point, Quaternion.identity);
                    Destroy(ball1, 2f);
                }
                lastShootTime = Time.time;
                currentAmmo--;
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
        if(isReloading == false && currentAmmo <= 0)
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
            currentAmmoText.text = currentAmmo + "/" + magSize;
            ammoDirty = false;
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(1.5f);
        currentAmmo = magSize;
        ammoDirty = true;
        isReloading = false;
    }
}

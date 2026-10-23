using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DamagePopup : MonoBehaviour
{
    public float floatSpeed = 50f;
    public float lifetime = 0.6f;
    private Text txt;
    private RectTransform rect;
    private float elapsed;
    void Awake()
    {
        txt = GetComponent<Text>();
        rect = GetComponent<RectTransform>();
    }

    public void Init(int damage)
    {
        txt.text = damage.ToString();
    }

    void Update()
    {
        rect.anchoredPosition += Vector2.up * floatSpeed * Time.unscaledDeltaTime;

        elapsed += Time.unscaledDeltaTime;
        if(elapsed > lifetime / 2)
        {
            Color c = txt.color;
            c.a -= Time.unscaledDeltaTime / (lifetime / 2);
            txt.color = c;
        }
        if(elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void SetWorldPos(Vector3 worldPos)
    {
        rect.position = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPos);
    }
}

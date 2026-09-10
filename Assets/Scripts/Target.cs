using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public int hp = 3;

    public void TakeDamage(int damage)
    {
        hp -=damage;
        if(hp <=0)
        {
            Destroy(this.gameObject);
            Debug.Log("目标摧毁");
        }
    }
}

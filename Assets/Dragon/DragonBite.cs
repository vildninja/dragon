using UnityEngine;
using System.Collections;

public class DragonBite : MonoBehaviour {

    public DragonSpawner spawner;

    public float damage;

    void OnTriggerEnter2D(Collider2D col)
    {
        var dp = col.GetComponent<DragonPiece>();
        if (dp && dp.spawner != spawner)
        {
            dp.hit = true;
            dp.spawner.Hit(damage);
            damage = 0;
            Destroy(gameObject);
        }
    }
}

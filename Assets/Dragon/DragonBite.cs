using UnityEngine;
using System.Collections;

public class DragonBite : MonoBehaviour {

    public DragonSpawner spawner;

    public float damage;

    void OnTriggerEnter2D(Collider2D col)
    {
        var dp = col.GetComponent<DragonPiece>();
        if (dp && damage > 0 && dp.spawner != spawner)
        {
            if (spawner.health > 0)
            {
                dp.hit = true;
                dp.spawner.Hit(damage);
            }
            damage = 0;
            col.rigidbody2D.AddForce(rigidbody2D.velocity.normalized * 10000);
            spawner.StopBite();
        }
    }
}

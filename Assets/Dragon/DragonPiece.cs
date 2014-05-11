using UnityEngine;
using System.Collections;

public class DragonPiece : MonoBehaviour {

    public DragonSpawner spawner;

    public Transform front;

    ParticleSystem blood;

    public bool hit = false;

    void Start()
    {
        spawner = transform.parent.GetComponent<DragonSpawner>();
        blood = GetComponentInChildren<ParticleSystem>();
        blood.transform.forward = transform.right + (Random.value > 0.5f ? transform.up : -transform.up);
    }

    void Update()
    {
        if (hit && blood)
        {
            blood.emissionRate = spawner.pulseAnimation.Evaluate(Time.time) * Mathf.Clamp(6 * (spawner.maxHealth - spawner.health) / spawner.maxHealth, 1, 5);
            blood.startSpeed = Mathf.Clamp(rigidbody2D.velocity.magnitude, 1, 10);
        }
    }

    void FixedUpdate()
    {
        Vector2 projected = Vector3.Project(rigidbody2D.velocity, transform.up);
        rigidbody2D.AddForce(-projected);
        Vector2 direction = new Vector2(projected.y, -projected.x);

        if (front)
        {
            Vector2 swim = front.right;
            float a = Vector2.Angle(swim, transform.right);
            a *= Mathf.Sign(Vector3.Cross(transform.right, swim).z);

            rigidbody2D.AddTorque(a);

            rigidbody2D.AddForce(transform.right * 50);
        }
    }
}

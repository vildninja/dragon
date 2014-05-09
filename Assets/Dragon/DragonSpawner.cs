using UnityEngine;
using System.Collections;

public class DragonSpawner : MonoBehaviour {

    public AnimationCurve dragonCurve;

    public Transform gfx;

    private Rigidbody2D biggest;
    private Rigidbody2D head;

	// Use this for initialization
	void Awake () {

        GameObject last = null;
        Vector3 point = transform.position;

        var limits = new JointAngleLimits2D();
        limits.max = 20;

        float biggestR = 0;

        for (int i = 0; i < dragonCurve[dragonCurve.length - 1].time; i++)
        {
            float r = dragonCurve.Evaluate(i);
            point.x += r;
            GameObject next = new GameObject(name + " " + i, typeof(Rigidbody2D), typeof(CircleCollider2D));
            next.GetComponent<CircleCollider2D>().radius = r;
            next.transform.position = point;
            next.rigidbody2D.mass = Mathf.PI * r * r;
            next.rigidbody2D.drag = 1;
            next.rigidbody2D.angularDrag = 1;

            var g = Instantiate(gfx, next.transform.position, Quaternion.identity) as Transform;
            g.parent = next.transform;
            g.localScale = Vector3.one * r * 2;
            point.x += r;

            if (r > biggestR)
            {
                biggestR = r;
                biggest = next.rigidbody2D;
            }

            if (last)
            {
                var joint = next.AddComponent<HingeJoint2D>();
                joint.connectedBody = last.rigidbody2D;
                joint.connectedAnchor = new Vector2(next.GetComponent<CircleCollider2D>().radius + last.GetComponent<CircleCollider2D>().radius, 0);
                joint.collideConnected = false;
                joint.useLimits = true;
                joint.limits = limits;
            }
            last = next;
        }
        head = last.rigidbody2D;
	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            biggest.AddForce(Vector2.up * 1000);
        }
	}
}

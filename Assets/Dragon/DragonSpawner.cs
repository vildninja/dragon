using UnityEngine;
using System.Collections;

public class DragonSpawner : MonoBehaviour {

    public AnimationCurve dragonCurve;
    public AnimationCurve pulseAnimation;

    public Sprite piece;
    public Sprite headGfx;
    public ParticleSystem bloodParticles;
    public ParticleSystem fatalBoodParticles;
    public ParticleSystem biteFx;

    private Rigidbody2D biggest;
    private Rigidbody2D head;
    private Rigidbody2D tail;

    public float wingForce;
    public float swimForce;

    public float minTailSpeed;
    public AnimationCurve tailDamage;

    public float maxHealth = 100;
    public float health;

    public bool player2;

    Transform headGfxTransform;

	// Use this for initialization
	void Awake () {

        health = maxHealth;

        GameObject last = null;
        Vector3 point = Vector3.zero;

        var limits = new JointAngleLimits2D();
        limits.max = 40;
        limits.min = -40;

        float biggestR = 0;

        for (int i = 0; i < dragonCurve[dragonCurve.length - 1].time; i++)
        {
            float r = dragonCurve.Evaluate(i);
            point.x += r;
            point.z -= 0.1f;
            GameObject next = new GameObject(name + " " + i, typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(DragonPiece));
            if (!tail)
                tail = next.rigidbody2D;

            next.transform.parent = transform;

            next.GetComponent<CircleCollider2D>().radius = r;
            next.transform.position = transform.TransformPoint(point);
            next.transform.rotation = transform.rotation;
            next.rigidbody2D.mass = Mathf.PI * r * r;
            next.rigidbody2D.drag = 3;
            next.rigidbody2D.angularDrag = 1;
            next.rigidbody2D.gravityScale = 1;

            next.layer = gameObject.layer;

            var g = new GameObject("gfx", typeof(SpriteRenderer));
            g.GetComponent<SpriteRenderer>().sprite = piece;
            g.transform.parent = next.transform;
            g.transform.localScale = Vector3.one * r * 2;
            g.transform.localPosition = Vector3.zero;
            g.transform.localRotation = Quaternion.identity;

            var blood = Instantiate(bloodParticles, next.transform.position, next.transform.rotation) as ParticleSystem;
            blood.transform.parent = next.transform;


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
                last.GetComponent<DragonPiece>().front = next.transform;
            }
            last = next;
        }
        headGfxTransform = last.transform.Find("gfx");
        headGfxTransform.GetComponent<SpriteRenderer>().sprite = headGfx;

        biteFx = Instantiate(biteFx, last.transform.position + new Vector3(0, 0, -1), Quaternion.identity) as ParticleSystem;
        biteFx.transform.parent = last.transform;
        biteFx.enableEmission = false;

        head = last.rigidbody2D;
	}

    public void Hit(float damage)
    {
        health -= damage;

        StartCoroutine(SlowMo());

        if (health <= 0)
        {
            foreach (var hj in GetComponentsInChildren<HingeJoint2D>())
            {
                if (Random.value < 0.3f)
                {
                    Destroy(hj);
                    var p = Instantiate(fatalBoodParticles, hj.transform.position, Quaternion.identity) as ParticleSystem;
                    p.transform.parent = hj.transform;
                }
            }

            foreach (var dp in GetComponentsInChildren<DragonPiece>())
            {
                dp.rigidbody2D.drag = 0.1f;
                Destroy(dp.GetComponentInChildren<ParticleSystem>());
                Destroy(dp);
            }
            StartCoroutine(Restart());
        }
    }

    bool isRestarting = false;
    IEnumerator Restart()
    {
        if (!isRestarting)
        {
            isRestarting = true;
            yield return new WaitForSeconds(8);
            Application.LoadLevel(Application.loadedLevel);
        }
    }

    static int slowMoCounter = 0;

    IEnumerator SlowMo()
    {
        slowMoCounter++;
        Time.timeScale = 0.4f;
        GameObject.Find("/Flash").renderer.enabled = true;
        yield return new WaitForSeconds(0.05f);
        GameObject.Find("/Flash").renderer.enabled = false;
        yield return new WaitForSeconds(0.5f);
        slowMoCounter--;
        if (slowMoCounter == 0)
            Time.timeScale = 1;
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(biggest.transform.position, leftStick);
            Gizmos.DrawWireCube(biggest.transform.position, Vector2.one);
            Gizmos.DrawRay(head.transform.position, rightStick);
            Gizmos.DrawWireCube(head.transform.position, Vector2.one);
        }
    }


    Vector2 leftStick
    {
        get
        {
            return new Vector2(Input.GetAxis("Horizontal" + (player2 ? "2" : "")), Input.GetAxis("Vertical" + (player2 ? "2" : "")));
        }
    }

    Vector2 rightStick
    {
        get
        {
            return new Vector2(Input.GetAxis("HorizontalRight" + (player2 ? "2" : "")), Input.GetAxis("VerticalRight" + (player2 ? "2" : "")));
        }
    }

    bool mayFlap = true;
    bool isCharging = false;
    float chargeCooldown = 2;

    void Update()
    {
        if (health > 0)
        {
            if (leftStick.sqrMagnitude < 0.5f)
                mayFlap = true;

            if (Input.GetAxis("FireRight" + (player2 ? "2" : "")) > 0.5f && !isCharging)
            {
                StartCoroutine(Charge());
            }

            if (Mathf.Abs(headGfxTransform.up.y) > 0.2f)
            {
                var s = headGfxTransform.localScale;
                s.y = s.x * Mathf.Sign(headGfxTransform.up.y);
                headGfxTransform.localScale = s;
            }
        }
    }

    IEnumerator Charge()
    {
        isCharging = true;
        biteFx.enableEmission = true;
        head.AddForce(rightStick * swimForce * 100);

        var bite = new GameObject("Bite", typeof(DragonBite), typeof(CircleCollider2D));
        bite.transform.parent = head.transform;
        bite.transform.localPosition = Vector3.zero;
        var circle = bite.GetComponent<CircleCollider2D>();
        circle.radius = head.GetComponent<CircleCollider2D>().radius;
        circle.isTrigger = true;
        var db = bite.GetComponent<DragonBite>();
        db.spawner = this;
        db.damage = 20.1f;

        for (float t = 0; t < 0.3f; t += Time.fixedDeltaTime)
        {
            yield return new WaitForFixedUpdate();
            head.AddForce(rightStick * swimForce * 10);
        }

        if (biteFx)
            biteFx.enableEmission = false;
        if (db)
            db.damage = 10.1f;

        yield return new WaitForSeconds(.2f);


        if (db)
            Destroy(db.gameObject);

        yield return new WaitForSeconds(1);

        isCharging = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (health > 0)
        {
            Vector2 wingInput = leftStick;
            if (mayFlap && wingInput.sqrMagnitude > 0.7f)
            {
                mayFlap = false;
                biggest.AddForce(wingInput * wingForce);
            }

            Vector2 swim = rightStick;
            if (swim.sqrMagnitude > 0.5f)
            {
                float a = Vector2.Angle(swim, head.transform.right);
                a *= Mathf.Sign(Vector3.Cross(head.transform.right, swim).z);

                head.AddTorque(a);
                head.AddForce(head.transform.right * swimForce);
            }
        }

        //if (tail.velocity.sqrMagnitude > minTailSpeed)
        //{

        //}
        //else if (tailSwing)
        //{
        //    tailSwing.enabled = false;
        //}
	}
}

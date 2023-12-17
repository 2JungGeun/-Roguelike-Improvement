using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // Start is called before the first frame update
    private Vector2 startPos = new Vector2();
    private float distance;
    private Vector2 direction;
    private int damage;
    private float lookAt;
    private float speed = 8.0f;
    void Start()
    {
        startPos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector2.Distance(startPos, this.transform.position) >= distance)
        {
            Destroy(this.gameObject);
        }
    }

    private void FixedUpdate()
    {
        transform.Translate(Vector3.right * lookAt * speed * Time.fixedDeltaTime);
    }

    public void Initialize(float lookAt, Vector2 direction, float distance, float damage)
    {
        this.distance = distance;
        this.damage = (int)damage;
        this.lookAt = lookAt;
        this.direction = direction;
        this.transform.localScale = new Vector3(lookAt, 1.0f, 1.0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<BossScript>().Hit(this.damage);
            Destroy(this.gameObject);
        }
        else if (collision.CompareTag("Ground"))
        {
            Destroy(this.gameObject);
        }

    }
}

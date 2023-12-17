using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    private float time;
    private GameObject projectile;
    private GameObject hitEffect;
    private void Awake()
    {
        projectile = Resources.Load<GameObject>("Prefab/Projectile/SoldierProjectile2");
        hitEffect = Resources.Load<GameObject>("Prefab/hitEffect");
    }
    // Start is called before the first frame update
    void Start()
    {
        time = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if(time>1.0f)
        {
            GameObject obj = Object.Instantiate(projectile, this.transform.position + new Vector3(1.0f, 0.0f, 0.0f), Quaternion.identity);
            obj.GetComponent<Projectile>().Initialize(1.0f, new Vector2(1, 0), 5.0f, 50);
            time = 0.0f;
        }
    }

    public void Hit()
    {
        Debug.Log("¸ÂÀ½" + this.name);
        GameObject obj = Object.Instantiate(hitEffect, this.transform.position, Quaternion.identity);
        Destroy(obj, 0.3f);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Monster : MonoBehaviour, IMonster
{
    private float time;
    private GameObject hitEffect;
    private void Awake()
    {
        hitEffect = Resources.Load<GameObject>("Prefab/hitEffect");
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Hit(int damage)
    {
        Debug.Log("¸ÂÀ½" + this.name);
        GameObject obj = Object.Instantiate(hitEffect, this.transform.position, Quaternion.identity);
        Destroy(obj, 0.3f);
    }
}

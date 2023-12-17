using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class GuidedMissile : MonoBehaviour
{
    private Vector3 target;
    private Dictionary<float, Vector3> targetDic;
    private int damage;
    private int count = 0;
    // Start is called before the first frame update
    void Start()
    {
        targetDic = new Dictionary<float, Vector3>();
        StartCoroutine("TimeOver");
        targetDic = FindEnemy();
        if (targetDic != null)
            target = targetDic.First().Value;
        else
            target = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = Vector2.MoveTowards(this.transform.position, target, 12.0f * 0.8f * Time.deltaTime);
        if(this.transform.position == target && count == 0)
        {
            targetDic = FindEnemy();
            if (targetDic != null)
                target = targetDic.First().Value;
            else
                Destroy(this.gameObject);
        }
    }

    public void Initialize(int damage)
    {
        this.damage = damage;
    }

    public Dictionary<float, Vector3> FindEnemy()
    {
        RaycastHit2D[] enemys = Physics2D.CircleCastAll(this.transform.position, 20.0f, Vector2.up, 0.0f, (int)Layer.Monster);
        if(enemys.Length != 0)
        {
            Dictionary<float, Vector3> tempDic = new Dictionary<float, Vector3>();
            foreach(RaycastHit2D enemy in enemys)
            {
                float distance = Vector2.Distance(this.transform.position, enemy.transform.position);
                if (!tempDic.ContainsKey(distance))
                {
                    tempDic.Add(distance, enemy.transform.position);
                }
            }
            Debug.Log("½ÇÇà");
            return tempDic.OrderBy(item => item.Key).ToDictionary(x => x.Key, x => x.Value);          
        }
        return null;
    }

    IEnumerator TimeOver()
    {
        yield return new WaitForSeconds(10000.0f);
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            collision.GetComponent<BossScript>().Hit(this.damage);
            Destroy(this.gameObject);
        }
    }
}

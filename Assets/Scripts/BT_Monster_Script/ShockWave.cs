using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockWave : MonoBehaviour
{
    private float animeTime;
    // Start is called before the first frame update
    void Start()
    {
        animeTime = this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length - 0.02f;
        StartCoroutine(DestroyObject());
    }
    IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(animeTime);
        Destroy(this.gameObject);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerController>().Hit(DamageType.HP, 1);
        }
    }
}

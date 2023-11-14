using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Money : MonoBehaviour
{
    private int money = 100;
    private PlayerController player = null;
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A) && player != null)
        {
            player.GetMoney(money);
            Destroy(this.gameObject);
        }
    }
    
    public void Initialize(int money)
    {
        this.money = money;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            player = collision.GetComponent<PlayerController>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = null;
        }
    }
}

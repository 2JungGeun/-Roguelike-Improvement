using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Layer
{
    Default = 1,
    Player = 8,
    Monster = 128
}

public class PassThroghPlatform : MonoBehaviour
{
    private PlatformEffector2D platform;
    private bool playerOnPlatform;
    // Start is called before the first frame update
    void Start()
    {
        platform = GetComponent<PlatformEffector2D>();
    }
    // Update is called once per frame
    void Update()
    {
        if (playerOnPlatform && Input.GetAxisRaw("Vertical") < 0 && Input.GetButtonDown("Jump"))
        {
            platform.colliderMask = (int)Layer.Monster;
            this.gameObject.layer = 2;
            StartCoroutine(EnableCollider());
        }
    }

    IEnumerator EnableCollider()
    {
        yield return new WaitForSeconds(0.5f);
        platform.colliderMask = (int)Layer.Player + (int)Layer.Monster;
        this.gameObject.layer = 6;
    }

    private void SetPlayerOnPlatform(bool value)
    {
        playerOnPlatform = value;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            SetPlayerOnPlatform(value: true);
        }
       
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            SetPlayerOnPlatform(value: false);
        }
    }
}

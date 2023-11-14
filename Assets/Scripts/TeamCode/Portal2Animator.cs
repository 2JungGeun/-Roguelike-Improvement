using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal2Animator : MonoBehaviour
{
    public AudioClip bgmDefault;
    public AudioClip bgm2;
    private AudioSource audioSource;
    public Animator animator;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = bgmDefault;
        audioSource.loop = true;
        audioSource.Play();
    }
    void Update()
    {
        animator.Play("portal2");
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.CompareTag("Player"))
        {
            audioSource.Stop();
            audioSource.clip = bgm2;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}

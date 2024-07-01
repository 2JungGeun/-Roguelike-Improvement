using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statue : MonoBehaviour
{
    private AudioSource audio;
    public AudioSource Audio { get { return audio; } set { audio = value; } }

    private AudioClip audioClip;

    private int[] randomNum = new int[3];
    private List<(string, bool)> soulList = new List<(string, bool)>();
    public List<(string, bool)> SoulList { get { return soulList; } }
    private List<string> playerSoulList = new List<string>();
    private bool isConnectedPlayer = false;
    private bool isActivatedUI = false;
    public bool IsActivatedUI { set { isActivatedUI = value; } }
    private bool isSelectedSoul = false;

    public delegate void staueUIEventHandler(bool activate);
    public staueUIEventHandler StaueUIEventHandler;

    private void Start()
    {
        audio = this.GetComponent<AudioSource>();
        audioClip = Resources.Load<AudioClip>("Sound/UISound/SelectStatue");
    }
    // Update is called once per frame
    void Update()
    {
        if (!isConnectedPlayer)
            return;

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (!isSelectedSoul)
            {
                SelectSoul();
                isSelectedSoul = true;
            }
            if (!isActivatedUI)
            {
                UIManager.Instance.ShowStatueUI(this);
                audio.clip = audioClip;
                audio.Play();
                isActivatedUI = true;
            }
            else
            {
                UIManager.Instance.HideStatueUi();
                isActivatedUI = false;
            }
        }
    }

    private void SelectSoul()
    {
        int i = 0;
        bool result = false;
        while (i < UIManager.Instance.SoulSeclectorUINum)
        {
            randomNum[i] = Random.Range(0, DataManager.Instance.SoulList.Count);
            if (FindNumber(i))
                continue;
            foreach(string name in playerSoulList)
            {
                result = name == DataManager.Instance.SoulList[randomNum[i]];
                if (result)
                    break;
            }
            if (result)
                continue;
            soulList.Add((DataManager.Instance.SoulList[randomNum[i]], true));
            i++;
        }
    }

    private bool FindNumber(int index)
    {
        for (int j = 0; j < index; j++)
        {
            if (randomNum[j] == randomNum[index])
                return true;
        }
        return false;
    }

    public void SetSoulDisabled(int index)
    {
        soulList[index] = (soulList[index].Item1, false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            playerSoulList.Clear();
            isConnectedPlayer = true;
            playerSoulList = collision.GetComponent<PlayerController>().GetPlayerSoulNameList();
            foreach(string name in playerSoulList)
            {
                Debug.Log(name);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isConnectedPlayer = false;
            isActivatedUI = false;
            UIManager.Instance.HideStatueUi();
        }
    }
}

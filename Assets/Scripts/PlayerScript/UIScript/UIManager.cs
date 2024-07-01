using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField]
    private GameObject statueUI;
    private int soulSelectorUINum;
    public int SoulSeclectorUINum { get { return soulSelectorUINum; } }
    private new void Awake()
    {
        base.Awake();
        GameObject obj = GameObject.Find("StatueUICanvas");
        statueUI = obj.transform.GetChild(0).gameObject;
        soulSelectorUINum = 3;
    }

    public void ShowStatueUI(Statue statue)
    {
        statueUI.SetActive(true);
        statueUI.GetComponent<StatueUI>().Initialize(statue);
    }

    public void HideStatueUi()
    {
        statueUI.SetActive(false);
    }

}

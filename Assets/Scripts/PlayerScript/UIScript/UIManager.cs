using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static UIManager GetUIManager()
    {
        if (instance == null)
        {
            instance = GameObject.FindObjectOfType<UIManager>();
            if (instance == null)
            {
                GameObject container = new GameObject("UIManager");
                instance = container.AddComponent<UIManager>();
            }
        }
        return instance;
    }

    private static UIManager instance;

    [SerializeField]
    private GameObject statueUI;
    private int soulSelectorUINum;
    public int SoulSeclectorUINum { get { return soulSelectorUINum; } }
    private void Awake()
    {
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

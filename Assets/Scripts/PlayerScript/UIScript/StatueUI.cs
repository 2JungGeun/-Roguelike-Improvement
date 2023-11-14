using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StatueUI : MonoBehaviour
{
    [SerializeField]
    private GameObject UIprefab;
    [SerializeField]
    private Transform parents;
    private List<GameObject> soulSelectorUI = new List<GameObject>();
    [SerializeField]
    // Start is called before the first frame update
    void Awake()
    {
        for(int i = 0; i < UIManager.GetUIManager().SoulSeclectorUINum; i++)
        {
            soulSelectorUI.Add(Instantiate(UIprefab, parents) as GameObject);
            soulSelectorUI[i].GetComponent<SoulSelectorUI>().Initailize(i);
        }
    }
    public void Initialize(Statue statue)
    {
        for(int i = 0; i< statue.SoulList.Count; i++)
        {
            soulSelectorUI[i].GetComponent<SoulSelectorUI>().UIUpdate(statue);
        }
    }
}

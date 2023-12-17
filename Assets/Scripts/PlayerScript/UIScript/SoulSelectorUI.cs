using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SoulSelectorUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text name;
    [SerializeField]
    private TMP_Text priceText;
    [SerializeField]
    private GameObject icon;

    private string iconPath = "Sprites/UI/Statue/Icon/";
    private string soundPath = "Sound/UISound/";
    private int index;
    private bool isActivated;
    private Statue currStatue;
    private int price;
    private AudioClip[] audioClips = new AudioClip[2];
    // Start is called before the first frame update
    void Start()
    {
        audioClips[0] = Resources.Load<AudioClip>(soundPath + "SoulSelect");
        audioClips[1] = Resources.Load<AudioClip>(soundPath + "BadClick");
        this.GetComponent<Button>().onClick.AddListener(ModifyPlayerSoul);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Initialize(int index)
    {
        this.index = index;
    }

    public void UIUpdate(Statue statue)
    {
        this.currStatue = statue;
        this.isActivated = statue.SoulList[index].Item2;
        if (isActivated)
        {
            this.name.text = statue.SoulList[index].Item1;
            icon.GetComponent<Image>().sprite = Resources.Load<Sprite>(iconPath + statue.SoulList[index].Item1 + "Icon");
            price = DataManager.Instance().SoulPriceDic[name.text];
            priceText.text = price.ToString();
        }
        else
        {
            this.name.text = "Non Activated";
        }
    }

    public void ModifyPlayerSoul()
    {
        if (isActivated)
        {
            PlayerController controller = GameObject.Find("Player").GetComponent<PlayerController>();
            if (controller.PlayerData.money >= price) 
            {
                currStatue.Audio.clip = audioClips[0];
                currStatue.Audio.Play();
                controller.UseMoney(price);
                controller.ModifySoul(name.text, 1);
                currStatue.SetSoulDisabled(index);
                currStatue.IsActivatedUI = false;
                UIManager.GetUIManager().HideStatueUi();
                currStatue = null;
            }
            else
            {
                currStatue.Audio.clip = audioClips[1];
                currStatue.Audio.Play();
            }
        }
    }
}

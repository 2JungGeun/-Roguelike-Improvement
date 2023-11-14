using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public PlayerController subject;
    [SerializeField]
    private TMP_Text intellectualityText;
    [SerializeField]
    private TMP_Text skill1Text;
    [SerializeField]
    private TMP_Text skill2Text;
    [SerializeField]
    private GameObject SkillIcon1;
    [SerializeField]
    private GameObject SkillIcon2;

    [SerializeField]
    private GameObject mainSoulImage;
    [SerializeField]
    private GameObject subSoulImage;

    [SerializeField]
    private GameObject intellectuality;
    [SerializeField]
    private GameObject[] hp = new GameObject[3];

    private Slider intellectualitySlider;

    [SerializeField]
    private TMP_Text moneyText;

    private float maxIntellectuality = 100.0f;
    // Start is called before the first frame update
    void Start()
    { 
        subject = GameObject.Find("Player").GetComponent<PlayerController>();
        subject.HealthEventHandler += OnHealthNotify;
        subject.SkillCooldownEventHandler += OnSkillCooldownNotify;
        subject.SoulSwapEventHandler += OnSoulSwapNotify;
        subject.MoneyEventHandler += OnMoneyNotify;
        intellectualitySlider = intellectuality.GetComponent<Slider>();
        mainSoulImage.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/UI/PlayerUI/MainSoul/" + subject.CurrSoul.Data.name + "UISprite");
        SkillIcon1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/UI/PlayerUI/SkillIcon/" + subject.OwnSouls[subject.MainIndex].Data.name + "SkillIcon1");
        SkillIcon2.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/UI/PlayerUI/SkillIcon/" + subject.OwnSouls[subject.MainIndex].Data.name + "SkillIcon2");
        moneyText.text = subject.PlayerData.money.ToString();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnHealthNotify()
    {
        intellectualityText.text = subject.PlayerData.intellectuality.ToString();
        intellectualitySlider.value = subject.PlayerData.intellectuality / maxIntellectuality;
        foreach (var obj in hp)
            obj.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        for (int i = 0; i < 3 - subject.PlayerData.hp; i++)
            hp[2 - i].GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    }

    public void OnSkillCooldownNotify()
    {
        if (subject.CurrSoul.Skills[KeyCode.X].CurrTime != 0.0f)
        {
            skill1Text.gameObject.SetActive(true);
            skill1Text.text = (subject.CurrSoul.Skills[KeyCode.X].Cooldown - subject.CurrSoul.Skills[KeyCode.X].CurrTime).ToString("F1");
            SkillIcon1.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.3f);
        }
        else
        {
            skill1Text.text = 0.0f.ToString("F1");
            SkillIcon1.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            skill1Text.gameObject.SetActive(false);
        }
        if (subject.CurrSoul.Skills[KeyCode.C].CurrTime != 0.0f)
        {
            skill2Text.gameObject.SetActive(true);
            skill2Text.text = (subject.CurrSoul.Skills[KeyCode.C].Cooldown - subject.CurrSoul.Skills[KeyCode.C].CurrTime).ToString("F1");
            SkillIcon2.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.3f);
        }
        else
        {
            skill2Text.text = 0.0f.ToString("F1");
            SkillIcon2.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            skill2Text.gameObject.SetActive(false);
        }
    }

    public void OnSoulSwapNotify()
    {
        mainSoulImage.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/UI/PlayerUI/MainSoul/" + subject.OwnSouls[subject.MainIndex].Data.name + "UISprite");
        subSoulImage.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/UI/PlayerUI/SubSoul/" + subject.OwnSouls[subject.SubIndex].Data.name + "UISprite");
        SkillIcon1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/UI/PlayerUI/SkillIcon/" + subject.OwnSouls[subject.MainIndex].Data.name + "SkillIcon1");
        SkillIcon2.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/UI/PlayerUI/SkillIcon/" + subject.OwnSouls[subject.MainIndex].Data.name + "SkillIcon2");
        skill1Text.text = 0.0f.ToString("F1");
        skill2Text.text = 0.0f.ToString("F1");
    }

    public void OnMoneyNotify()
    {
        moneyText.text = subject.PlayerData.money.ToString();
    }
}

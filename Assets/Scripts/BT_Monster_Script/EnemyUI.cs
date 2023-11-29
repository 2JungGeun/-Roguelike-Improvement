using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class EnemyUI : MonoBehaviour
{
    private BossScript bossScript;
    [SerializeField]
    private Slider intellectualitySlider;

    void Update()
    {
        transform.position = Camera.main.WorldToScreenPoint(new Vector3(bossScript.transform.position.x, bossScript.transform.position.y + 7.0f, 0));
    }

    public void Initialze(string name)
    {
        bossScript = GameObject.Find(name).GetComponent<BossScript>();
        bossScript.HealthEventHandler += OnHealthNotify;
    }

    public void OnHealthNotify()
    {
        intellectualitySlider.value = bossScript.Data.hp / 1000.0f;
    }
}

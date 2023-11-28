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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Camera.main.WorldToScreenPoint(new Vector3(bossScript.transform.position.x, bossScript.transform.position.y + 4.0f, 0));
    }

    public void Initialze(string name)
    {
        bossScript = GameObject.Find(name).GetComponent<BossScript>();
        bossScript.HealthEventHandler += OnHealthNotify;
    }

    public void OnHealthNotify()
    {
        intellectualitySlider.value = bossScript.HP / 1000.0f;
    }
}

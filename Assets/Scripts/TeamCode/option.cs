using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class option : MonoBehaviour
{
    bool visible = false;
    public GameObject optionObj;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(visible == false)
            {     
                optionObj.gameObject.SetActive(true);
                visible = true;
            }
            else
            {     
                optionObj.gameObject.SetActive(false);
                visible = false;
            }
        }
    }
}

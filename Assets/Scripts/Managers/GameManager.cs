using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{ 
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("GameManager");
    }

    public void CreateGame()
    {
        Debug.Log("���� ����");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

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
        Debug.Log("게임 생성");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

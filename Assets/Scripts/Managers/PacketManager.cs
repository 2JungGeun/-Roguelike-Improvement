using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketManager : Singleton<PacketManager>
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("PacketManager");
    }

    public void CreatePacket()
    {
        Debug.Log("패킷 생성");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

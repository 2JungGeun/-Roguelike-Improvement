using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.IO;
using System;
public class DataManager : Singleton<DataManager>
{
    private string soulDataPath = "Database/SoulData";
    private string soulPricePath = "Database/SoulPrice";
    private Dictionary<string, SoulData> soulDataDic = new Dictionary<string, SoulData>();
    public Dictionary<string, SoulData> SoulDataDic { get { return soulDataDic; } }
    private Dictionary<string, int> soulPriceDic = new Dictionary<string, int>();
    public Dictionary<string, int> SoulPriceDic { get { return soulPriceDic; } }
    private List<string> soulList = new List<string>();
    public List<string> SoulList { get { return soulList; } }

    private new void Awake()
    {
        base.Awake();
        LoadBaseData();
    }

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void LoadBaseData()
    {
        List<Dictionary<string, object>> soulData = CSVReader.Read(soulDataPath);
        for(int i = 0; i <soulData.Count; i++)
        {
            soulDataDic.Add(soulData[i]["Name"].ToString(), 
                new SoulData(soulData[i]["ID"].ToString(),soulData[i]["Name"].ToString(), soulData[i]["Speed"].ToString(), soulData[i]["Damage"].ToString(),
                soulData[i]["Range"].ToString(), soulData[i]["IsUseDash"].ToString(), soulData[i]["AvailableJumpCount"].ToString()));
            soulList.Add(soulData[i]["Name"].ToString());
        }
        List<Dictionary<string, object>> soulPrices = CSVReader.Read(soulPricePath);
        foreach(var soulPrice in soulPrices)
        {
            Int32.TryParse(soulPrice["Price"].ToString(), out int price);
            soulPriceDic.Add(soulPrice["Name"].ToString(), price);
        }
        /*if (!File.Exists(soulDataPath))
            return;
        string text = File.ReadAllText(soulDataPath);
        string[] lines = text.Substring(0,text.Length - 1).Split("\n");
        for (int i = 1; i < lines.Length; i++)
        {
            string[] row = lines[i].Split(',');
            soulDataDic.Add(row[0], new SoulData(row[0], row[1], row[2], row[3], row[4], row[5]));
        }*/

    }
}

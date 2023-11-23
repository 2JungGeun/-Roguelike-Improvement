using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT;

public class BTRunner
{
    private static int nodeCount;
    private BTRoot root;
    public BTRunner(BTRoot node)
    {
        root = node;
        nodeCount = 0;
    }
    public void Initailize()
    {
        root.Initailize(ref nodeCount, null, root);
        Debug.Log("Print Node Count : " + nodeCount);
    }
    public void Tick()
    {
        root.RemoveConditions();
        root.CheckConditons();
        root.RunningNode.Tick();
    }
}

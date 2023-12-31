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
    public void Initialize()
    {
        root.Initialize(ref nodeCount, null, root);
        Debug.Log("Print Node Count : " + nodeCount);
    }
    public bool Tick()
    {
        root.CheckConditions();
        root.RunningNode.Tick();
        if(root.RunningNode.Priority == 0 && !root.IsAbort)
        {
            return true;
        }
        return false;
    }
}

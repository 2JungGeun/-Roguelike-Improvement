using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT;

public class BossScript : MonoBehaviour
{
    BossAction action;
    BTRunner bt;

    private void Start()
    {
        action = new BossAction();
        bt = new BTRunner
        (
            new BTRoot
            (
                new BTSequenceNode
                (
                    new List<BTNode>()
                    {
                        new BTSequenceNode
                        (
                            new List<BTNode>()
                            {
                                new BTActionNode(action.Print),
                                new BTActionNode(action.Wait),
                            }
                         ),
                        new BTActionNode(action.Print),
                        new BTActionNode(action.Wait),
                    }
                )
            )
        );
        bt.Initailize();
    }

    // Update is called once per frame
    void Update()
    {
        bt.Tick();
    }
}

public class BossAction
{
    private static float time = 0.0f;

    public NodeState Print()
    {
        Debug.Log("print3");
        return NodeState.SUCCESS;
    }

    public NodeState Wait()
    {
        time += Time.deltaTime;
        if (time <= 2.0f)
            return NodeState.RUNNING;
        time = 0.0f;
        Debug.Log("wait end");
        return NodeState.SUCCESS;
    }
}


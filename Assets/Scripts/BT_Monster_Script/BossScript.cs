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
                        new BTConditionalDecoratorNode(action.Condition, eAbortType.LOWPRIORITY,
                            new BTSelectorNode
                            (
                                new List<BTNode>()
                                {
                                    new BTActionNode(action.Print),
                                    new BTActionNode(action.Wait)
                                }
                            )
                        ),
                        new BTSequenceNode
                        (
                            new List<BTNode>()
                            {
                                new BTActionNode(action.Print),
                                new BTActionNode(action.Wait)
                            }
                        ),
                        new BTSelectorNode
                        (
                            new List<BTNode>()
                            {
                                new BTConditionalDecoratorNode(action.Condition, eAbortType.SELF,
                                    new BTActionNode(action.Wait)),
                                new BTActionNode(action.Wait)
                            }
                        )  
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
    private float time = 0.0f;
    public eNodeState Print()
    {
        int num = Random.Range(0, 10);
        Debug.Log("print 积己等 箭磊 : " + num);
        if (num % 2 == 0)
            return eNodeState.SUCCESS;
        else
            return eNodeState.FAILURE;
    }

    public eNodeState Wait()
    {
        time += Time.deltaTime;
        if (time <= 2.0f)
            return eNodeState.RUNNING;
        time = 0.0f;
        Debug.Log("wait end");
        return eNodeState.SUCCESS;
    }

    public bool Condition()
    {
        int condition = Random.Range(0, 10);
        Debug.Log("condition 积己等 箭磊 : " + condition);
        if (condition % 2 == 0)
            return true;
        else
            return false;
    }
}


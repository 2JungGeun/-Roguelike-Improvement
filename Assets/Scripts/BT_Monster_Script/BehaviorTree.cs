using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BT
{
    public enum NodeState
    {
        SUCCESS,
        FAILURE,
        RUNNING
    }

    public enum AbortType
    {
        NONE,
        SELF,
        LOWPRIORITY,
        BOTH
    }
    
    public abstract class BTNode
    {
        protected BTRoot root;
        protected BTNode prevNode;
        protected int priority;
        private NodeState state;
        public NodeState State { get { return state; } }
        public BTNode() { }
        public abstract NodeState Tick();
        public abstract void Initailize(ref int priority, BTNode prev, BTRoot root);
    }

    public class BTRoot : BTNode
    {
        private List<BTConditionalDecoratorNode> conditonNodes;
        private BTNode currNode;
        public BTNode CurrNode { get { return currNode; } set { currNode = value; } }
        private BTNode child;
        public BTRoot(BTNode node)
        {
            this.child = node;
        }

        public override void Initailize(ref int priority, BTNode prev, BTRoot root)
        {
            this.priority = priority;
            priority++;
            child.Initailize(ref priority, this, this);
        }
        public void CheckConditons()
        {

        }
        public void AddConditionNode(BTConditionalDecoratorNode node)
        {
            this.conditonNodes.Add(node);
        }
        public override NodeState Tick()
        {
            return child.Tick();
        }

    }

    public class BTActionNode : BTNode
    {
        private Func<NodeState> tick;
        public BTActionNode(Func<NodeState> func)
        {
            tick = func;
        }
        public override NodeState Tick()
        {
            Debug.Log("Action " + priority);
            return tick?.Invoke() ?? NodeState.FAILURE;
        }
        public override void Initailize(ref int priority, BTNode prev, BTRoot root)
        {
            this.priority = priority;
            this.prevNode = prev;
            this.root = root;
        }
    }

    public class BTConditionalDecoratorNode : BTNode
    {
        private Func<bool> tick;
        private BTNode child;
        AbortType abortType;
        public BTConditionalDecoratorNode(Func<bool> func, BTNode node, AbortType conditonalAbort)
        {
            tick = func;
            this.child = node;
            this.abortType = conditonalAbort;
        }
        public override NodeState Tick()
        {
            if(tick())
            {
                switch (abortType)
                {
                    case AbortType.NONE:
                        break;
                    case AbortType.LOWPRIORITY:
                    case AbortType.SELF:
                    case AbortType.BOTH:
                        root.AddConditionNode(this);

                        break;
                    default:
                        break;
                }
                return child.Tick();
            }
            else
            {
                return NodeState.FAILURE;
            }
        }
        public override void Initailize(ref int priority, BTNode prev, BTRoot root)
        {
            this.priority = priority;
            this.prevNode = prev;
            this.root = root;
            priority++;
            child.Initailize(ref priority, this, root);
        }
    }

    public class BTConditionalLoopNode : BTNode
    {
        private Func<bool> tick;
        private BTNode node;
        public BTConditionalLoopNode(Func<bool> func, BTNode node)
        {
            tick = func;
            this.node = node;
        }

        public override NodeState Tick()
        {
            if (tick == null)
                return NodeState.FAILURE;

            while (tick())
            {
                node.Tick();
            }
            return NodeState.SUCCESS;       
        }
        public override void Initailize(ref int priority, BTNode prev, BTRoot root)
        {
            this.priority = priority;
            this.prevNode = prev;
            this.root = root;
            priority++;
            node.Initailize(ref priority, this, root);
        }
    }

    public class BTLoopNode : BTNode
    {
        BTNode node;
        int loopCount;
        public BTLoopNode(int loopCount, BTNode node)
        {
            this.loopCount = loopCount;
            this.node = node;
        }
        public override NodeState Tick()
        {
            for(int i = 0; i < loopCount; i++)
            {
                node.Tick();
            }
            return NodeState.SUCCESS;
        }
        public override void Initailize(ref int priority, BTNode prev, BTRoot root)
        {
            this.priority = priority;
            this.prevNode = prev;
            this.root = root;
            priority++;
            node.Initailize(ref priority, this, root);
        }
    }

    public abstract class BTCompositeNode : BTNode
    {
        protected List<BTNode> children;
        protected BTNode currNode;
        protected int index = 0;
        public BTCompositeNode(List<BTNode> nodes)
        {
            this.children = nodes;
            this.currNode = nodes[0];
        }
        public abstract override NodeState Tick();
        public override void Initailize(ref int priority, BTNode prev, BTRoot root)
        {
            this.priority = priority;
            this.prevNode = prev;
            this.root = root;
            foreach (BTNode node in children)
            {
                priority++;
                node.Initailize(ref priority, this, root);
            }
        }
    }

    public class BTSequenceNode : BTCompositeNode
    {
        public BTSequenceNode(List<BTNode> nodes) : base(nodes) { }
        public override NodeState Tick()
        {
            while (index < children.Count)
            {
                Debug.Log("Sequence " + priority);
                NodeState retval = children[index].Tick();
                if (retval == NodeState.SUCCESS)
                {
                    index++;
                    currNode = children[index];
                }
                else if (retval == NodeState.FAILURE)
                {
                    index = 0;
                    currNode = children[index];
                    return NodeState.FAILURE;
                }
                else if (retval == NodeState.RUNNING)
                {
                    return NodeState.RUNNING;
                }
            }
            index = 0;
            currNode = children[index];
            return NodeState.SUCCESS;
        }
    }

    public class BTSelectorNode : BTCompositeNode
    {
        public BTSelectorNode(List<BTNode> nodes) : base(nodes) { }
        public override NodeState Tick()
        {
            while (index < children.Count)
            {
                Debug.Log(priority);
                NodeState retval = children[index].Tick();
                if (retval == NodeState.SUCCESS)
                {
                    index = 0;
                    currNode = children[index];
                    return NodeState.SUCCESS;
                }
                else if (retval == NodeState.FAILURE)
                {
                    index++;
                    currNode = children[index];
                }
                else if (retval == NodeState.RUNNING)
                {
                    return NodeState.RUNNING;
                }
            }
            index = 0;
            currNode = children[index];
            return NodeState.FAILURE;
        }
    }
}

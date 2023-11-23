using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BT
{
    public enum eNodeState
    {
        SUCCESS,
        FAILURE,
        RUNNING,
        CANCLE
    }

    public enum eAbortType
    {
        NONE,
        SELF,
        LOWPRIORITY,
        BOTH
    }

    public abstract class BTNode
    {
        protected BTRoot root;
        protected BTNode parentNode;
        public BTNode ParaentNode { get { return parentNode; } }
        protected int priority;
        public int Priority { get { return priority; } }
        protected int lastChildPrioriy;
        public int LastChildPrioriy { get { return lastChildPrioriy; } set { lastChildPrioriy = value; } }
        protected eNodeState state;
        public eNodeState State { get { return state; } }
        public BTNode() { }
        public abstract void Initailize(ref int priority, BTNode parent, BTRoot root);
        public abstract void Tick();
        protected abstract void CheckInnerState();
        protected abstract void CanceledByConditionalAborts();
    }

    public class BTRoot : BTNode
    {
        private List<BTConditionalDecoratorNode> conditionNodes;
        private int abortNodePrioriy;
        public int AbortNodePrioriy { get { return abortNodePrioriy; } }
        //private eAbortType abortNodeAbortType;
        //public eAbortType AbortNodeAbortType { get { return abortNodeAbortType; } }
        private BTNode runningNode;
        public BTNode RunningNode { get { return runningNode; } set { runningNode = value; } }
        private BTNode child;
        public bool isConditionChanged = false;
        public BTRoot(BTNode node)
        {
            this.child = node;
            this.runningNode = this;
            conditionNodes = new List<BTConditionalDecoratorNode>();
        }
        public override void Initailize(ref int priority, BTNode parent, BTRoot root)
        {
            this.priority = priority;
            priority++;
            child.Initailize(ref priority, this, this);
            child.LastChildPrioriy = priority;
        }
        public void RemoveConditions()
        {
            if (conditionNodes.Count == 0) return;
            for (int i = conditionNodes.Count - 1; i > -1; i--)
            {
                switch (conditionNodes[i].AbortType)
                {
                    case eAbortType.SELF:
                        if (runningNode.Priority == conditionNodes[i].Priority)
                        {
                            conditionNodes.RemoveAt(conditionNodes.Count - 1);
                            return;
                        }
                        break;
                    case eAbortType.LOWPRIORITY:
                    case eAbortType.BOTH:
                        if (runningNode.Priority < conditionNodes[i].ParaentNode.Priority)
                        {
                            conditionNodes.RemoveAt(conditionNodes.Count - 1);
                        }
                        break;
                }
            }
        }
        public void CheckConditons()
        {
            if (runningNode.Priority == AbortNodePrioriy) isConditionChanged = false;
            if (conditionNodes.Count == 0) return;
            if (isConditionChanged) return;
            for (int i = 0; i < conditionNodes.Count; i++)
            {
                bool condition;
                switch (conditionNodes[i].AbortType)
                {
                    case eAbortType.SELF:
                    case eAbortType.BOTH:
                        condition = conditionNodes[i].Funcion();
                        if (condition != conditionNodes[i].Condition)
                        {
                            isConditionChanged = true;
                            abortNodePrioriy = conditionNodes[i].Priority;
                            conditionNodes.RemoveRange(i, conditionNodes.Count - i);
                            Debug.Log("---------------------------------------Assert------------------------------------------" + conditionNodes.Count);
                            return;
                        }
                        break;
                    case eAbortType.LOWPRIORITY:
                        if (runningNode.Priority > conditionNodes[i].LastChildPrioriy)
                        {
                            condition = conditionNodes[i].Funcion();
                            if (condition != conditionNodes[i].Condition)
                            {
                                Debug.Log(runningNode.Priority + "/" + conditionNodes[i].LastChildPrioriy);
                                isConditionChanged = true;
                                abortNodePrioriy = conditionNodes[i].Priority;
                                conditionNodes.RemoveRange(i, conditionNodes.Count - i);
                                Debug.Log("---------------------------------------Assert------------------------------------------" + conditionNodes.Count);
                                return;
                            }
                        }
                        break;
                }
            }
        }
        public void AddConditionNode(BTConditionalDecoratorNode node)
        {
            this.conditionNodes.Add(node);
        }
        public override void Tick()
        {
            CheckInnerState();
            runningNode = child;
            Debug.Log("root tick" + priority);
        }
        protected override void CheckInnerState()
        {
            Debug.Log("root func" + priority);
        }
        protected override void CanceledByConditionalAborts() { }
    }

    public class BTActionNode : BTNode
    {
        private Func<eNodeState> tick;
        public BTActionNode(Func<eNodeState> func)
        {
            tick = func;
        }
        public override void Initailize(ref int priority, BTNode parent, BTRoot root)
        {
            this.priority = priority;
            this.parentNode = parent;
            this.root = root;
        }
        public override void Tick()
        {
            CheckInnerState();
            switch (state)
            {
                case eNodeState.SUCCESS:
                case eNodeState.FAILURE:
                case eNodeState.CANCLE:
                    Debug.Log("Action tick" + priority + "/" + lastChildPrioriy);
                    root.RunningNode = parentNode;
                    break;
                case eNodeState.RUNNING:
                default:
                    break;
            }
        }
        protected override void CheckInnerState()
        {
            if (root.isConditionChanged)
            {
                state = eNodeState.CANCLE;
                Debug.Log("ActionNode Cancled");
                return;
            }
            state = tick?.Invoke() ?? eNodeState.FAILURE;
        }
        protected override void CanceledByConditionalAborts() { }
    }

    public class BTConditionalDecoratorNode : BTNode
    {
        private Func<bool> tick;
        public Func<bool> Funcion { get { return tick; } }
        private bool condition;
        public bool Condition { get { return condition; } }
        private BTNode child;
        private eAbortType abortType;
        public eAbortType AbortType { get { return abortType; } }
        public BTConditionalDecoratorNode() { }
        public BTConditionalDecoratorNode(Func<bool> func, eAbortType conditonalAbort, BTNode node)
        {
            tick = func;
            this.child = node;
            this.abortType = conditonalAbort;
        }
        public BTConditionalDecoratorNode Deepcopy()
        {
            BTConditionalDecoratorNode temp = new BTConditionalDecoratorNode();
            temp.parentNode = this.parentNode;
            temp.priority = this.priority;
            temp.lastChildPrioriy = this.lastChildPrioriy;
            temp.tick = this.tick;
            temp.condition = this.condition;
            temp.abortType = this.abortType;
            return temp;
        }
        public override void Initailize(ref int priority, BTNode parent, BTRoot root)
        {
            this.priority = priority;
            this.parentNode = parent;
            this.root = root;
            priority++;
            child.Initailize(ref priority, this, root);
            child.LastChildPrioriy = priority;
        }
        public override void Tick()
        {
            CheckInnerState();
            switch (state)
            {
                case eNodeState.RUNNING:
                    condition = tick?.Invoke() ?? false;
                    Debug.Log("conditonNode " + priority + "/" + lastChildPrioriy);
                    if (!condition)
                    {
                        root.RunningNode = parentNode;
                        state = eNodeState.FAILURE;
                        return;
                    }
                    switch (abortType)
                    {
                        case eAbortType.NONE:
                            break;
                        case eAbortType.LOWPRIORITY:
                        case eAbortType.SELF:
                        case eAbortType.BOTH:
                            root.AddConditionNode(this.Deepcopy());
                            break;
                        default:
                            break;
                    }
                    root.RunningNode = child;
                    return;
                    break;
                case eNodeState.SUCCESS:
                case eNodeState.FAILURE:
                    root.RunningNode = parentNode;
                    Debug.Log("condition returned");
                    break;
                case eNodeState.CANCLE:
                    CanceledByConditionalAborts();
                    break;
                default:
                    break;
            }
        }
        protected override void CheckInnerState()
        {
            if (root.isConditionChanged)
            {
                state = eNodeState.CANCLE;
                return;
            }
            if (state != eNodeState.RUNNING)
            {
                state = eNodeState.RUNNING;
                return;
            }
            state = child.State;
        }
        protected override void CanceledByConditionalAborts()
        {
            if (root.AbortNodePrioriy == priority)
                root.RunningNode = this;
            else
                root.RunningNode = parentNode;
        }
    }

    /*    public class BTConditionalLoopNode : BTNode
        {
            private Func<bool> tick;
            private BTNode node;
            public BTConditionalLoopNode(Func<bool> func, BTNode node)
            {
                tick = func;
                this.node = node;
            }

            public override eNodeState Tick()
            {
                if (tick == null)
                    return eNodeState.FAILURE;

                while (tick())
                {
                    node.Tick();
                }
                return eNodeState.SUCCESS;       
            }
            public override void Initailize(ref int priority, BTNode parent, BTRoot root)
            {
                this.priority = priority;
                this.parentNode = parent;
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
            public override eNodeState Tick()
            {
                for(int i = 0; i < loopCount; i++)
                {
                    node.Tick();
                }
                return eNodeState.SUCCESS;
            }
            public override void Initailize(ref int priority, BTNode parent, BTRoot root)
            {
                this.priority = priority;
                this.parentNode = parent;
                this.root = root;
                priority++;
                node.Initailize(ref priority, this, root);
            }
        }*/

    public abstract class BTCompositeNode : BTNode
    {
        protected List<BTNode> children;
        protected int index;
        public BTCompositeNode(List<BTNode> nodes)
        {
            this.children = nodes;
            this.index = -1;
        }
        public override void Initailize(ref int priority, BTNode parent, BTRoot root)
        {
            this.priority = priority;
            this.parentNode = parent;
            this.root = root;
            foreach (BTNode node in children)
            {
                priority++;
                node.Initailize(ref priority, this, root);
                node.LastChildPrioriy = priority;
            }
        }
        public abstract override void Tick();
        protected abstract override void CheckInnerState();
        protected override void CanceledByConditionalAborts()
        {
            if (root.AbortNodePrioriy > this.priority)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i].Priority <= root.AbortNodePrioriy) continue;
                    else
                    {
                        root.RunningNode = children[i - 1];
                        index = i - 1;
                        state = eNodeState.RUNNING;
                        return;
                    }
                }
            }
            root.RunningNode = parentNode;
            reset(eNodeState.CANCLE);
        }
        protected void reset(eNodeState state)
        {
            this.index = -1;
            this.state = state;
        }
    }

    public class BTSequenceNode : BTCompositeNode
    {
        public BTSequenceNode(List<BTNode> nodes) : base(nodes) { state = eNodeState.SUCCESS; }
        public override void Tick()
        {
            CheckInnerState();
            Debug.Log("Sequence tick" + priority + "/" + lastChildPrioriy);
            switch (state)
            {
                case eNodeState.RUNNING:
                    index++;
                    if (index == children.Count)
                    {
                        root.RunningNode = parentNode;
                        reset(eNodeState.SUCCESS);
                        return;
                    }
                    root.RunningNode = children[index];
                    break;
                case eNodeState.FAILURE:
                    reset(eNodeState.FAILURE);
                    root.RunningNode = parentNode;
                    break;
                case eNodeState.CANCLE:
                    CanceledByConditionalAborts();
                    break;
                default:
                    break;
            }
        }
        protected override void CheckInnerState()
        {
            if (root.isConditionChanged)
            {
                state = eNodeState.CANCLE;
                return;
            }
            if (state != eNodeState.RUNNING)
            {
                state = eNodeState.RUNNING;
                return;
            }
            switch (children[index].State)
            {
                case eNodeState.SUCCESS:
                    break;
                case eNodeState.FAILURE:
                    state = eNodeState.FAILURE;
                    break;
                case eNodeState.CANCLE:
                    state = eNodeState.CANCLE;
                    break;
                default:
                    break;
            }
        }
    }

    public class BTSelectorNode : BTCompositeNode
    {
        public BTSelectorNode(List<BTNode> nodes) : base(nodes) { state = eNodeState.FAILURE; }
        public override void Tick()
        {
            CheckInnerState();
            Debug.Log("Selector tick" + priority + "/" + lastChildPrioriy);
            switch (state)
            {
                case eNodeState.RUNNING:
                    index++;
                    if (index == children.Count)
                    {
                        root.RunningNode = parentNode;
                        reset(eNodeState.FAILURE);
                        return;
                    }
                    root.RunningNode = children[index];
                    break;
                case eNodeState.SUCCESS:
                    root.RunningNode = parentNode;
                    reset(eNodeState.SUCCESS);
                    break;
                case eNodeState.CANCLE:
                    Debug.Log("Selector Cancle");
                    CanceledByConditionalAborts();
                    break;
                default:
                    break;
            }
        }
        protected override void CheckInnerState()
        {
            if (root.isConditionChanged)
            {
                state = eNodeState.CANCLE;
                return;
            }
            if (state != eNodeState.RUNNING)
            {
                state = eNodeState.RUNNING;
                return;
            }
            switch (children[index].State)
            {
                case eNodeState.SUCCESS:
                    state = eNodeState.SUCCESS;
                    break;
                case eNodeState.FAILURE:
                    break;
                case eNodeState.CANCLE:
                    state = eNodeState.CANCLE;
                    break;
                default:
                    break;
            }
        }
    }
}

//지금은 cancle을 actionNode까지 가야지 확인 가능함.
//

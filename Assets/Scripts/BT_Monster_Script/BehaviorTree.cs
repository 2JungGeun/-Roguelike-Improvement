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
        public abstract void Initialize(ref int priority, BTNode parent, BTRoot root);
        public abstract void Tick();
        protected abstract void CheckInnerState();
        protected abstract void CanceledByConditionalAborts();
    }

    public class BTRoot : BTNode
    {
        private List<BTConditionalDecoratorNode> conditionNodes;
        private int abortNodePrioriy;
        public int AbortNodePrioriy { get { return abortNodePrioriy; } }
        public bool isAbort = false;
        private BTNode runningNode;
        public BTNode RunningNode { get { return runningNode; } set { runningNode = value; } }
        private BTNode child;
        public BTRoot(BTNode node)
        {
            this.child = node;
            this.runningNode = this;
            conditionNodes = new List<BTConditionalDecoratorNode>();
        }
        public override void Initialize(ref int priority, BTNode parent, BTRoot root)
        {
            this.priority = priority;
            priority++;
            child.Initialize(ref priority, this, this);
            child.LastChildPrioriy = priority;
        }
        public override void Tick()
        {
            CheckInnerState();
            runningNode = child;
            //Debug.Log("root tick" + priority);
        }
        protected override void CheckInnerState()
        {
            Debug.Log("root func" + priority);
        }
        protected override void CanceledByConditionalAborts() { }
        public void CheckConditions()
        {
            if (conditionNodes.Count == 0) return;
            if (runningNode.Priority == abortNodePrioriy) 
            {
                isAbort = false;
                return;
            }
            if (isAbort) return;
            for (int i = 0; i < conditionNodes.Count; i++)
            {
                if (conditionNodes[i].Priority >= runningNode.Priority) return;
                switch (conditionNodes[i].AbortType)
                {
                    case eAbortType.SELF:
                        if (conditionNodes[i].LastChildPrioriy < runningNode.Priority) continue;
                        if (isConditionChanged(i)) return;
                        break;
                    case eAbortType.BOTH:
                        if(conditionNodes[i].ParaentNode.LastChildPrioriy > runningNode.Priority)
                            if (isConditionChanged(i)) return;
                        break;
                    case eAbortType.LOWPRIORITY:
                        if (runningNode.Priority > conditionNodes[i].LastChildPrioriy && conditionNodes[i].ParaentNode.LastChildPrioriy > runningNode.Priority)
                            if (isConditionChanged(i)) return;
                        break;
                }           
            }
        }
        private bool isConditionChanged(int index)
        {
            bool condition = conditionNodes[index].Funcion();
            if (condition != conditionNodes[index].Condition)
            {
                isAbort = true;
                abortNodePrioriy = conditionNodes[index].Priority;
                Debug.Log("---------------------------------------Assert------------------------------------------" + conditionNodes.Count);
                return true;
            }
            return false;
        }
        public void AddConditionNode(BTConditionalDecoratorNode node)
        {
            this.conditionNodes.Add(node);
        }
    }

    public class BTActionNode : BTNode
    {
        private Func<eNodeState> tick;
        public BTActionNode(Func<eNodeState> func)
        {
            tick = func;
        }
        public override void Initialize(ref int priority, BTNode parent, BTRoot root)
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
            if (root.isAbort)
            {
                state = eNodeState.CANCLE;
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
        public override void Initialize(ref int priority, BTNode parent, BTRoot root)
        {
            this.priority = priority;
            this.parentNode = parent;
            this.root = root;
            priority++;
            switch (abortType)
            {
                case eAbortType.SELF:
                case eAbortType.LOWPRIORITY:
                case eAbortType.BOTH:
                    this.root.AddConditionNode(this);
                    break;
                default:
                    break;
            }
            child.Initialize(ref priority, this, root);
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
                    root.RunningNode = child;
                    break;
                case eNodeState.SUCCESS:
                case eNodeState.FAILURE:
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
            if (root.isAbort)
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
            root.RunningNode = parentNode;
        }
    }

    public class BTLoopNode : BTNode
    {
        BTNode childNode;
        public BTLoopNode(BTNode node)
        {
            this.childNode = node;
        }
        public override void Tick()
        {
            CheckInnerState();
            if (state == eNodeState.CANCLE)
            {
                CanceledByConditionalAborts();
                return;
            }
            childNode.Tick();
        }
        public override void Initialize(ref int priority, BTNode parent, BTRoot root)
        {
            this.priority = priority;
            this.parentNode = parent;
            this.root = root;
            priority++;
            childNode.Initialize(ref priority, this, root);
            childNode.LastChildPrioriy = priority;
        }
        protected override void CheckInnerState()
        {
            if (root.isAbort)
            {
                state = eNodeState.CANCLE;
            }
        }
        protected override void CanceledByConditionalAborts()
        {
            root.RunningNode = parentNode;
            state = eNodeState.SUCCESS;
        }
    }

    public abstract class BTCompositeNode : BTNode
    {
        protected List<BTNode> children;
        protected int index;
        public BTCompositeNode(List<BTNode> nodes)
        {
            this.children = nodes;
            this.index = -1;
        }
        public override void Initialize(ref int priority, BTNode parent, BTRoot root)
        {
            this.priority = priority;
            this.parentNode = parent;
            this.root = root;
            foreach (BTNode node in children)
            {
                priority++;
                node.Initialize(ref priority, this, root);
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
        protected bool CheckCancleAndRunning()
        {
            if (root.isAbort)
            {
                state = eNodeState.CANCLE;
                return true;
            }
            if (state != eNodeState.RUNNING)
            {
                state = eNodeState.RUNNING;
                return true;
            }
            return false;
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
            if (CheckCancleAndRunning()) return;
            switch (children[index].State)
            {
                case eNodeState.FAILURE:
                    state = eNodeState.FAILURE;
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
                    CanceledByConditionalAborts();
                    break;
                default:
                    break;
            }
        }
        protected override void CheckInnerState()
        {
            if (CheckCancleAndRunning()) return;
            switch (children[index].State)
            {
                case eNodeState.SUCCESS:
                    state = eNodeState.SUCCESS;
                    break;
                default:
                    break;
            }
        }
    }
}
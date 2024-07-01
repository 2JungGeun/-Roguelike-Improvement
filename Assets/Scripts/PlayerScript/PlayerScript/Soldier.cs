using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : Soul
{
    public Soldier() { }

    public Soldier(string name) : base(name)
    {
        skills.Add(KeyAction.FIRST_SKILL, new SoldierSkill1(this));
        skills.Add(KeyAction.SECOND_SKILL, new SoldierSkill2(this));
        state = new SoldierIdleState();
    }

    public override void Start(KeyAction input)
    {
        if (input == KeyAction.LEFTMOVE || input == KeyAction.RIGHTMOVE)
            state = new SoldierWalkState();
        else if (input == KeyAction.NONE)
            state = new SoldierIdleState();
        state.start(this, input);
    }

    override public void Update(KeyAction input)
    {
        base.Update(input);
    }

    override public void FixedUpdate(KeyAction input)
    {
        IsGround(this);
        state.fixedUpdate(this, input);
    }

    public override void SwapingSoul(KeyAction input)
    {
        this.state.end(this, input);
        this.state = new SoldierIdleState();
    }

    public override SoulState StateChanger(State innerState)
    {
        switch (innerState)
        {
            case State.IDLE:
                return new SoldierIdleState();
            case State.WALK:
                return new SoldierWalkState();
            case State.JUMP:
                return new SoldierJumpState();
            case State.FALL:
                return new SoldierFallState();
            case State.DASH:
                return new SoldierDashState();
            case State.BASEATTACK:
                return new SoldierGroundBasicAttackState();
            case State.AIRATTACK:
                return new SoldierAirBasicAttackState();
            case State.SKILL:
                return new SkillAdapterState();
            default:
                return null;
        }
    }
}

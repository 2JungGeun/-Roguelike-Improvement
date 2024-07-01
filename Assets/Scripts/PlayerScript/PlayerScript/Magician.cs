using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magician : Soul
{
    public Magician(string name) : base(name)
    {
        skills.Add(KeyAction.FIRST_SKILL, new MagicianSkill1(this));
        skills.Add(KeyAction.SECOND_SKILL, new MagicianSkill2(this));
        state = new MagicianIdleState();
    }

    public override void Start(KeyAction input)
    {
        if (input == KeyAction.LEFTMOVE || input == KeyAction.RIGHTMOVE)
            state = new MagicianWalkState();
        else if (input == KeyAction.NONE)
            state = new MagicianIdleState();
        state.start(this, input);
    }

    public override void Update(KeyAction input)
    {
        base.Update(input);
    }

    public override void FixedUpdate(KeyAction input)
    {
        IsGround(this);
        state.fixedUpdate(this, input);
    }

    public override SoulState StateChanger(State innerState)
    {
        switch (innerState)
        {
            case State.IDLE:
                return new MagicianIdleState();
            case State.WALK:
                return new MagicianWalkState();
            case State.JUMP:
                return new MagicianJumpState();
            case State.FALL:
                return new MagicianFallState();
            case State.DASH:
                return new MagicianDashState();
            case State.BASEATTACK:
                return new MagicianGroundBasicAttackState();
            case State.AIRATTACK:
                return new MagicianAirBasicAttackState();
            case State.SKILL:
                return new SkillAdapterState();
            default:
                return null;
        }
    }

    public override void SwapingSoul(KeyAction input)
    {
        state.end(this, input);
        state = new MagicianIdleState();
    }
}

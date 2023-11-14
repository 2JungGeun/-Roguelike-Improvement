using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Knight : Soul
{
    private float coolTime = 0.0f;
    public Knight() { }

    public Knight(string name) : base(name)
    {
        skills.Add(KeyCode.X, new KnightSkill1(this));
        skills.Add(KeyCode.C, new KnightSkill2(this));
        state = new KnightIdleState();
    }

    public override void Start(InputManager input)
    {
        if (Mathf.Abs(input.moveDir) > 0)
            state = new KnightWalkState();
        else
            state = new KnightIdleState();
        state.start(this, input);
    }

    override public void Update(InputManager input)
    {
        base.Update(input);
        //dash
        if (!cooldownTime.dashCoolingdown)
        {
            coolTime += Time.deltaTime;
            if (cooldownTime.dashCooldownTime <= coolTime)
            {
                coolTime = 0.0f;
                cooldownTime.dashCoolingdown = true;
            }
        }
    }

    override public void FixedUpdate(InputManager input)
    {
        IsGround(this);
        state.fixedUpdate(this, input);
    }

    public override void SwapingSoul(InputManager input)
    {
        state.end(this, input);
        this.state = new KnightIdleState();
    }

    public override SoulState StateChanger(State innerState)
    {
        switch (innerState)
        {
            case State.IDLE:
                return new KnightIdleState();
            case State.WALK:
                return new KnightWalkState();
            case State.JUMP:
                return new KnightJumpState();
            case State.FALL:
                return new KnightFallState();
            case State.DASH:
                return new KnightDashState();
            case State.BASEATTACK:
                return new KnightGroundBasicAttackState();
            case State.AIRATTACK:
                return new KnightAirBasicAttackState();
            case State.SKILL:
                return new SkillAdapterState();
            default:
                return null;
        }
    }
}
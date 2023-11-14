using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Skill
{
    protected Soul soul;
    protected float cooldown;
    public float Cooldown { get { return cooldown; } }
    private float time;
    public float CurrTime { get { return time; } }
    protected bool isSkillAvailable;
    private int cost;
    public int Cost { get { return cost; } }

    public Skill(Soul soul, float cooldown, int cost)
    {
        this.soul = soul;
        this.cooldown = cooldown;
        this.cost = cost;
        this.isSkillAvailable = true;
        this.time = 0.0f;
    }

    virtual public void start(InputManager input)
    {
        soul.UseCost(this.cost);
    }

    abstract public State handleInput(InputManager input);
    abstract public void update(InputManager input);
    abstract public void fixedUpdate(InputManager input);
    abstract public void end(InputManager input);

    public void ColldownUpdate()
    {
        if (isSkillAvailable) return;
        time += Time.deltaTime;
        if (time >= cooldown)
        {
            isSkillAvailable = true;
            time = 0.0f;
        }
    }

    public bool CanUseSkill(int intellectuality)
    {
        if (this.isSkillAvailable && intellectuality > cost)
            return true;
        else
            return false;
    }
}

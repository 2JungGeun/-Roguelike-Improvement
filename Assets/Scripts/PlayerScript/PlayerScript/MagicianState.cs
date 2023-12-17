using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicianIdleState : IdleState { }

public class MagicianWalkState : WalkState { }

public class MagicianJumpState : JumpState { }

public class MagicianFallState : FallState { }

public class MagicianDashState : DashState { }

public class MagicianGroundBasicAttackState : RangedGroundBasicAttackState
{
    public override void start(Soul soul, InputManager input)
    {
        audioClip = Resources.Load<AudioClip>("Sound/Magician/Attack/Attack0");
        base.start(soul, input);
        attackDelay[0] = attackDelay[1] = attackDelay[2] = 0.417f;
        projectile.Add(Resources.Load<GameObject>("Prefab/Projectile/MagicianBaseProjectile1"));
        projectile.Add(Resources.Load<GameObject>("Prefab/Projectile/MagicianBaseProjectile2"));
        switch (soul.AttackCount)
        {
            case 0:
            case 1:
                projectileIndex = 0;
                break;
            case 2:
                projectileIndex = 1;
                break;
        }
    }
}

public class MagicianAirBasicAttackState : RangedAirBasicAttackState
{

    public override void start(Soul soul, InputManager input)
    {
        delay = 0.25f;
        audioClip = Resources.Load<AudioClip>("Sound/Magician/Attack/Attack1");
        base.start(soul, input);
        projectile = Resources.Load<GameObject>("Prefab/Projectile/MagicianBaseProjectile1");
        Debug.Log(projectile.name);
    }
}

public class MagicianSkill1 : Skill
{
    AudioClip audioClip;
    private GameObject prefab;
    private Vector3 offset;
    private float time;
    public MagicianSkill1(Soul soul) : base(soul, 2.0f, 5)
    {
        prefab = Resources.Load<GameObject>("Prefab/Projectile/MagicianSkill1Projectile");
        audioClip = Resources.Load<AudioClip>("Sound/Magician/Skill1/Attack");
    }

    public override void start(InputManager input)
    {
        base.start(input);
        isSkillAvailable = false;
        time = 0.0f;
        for (int i = 0; i < 3; i++)
        {
            int randomX = Random.Range(-10, 11);
            int randomY = Random.Range(-10, 11);
            offset = new Vector3(soul.MoveData.lookAt * 1.2f + randomX * 0.2f, 1.0f + randomY * 0.2f, 0.0f);
            GameObject.Instantiate(prefab, soul.mTransform.position + offset, Quaternion.identity).GetComponent<GuidedMissile>().Initialize(soul.Data.damage * 3);
        }
        soul.Anime.Play("SKILL1");
        soul.Audio.clip = audioClip;
        soul.Audio.Play();
    }

    public override State handleInput(InputManager input)
    {
        if (time >= 0.5f)
            return State.IDLE;
        return State.NULL;
    }

    public override void update(InputManager input)
    {
        time += Time.deltaTime;
    }

    public override void fixedUpdate(InputManager input) { }
    public override void end(InputManager input) { }
}

public class MagicianSkill2 : Skill
{
    private MagicianSkill2State state;
    public MagicianSkill2(Soul soul) : base(soul, 10.0f, 30) { }

    public override void start(InputManager input)
    {
        base.start(input);
        isSkillAvailable = false;
        state = new MagicianSkill2ChargeState();
        state.start(soul, 0.0f);
    }
    public override State handleInput(InputManager input)
    {
        float i = 0.0f;
        if (state.handleInput(soul) != null)
        {
            i = state.end(soul);
            state = state.handleInput(soul);
            state.start(soul, i);
        }
        if (i == -1.0f)
            return State.IDLE;
        return State.NULL;
    }
    public override void update(InputManager input)
    {
        state.update(soul);
    }
    public override void fixedUpdate(InputManager input) { }
    public override void end(InputManager input)
    {
        state = null;
    }
}

public abstract class MagicianSkill2State 
{
    public abstract void start(Soul soul, float time);
    public abstract MagicianSkill2State handleInput(Soul soul);
    public abstract void update(Soul soul);
    public abstract float end(Soul soul);
}

public class MagicianSkill2ChargeState : MagicianSkill2State
{
    AudioClip audioClip;
    private GameObject prefab;
    private GameObject projectile;
    private Vector3 offset;
    private float time;
    public override void start(Soul soul, float time)
    {
        time = 0.0f;
        audioClip = Resources.Load<AudioClip>("Sound/Magician/Skill2/Charge");
        prefab = Resources.Load<GameObject>("Prefab/Projectile/MagicianSkill2ChargeProjectile");
        offset = new Vector3(soul.MoveData.lookAt * 1.5f, 1.1f, 0.0f);
        projectile = GameObject.Instantiate(prefab, soul.mTransform.position + offset, Quaternion.identity);
        soul.Anime.Play("SKILL2CHARGE");
        soul.Audio.clip = audioClip;
        soul.Audio.Play();
    }
    public override MagicianSkill2State handleInput(Soul soul)
    {
        if (time >= 1.0f || Input.GetKeyUp(KeyCode.C))
        {
            return new MagicianSkill2AttackState();
        }
        return null;
    }
    public override void update(Soul soul)
    {
        time += Time.deltaTime;
        projectile.transform.localScale = new Vector3(1.0f + time, 1.0f + time, 0.0f);
    }
    public override float end(Soul soul)
    {
        GameObject.Destroy(projectile);
        return time;
    }
}

public class MagicianSkill2AttackState : MagicianSkill2State
{
    AudioClip audioClip;
    private Vector3 offset;
    private GameObject prefab;
    float time = 0.0f;
    public override void start(Soul soul, float size)
    {
        offset = new Vector3(soul.MoveData.lookAt * 1.5f, 1.1f, 0.0f);
        audioClip = Resources.Load<AudioClip>("Sound/Magician/Skill2/Charge");
        prefab = Resources.Load<GameObject>("Prefab/Projectile/MagicianSkill2Projectile");
        GameObject.Instantiate(prefab, soul.mTransform.position + offset, Quaternion.identity).GetComponent<GuidedMissileFar>().Initialize(soul.Data.damage * 4, 1.0f + size);
        soul.Anime.Play("SKILL2ATTACK");
        soul.Audio.clip = audioClip;
        soul.Audio.Play();
    }
    public override MagicianSkill2State handleInput(Soul soul)
    {
        if (time >= 0.25f)
            return new MagicianSkill2NullState();
        return null;
    }
    public override void update(Soul soul)
    {
        time += Time.deltaTime;
    }
    public override float end(Soul soul)
    {
        return -1.0f;
    }
}

public class MagicianSkill2NullState : MagicianSkill2State
{
    public override float end(Soul soul) { return 0.0f; }

    public override MagicianSkill2State handleInput(Soul soul) { return null; }

    public override void start(Soul soul, float time) { }

    public override void update(Soul soul) { }
}

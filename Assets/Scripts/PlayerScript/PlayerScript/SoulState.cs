using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum State
{
    IDLE,
    WALK,
    JUMP,
    FALL,
    DASH,
    BASEATTACK,
    AIRATTACK,
    SKILL,
    NULL
}

abstract public class SoulState
{
    protected State innerState = State.NULL;
    virtual public void start(Soul soul, KeyAction input) { }
    abstract public SoulState handleInput(Soul soul, KeyAction input);
    virtual public void update(Soul soul, KeyAction input) { }
    virtual public void fixedUpdate(Soul soul, KeyAction input) { }
    virtual public void end(Soul soul, KeyAction input) { }
}

abstract public class IdleState : SoulState
{
    override public void start(Soul soul, KeyAction input)
    {
        soul.Anime.Play("IDLE");
    }
    override public SoulState handleInput(Soul soul, KeyAction input)
    {
        if (input == KeyAction.JUMP)
        {
            innerState = State.JUMP;
        }
        else if (input == KeyAction.DONWJUMP || !soul.IsOnGround)
        {
            innerState = State.FALL;
        }
        else if (input == KeyAction.LEFTMOVE || input == KeyAction.RIGHTMOVE)
        {
            innerState = State.WALK;
        }
        else if (soul.Data.isUseDash && soul.mCooldownTime.dashCoolingdown && input == KeyAction.DASH)
        {
            innerState = State.DASH;
        }
        else if (input == KeyAction.BASIC_ATTACK)
        {
            innerState = State.BASEATTACK;
        }
        else if (input == KeyAction.FIRST_SKILL || input == KeyAction.SECOND_SKILL)
        {
            if (soul.Skills[input].CanUseSkill())
                innerState = State.SKILL;
        }
        return soul.StateChanger(innerState);
    }
}

abstract public class WalkState : SoulState
{
    AudioClip audioClip;
    override public void start(Soul soul, KeyAction input)
    {
        audioClip = Resources.Load<AudioClip>("Sound/Public/Walk/Walk");
        soul.Audio.clip = audioClip;
        soul.Anime.Play("WALK");
        soul.Audio.Play();
    }

    override public SoulState handleInput(Soul soul, KeyAction input)
    {
        if (input == KeyAction.JUMP)
        {
            innerState = State.JUMP;
        }
        else if (input == KeyAction.DONWJUMP || !soul.IsOnGround)
        {
            innerState = State.FALL;
        }
        else if (input == KeyAction.NONE)
        {
            innerState = State.IDLE;
        }
        else if (soul.Data.isUseDash && soul.mCooldownTime.dashCoolingdown && input == KeyAction.DASH)
        {
            innerState = State.DASH;
        }
        else if (input == KeyAction.BASIC_ATTACK)
        {
            innerState = State.BASEATTACK;
        }
        else if (input == KeyAction.FIRST_SKILL || input == KeyAction.SECOND_SKILL)
        {
            if (soul.Skills[input].CanUseSkill())
                innerState = State.SKILL;
        }
        return soul.StateChanger(innerState);
    }

    override public void update(Soul soul, KeyAction input)
    {
        if(input == KeyAction.LEFTMOVE)
                soul.Sprite.flipX = true;
        else if(input == KeyAction.RIGHTMOVE)
            soul.Sprite.flipX = false;
        soul.MoveData.lookAt = (soul.Sprite.flipX) ? -1 : 1;
        soul.mTransform.position = Vector2.MoveTowards(soul.mTransform.position, soul.mTransform.position + new Vector3(soul.MoveData.lookAt * soul.Data.speed * Time.deltaTime, 0, 0), 0.8f);
    }
    public override void fixedUpdate(Soul soul, KeyAction input)
    {
        
    }

    public override void end(Soul soul, KeyAction input)
    {
        soul.Audio.Stop();
    }
}

abstract public class JumpState : SoulState
{
    AudioClip audioClip;
    override public void start(Soul soul, KeyAction input)
    {
        audioClip = Resources.Load<AudioClip>("Sound/Public/Jump/Jump");
        soul.Audio.clip = audioClip;
        soul.Anime.Play("JUMP");
        soul.IsOnGround = false;
        Jump(soul);
        soul.Rigid.gravityScale = soul.MoveData.generalGravityScale;
        soul.Audio.Play();
    }

    override public SoulState handleInput(Soul soul, KeyAction input)
    {
        if (soul.Rigid.velocity.y < 0)
        {
            innerState = State.FALL;
        }
        else if (input == KeyAction.JUMP && soul.MoveData.jumpCount < soul.Data.availableJumpCount)
        {
            innerState = State.JUMP;
        }
        else if (soul.Data.isUseDash && soul.mCooldownTime.dashCoolingdown && input == KeyAction.DASH)
        {
            innerState = State.DASH;
        }
        else if (input == KeyAction.BASIC_ATTACK)
        {
            innerState = State.AIRATTACK;
        }
        else if (input == KeyAction.FIRST_SKILL || input == KeyAction.SECOND_SKILL)
        {
            if (soul.Skills[input].CanUseSkill())
                innerState = State.SKILL;
        }
        return soul.StateChanger(innerState);
    }

    public override void update(Soul soul, KeyAction input)
    {
        if (input == KeyAction.NONE)
            return;
        if (input == KeyAction.LEFTMOVE)
            soul.Sprite.flipX = true;
        else if (input == KeyAction.RIGHTMOVE)
            soul.Sprite.flipX = false;
        soul.MoveData.lookAt = (soul.Sprite.flipX) ? -1 : 1;
        soul.mTransform.position = Vector2.MoveTowards(soul.mTransform.position, soul.mTransform.position + new Vector3(soul.MoveData.lookAt * soul.Data.speed * Time.deltaTime, 0, 0), 0.8f);
    }
    override public void fixedUpdate(Soul soul, KeyAction input)
    {
        
    }

    override public void end(Soul soul, KeyAction input) { }

    private void Jump(Soul soul)
    {
        soul.Rigid.velocity = new Vector2(soul.Rigid.velocity.x, 0.0f);
        soul.Rigid.AddForce(Vector2.up * soul.MoveData.jumpPower, ForceMode2D.Impulse);
        soul.MoveData.jumpCount++;
    }
}

abstract public class FallState : SoulState
{
    AudioClip audioClip;
    public override void start(Soul soul, KeyAction input)
    {
        soul.Anime.Play("FALL");
        soul.IsOnGround = false;
        soul.Rigid.gravityScale = soul.MoveData.fallGravityScale;
        if (input == KeyAction.DONWJUMP)
            soul.MoveData.jumpCount++;
    }
    override public SoulState handleInput(Soul soul, KeyAction input)
    {
        if (soul.Rigid.velocity.y == 0 && soul.IsOnGround)
        {
            if (input == KeyAction.LEFTMOVE || input == KeyAction.RIGHTMOVE)
                innerState = State.WALK;
            else if (input == KeyAction.NONE)
                innerState = State.IDLE;
        }
        else if (input == KeyAction.JUMP && soul.MoveData.jumpCount < soul.Data.availableJumpCount)
        {
            innerState = State.JUMP;
        }
        else if (soul.Data.isUseDash && soul.mCooldownTime.dashCoolingdown && input == KeyAction.DASH)
        {
            innerState = State.DASH;
        }
        else if (input == KeyAction.BASIC_ATTACK)
        {
            innerState = State.AIRATTACK;
        }
        else if (input == KeyAction.FIRST_SKILL || input == KeyAction.SECOND_SKILL)
        {
            if(soul.Skills[input].CanUseSkill())
                innerState = State.SKILL;
        }
        return soul.StateChanger(innerState);
    }

    public override void update(Soul soul, KeyAction input)
    {
        if (input == KeyAction.NONE)
            return;
        if (input == KeyAction.LEFTMOVE)
            soul.Sprite.flipX = true;
        else if (input == KeyAction.RIGHTMOVE)
            soul.Sprite.flipX = false;
        soul.MoveData.lookAt = (soul.Sprite.flipX) ? -1 : 1;
        soul.mTransform.position = Vector2.MoveTowards(soul.mTransform.position, soul.mTransform.position + new Vector3(soul.MoveData.lookAt * soul.Data.speed * Time.deltaTime, 0, 0), 0.8f);
    }

    public override void fixedUpdate(Soul soul, KeyAction input)
    {
        
    }

    public override void end(Soul soul, KeyAction input)
    {
        if (soul.IsOnGround)
        {
            audioClip = Resources.Load<AudioClip>("Sound/Public/Jump/landing");
            soul.Audio.clip = audioClip;
            soul.Audio.Play();
        }
    }
}

abstract public class DashState : SoulState
{
    protected float dashTime;
    override public void start(Soul soul, KeyAction input)
    {
        dashTime = 0;
        soul.Anime.Play("DASH");
        soul.mCooldownTime.dashCoolingdown = false;
        soul.Rigid.velocity = new Vector2(soul.Rigid.velocity.x, 0.0f);
        soul.Rigid.gravityScale = 0.0f;
    }
    override public SoulState handleInput(Soul soul, KeyAction input)
    {
        if (soul.MoveData.dashTime < dashTime)
        {
            if (soul.IsOnGround)
                innerState = State.IDLE;
            else
                innerState = State.FALL;
        }
        return soul.StateChanger(innerState);
    }
    override public void update(Soul soul, KeyAction input)
    {
        dashTime += Time.deltaTime;
        soul.mTransform.position = Vector2.MoveTowards(soul.mTransform.position, soul.mTransform.position + new Vector3(soul.MoveData.lookAt * soul.MoveData.dashDistance * Time.deltaTime, 0, 0), 0.8f);
    }

    public override void fixedUpdate(Soul soul, KeyAction input)
    {
    }

    override public void end(Soul soul, KeyAction input)
    {
        soul.Rigid.gravityScale = soul.MoveData.fallGravityScale;
    }
}

abstract public class GroundBasicAttackState : SoulState
{
    protected AudioClip audioClip;
    protected float[] attackDelay = new float[3];
    protected float time;
    protected bool isAttack = false;

    override public void start(Soul soul, KeyAction input)
    {
        soul.attacking = true;
        soul.Anime.Play("ATTACK" + soul.AttackCount.ToString());
        soul.Audio.clip = audioClip;
        soul.Audio.Play();
        time = 0.0f;
    }

    override public SoulState handleInput(Soul soul, KeyAction input)
    {
        if (time >= attackDelay[soul.AttackCount])
        {
            if (soul.IsOnGround)
                innerState = State.IDLE;
            else
                innerState = State.FALL;
        }
        return soul.StateChanger(innerState);
    }

    override public void update(Soul soul, KeyAction input)
    {
        time += Time.deltaTime;
    }

    abstract override public void fixedUpdate(Soul soul, KeyAction input);
    override public void end(Soul soul, KeyAction input)
    {
        soul.attacking = false;
        soul.combatAttackTerm = 1.5f;
        soul.AttackCount++;
    }
}

abstract public class AirBasicAttackState : SoulState
{
    protected AudioClip audioClip;
    protected float delay = 0.42f;
    protected float time = 0.0f;
    protected bool isAttack = false;
    public override void start(Soul soul, KeyAction input)
    {
        soul.Anime.Play("AIRATTACK");
        soul.Audio.clip = audioClip;
        soul.Audio.Play();
    }

    public override SoulState handleInput(Soul soul, KeyAction input)
    {
        if (time >= delay)
        {
            if (soul.IsOnGround)
                innerState = State.IDLE;
            else
                innerState = State.FALL;
        }
        if (soul.IsOnGround)
            innerState = State.IDLE;
        return soul.StateChanger(innerState);
    }

    public override void update(Soul soul, KeyAction input)
    {
        time += Time.deltaTime;
    }

    public override void fixedUpdate(Soul soul, KeyAction input)
    {
    }

    public override void end(Soul soul, KeyAction input)
    {
        isAttack = false;
    }
}

abstract public class MeleeGroundBasicAttackState : GroundBasicAttackState
{
    protected Vector2 offset;
    protected Vector2 size;
    public override void start(Soul soul, KeyAction input)
    {
        base.start(soul, input);
    }

    public override void fixedUpdate(Soul soul, KeyAction input)
    {
        if (time >= (attackDelay[soul.AttackCount] * 0.5f) && !isAttack)
        {
            isAttack = createHitbox(soul);
        }
    }

    private bool createHitbox(Soul soul)
    {
        RaycastHit2D[] hits = Physics2D.BoxCastAll(soul.mTransform.position + new Vector3(soul.MoveData.lookAt * offset.x, offset.y, 0), size, 0, Vector2.up, 0, 128);
        if (hits != null)
        {
            foreach (RaycastHit2D hit in hits)
            {
                hit.collider.GetComponent<IMonster>().Hit(soul.Data.damage);
            }
        }
        return true;
    }
}

abstract public class MeleeAirBasicAttackState : AirBasicAttackState
{
    protected Vector2 offset;
    protected Vector2 size;
    public override void fixedUpdate(Soul soul, KeyAction input)
    {
        if (time >= (delay * 0.5f) && !isAttack)
        {
            isAttack = createHitbox(soul);
        }
    }

    private bool createHitbox(Soul soul)
    {
        RaycastHit2D[] hits = Physics2D.BoxCastAll(soul.mTransform.position + new Vector3(soul.MoveData.lookAt * offset.x, offset.y, 0), size, 0, Vector2.up, 0, 128);
        if (hits != null)
        {
            foreach (RaycastHit2D hit in hits)
            {
                hit.collider.gameObject.GetComponent<IMonster>().Hit(soul.Data.damage);
            }
        }
        return true;
    }
}

abstract public class RangedGroundBasicAttackState : GroundBasicAttackState
{
    protected List<GameObject> projectile = new List<GameObject>();
    protected int projectileIndex = 0;
    protected Vector2 direction;
    protected float degree = 0.0f;
    public override void start(Soul soul, KeyAction input)
    {
        base.start(soul, input);
        direction = new Vector2(soul.MoveData.lookAt, 0.0f);
    }

    public override void update(Soul soul, KeyAction input)
    {
        time += Time.deltaTime;
        if (time >= (attackDelay[soul.AttackCount] * 0.5f) && !isAttack)
        {
            isAttack = createProjectile(soul, projectileIndex);
        }
    }

    public override void fixedUpdate(Soul soul, KeyAction input) { }

    protected bool createProjectile(Soul soul, int index)
    {
        GameObject obj = Object.Instantiate(projectile[index], soul.mTransform.position + new Vector3(soul.MoveData.lookAt * soul.Collider.bounds.size.x, soul.Collider.offset.y, 0.0f), Quaternion.identity);
        obj.GetComponent<Projectile>().Initialize(soul.MoveData.lookAt, direction, soul.Data.range, soul.Data.damage);
        return true;
    }
}

abstract public class RangedAirBasicAttackState : AirBasicAttackState
{
    protected GameObject projectile;
    protected Vector2 direction;
    public override void start(Soul soul, KeyAction input)
    {
        base.start(soul, input);
        direction = new Vector2(soul.MoveData.lookAt, 0.0f);
    }

    public override void fixedUpdate(Soul soul, KeyAction input)
    {
        if (time >= (delay * 0.5f) && !isAttack)
        {
            isAttack = createProjectile(soul);
        }
    }

    private bool createProjectile(Soul soul)
    {
        GameObject obj = Object.Instantiate(projectile, soul.mTransform.position + new Vector3(soul.MoveData.lookAt * soul.Collider.bounds.size.x, soul.Collider.offset.y, 0.0f), Quaternion.identity);
        obj.GetComponent<Projectile>().Initialize(soul.MoveData.lookAt, direction, soul.Data.range, soul.Data.damage);
        return true;
    }
}

public class SkillAdapterState : SoulState
{
    Skill skill;
    public override void start(Soul soul, KeyAction input)
    {
        skill = soul.Skills[input];
        skill.start(input);
    }
    public override SoulState handleInput(Soul soul, KeyAction input)
    {
        return soul.StateChanger(skill.handleInput(input));
    }
    public override void update(Soul soul, KeyAction input)
    {
        skill.update(input);
    }
    public override void fixedUpdate(Soul soul, KeyAction input)
    {
        skill.fixedUpdate(input);
    }
    public override void end(Soul soul, KeyAction input)
    {
        skill.end(input);
    }
}

public class HitState : SoulState
{
    float time;
    AudioClip audioClip;
    public override void start(Soul soul, KeyAction input)
    {
        soul.Anime.Play("HIT");
        audioClip = Resources.Load<AudioClip>("Sound/Public/Hit/HPHit");
        soul.Audio.clip = audioClip;
        soul.Audio.Play();
        time = 0.0f;
    }
    public override SoulState handleInput(Soul soul, KeyAction input)
    {
        if (time >= 0.2f)
        {
            if (soul.IsOnGround)
                innerState = State.IDLE;
            else
                innerState = State.FALL;
        }
        return soul.StateChanger(innerState);
    }

    public override void update(Soul soul, KeyAction input)
    {
        time += Time.deltaTime;
    }
}

public class DeadState : SoulState
{
    float time;
    AudioClip audioClip;

    GameObject dead;
    public override void start(Soul soul, KeyAction input)
    {
        time = 0.0f;
        dead = Object.Instantiate(Resources.Load<GameObject>("Prefab/DeadMain"), soul.mTransform.position, soul.mTransform.rotation);
        soul.Rigid.velocity = new Vector3(0.0f, 0.0f, 0.0f);
        dead.GetComponent<Rigidbody2D>().gravityScale = soul.Rigid.gravityScale;
        audioClip = Resources.Load<AudioClip>("Sound/Public/Dead/Dead");
        soul.Audio.clip = audioClip;
        switch (soul.MoveData.lookAt)
        {
            case 1:
                dead.GetComponent<SpriteRenderer>().flipX = false;
                break;
            case -1:
                dead.GetComponent<SpriteRenderer>().flipX = true;
                break;
        }
        soul.Anime.Play("DEAD");
        soul.Audio.Play();
    }

    public override SoulState handleInput(Soul soul, KeyAction input)
    {
        if (4.0f <= time)
            innerState = State.IDLE;
        return soul.StateChanger(innerState);
    }
    public override void update(Soul soul, KeyAction input)
    {
        time += Time.deltaTime;
    }
    public override void end(Soul soul, KeyAction input)
    {
        Object.Destroy(dead);
        SceneManager.LoadScene("SampleScene");
    }
}

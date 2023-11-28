using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightIdleState : IdleState { }

public class KnightWalkState : WalkState { }

public class KnightJumpState : JumpState { }

public class KnightFallState : FallState { }

public class KnightDashState : DashState { }

public class KnightGroundBasicAttackState : MeleeGroundBasicAttackState 
{
    public override void start(Soul soul, InputManager input)
    {
        audioClip = Resources.Load<AudioClip>("Sound/Knight/Attack/Hit/Attack");
        base.start(soul, input);
        attackDelay[0] = 0.42f;
        attackDelay[1] = 0.34f;
        attackDelay[2] = 0.34f;
        offset = new Vector2(1.1f, soul.Collider.offset.y);
        size = new Vector2(3.0f, soul.Collider.bounds.size.y);
    }
}

public class KnightAirBasicAttackState : MeleeAirBasicAttackState
{
    public override void start(Soul soul, InputManager input)
    {
        audioClip = Resources.Load<AudioClip>("Sound/Knight/Attack/NonHit/Attack");
        base.start(soul, input);
        delay = 0.42f;
        offset = new Vector2(1.1f, soul.Collider.offset.y);
        size = new Vector2(3.0f, soul.Collider.bounds.size.y);
    }
}

public class KnightSkill1 : Skill
{
    AudioClip audioClip;
    GameObject prefab;
    private Vector3 startPos = new Vector3();
    private float time;
    private bool isAttacked;

    public KnightSkill1(Soul soul) : base(soul, 3.0f, 10)
    {
        prefab = Resources.Load<GameObject>("Prefab/fireBall");
        audioClip = Resources.Load<AudioClip>("Sound/Knight/Skill/DashSkill");
    }

    public override void start(InputManager input)
    {
        base.start(input);
        isSkillAvailable = false;
        isAttacked = false;
        time = 0.0f;
        startPos = soul.mTransform.position;
        soul.Anime.Play("SKILL1");
        soul.Audio.clip = audioClip;
        soul.Audio.Play();
    }

    public override State handleInput(InputManager input)
    {
        if (time >= 0.4f)
            if (soul.IsOnGround)
                return State.IDLE;
            else
                return State.FALL;
        return State.NULL;
    }

    public override void update(InputManager input)
    {
        time += Time.deltaTime;
    }

    public override void fixedUpdate(InputManager input)
    {

        soul.Rigid.velocity = new Vector2(soul.Rigid.velocity.x, 0.0f);
        if (Vector3.Distance(startPos, soul.mTransform.position) <= 7f)
        {
            soul.mTransform.position = Vector2.MoveTowards(soul.mTransform.position, soul.mTransform.position + new Vector3(soul.MoveData.lookAt * 40.0f * Time.fixedDeltaTime, 0), 1.0f);
        }
        else
        {
            if (!isAttacked)
            {
                CreateHitbox();
                isAttacked = true;
            }
        }

    }

    public override void end(InputManager input)
    {
        if (!isAttacked)
            CreateHitbox();
    }

    private void CreateHitbox()
    {
        float offsetX = (startPos.x + soul.mTransform.position.x) * 0.5f;
        float offsetY = soul.Collider.offset.y;
        float sizeX = Mathf.Abs(startPos.x - soul.mTransform.position.x);
        RaycastHit2D[] hits = Physics2D.BoxCastAll(new Vector2(offsetX, soul.mTransform.position.y + offsetY), new Vector2(sizeX, soul.Collider.bounds.size.y), 0, Vector2.up, 0, (int)Layer.Monster);
        if (hits != null)
        {
            foreach (RaycastHit2D hit in hits)
            {
                hit.collider.GetComponent<BossScript>().Hit(soul.Data.damage * 2);
            }
        }
    }
}

public class KnightSkill2 : Skill
{
    AudioClip[] audioClips = new AudioClip[3];
    private Vector2 offset;
    private Vector2 size;
    private float delay;
    private int attackCount;
    private float time;

    public KnightSkill2(Soul soul) : base(soul, 5.0f, 10)
    {
        offset = new Vector2(1.2f, 1.5f);
        size = new Vector2(2.4f, 3.0f);
        delay = 0.167f;
        audioClips[0] = Resources.Load<AudioClip>("Sound/Knight/Skill/Slash1");
        audioClips[1] = Resources.Load<AudioClip>("Sound/Knight/Skill/Slash2");
        audioClips[2] = Resources.Load<AudioClip>("Sound/Knight/Skill/Slash1");
    }

    public override void start(InputManager input)
    {
        base.start(input);
        isSkillAvailable = false;
        attackCount = 0;
        time = 0.0f;
        soul.Anime.Play("SKILL2");
        soul.Audio.clip = audioClips[attackCount];
        soul.Audio.Play();
        CreateHitbox();
    }

    public override State handleInput(InputManager input)
    {
        if (time >= 0.542f)
            if (soul.IsOnGround)
                return State.IDLE;
            else
                return State.FALL;
        return State.NULL;
    }

    public override void update(InputManager input)
    {
        time += Time.deltaTime;
    }

    public override void fixedUpdate(InputManager input)
    {
        if (time >= delay && attackCount < 2)
        {
            soul.Audio.clip = audioClips[attackCount + 1];
            soul.Audio.Play();
            CreateHitbox();
            attackCount++;
            time = 0.0f;
        }
    }

    public override void end(InputManager input) { }

    private void CreateHitbox()
    {
        RaycastHit2D[] hits = Physics2D.BoxCastAll(soul.mTransform.position + new Vector3(soul.MoveData.lookAt * offset.x, offset.y, 0.0f), size, 0, Vector2.up, 0, (int)Layer.Monster);
        if (hits != null)
        { foreach (RaycastHit2D hit in hits)
            {
                hit.collider.GetComponent<BossScript>().Hit(soul.Data.damage);
            }
        }
    }
}
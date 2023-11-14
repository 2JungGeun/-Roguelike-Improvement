using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierIdleState : IdleState { }

public class SoldierWalkState : WalkState { }

public class SoldierJumpState : JumpState { }

public class SoldierFallState : FallState { }

public class SoldierDashState : DashState { }

public class SoldierGroundBasicAttackState : RangedGroundBasicAttackState 
{
    public override void start(Soul soul, InputManager input)
    {
        audioClip = Resources.Load<AudioClip>("Sound/Soldier/Attack/Attack");
        base.start(soul, input);
        attackDelay[0] = 0.667f;
        attackDelay[1] = 0.667f;
        attackDelay[2] = 0.333f;
        projectile.Add(Resources.Load<GameObject>("Prefab/Projectile/SoldierProjectile1"));
        projectile.Add(Resources.Load<GameObject>("Prefab/Projectile/SoldierProjectile2"));
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
        createProjectile(soul, projectileIndex);
    }
    public override void update(Soul soul, InputManager input)
    {
        time += Time.deltaTime;
        switch (soul.AttackCount)
        {
            case 0:
            case 1:
                if (time >= (attackDelay[soul.AttackCount] * 0.5f) && !isAttack)
                {
                    isAttack = createProjectile(soul, projectileIndex);
                    soul.Audio.clip = audioClip;
                    soul.Audio.Play();
                }
                break;
            default:
                break;

        }
    }
}

public class SoldierAirBasicAttackState : RangedAirBasicAttackState
{
    public override void start(Soul soul, InputManager input)
    {
        audioClip = Resources.Load<AudioClip>("Sound/Soldier/Attack/Attack");
        base.start(soul, input);
        projectile = Resources.Load<GameObject>("Prefab/Projectile/SoldierProjectile1");
        Debug.Log(projectile.name);
    }
}

public class Gun
{
    public GameObject prefab;
    public GameObject gun;
    public float degree;
    public Vector2 direction;

    public Gun(GameObject gun, Vector2 dir)
    {
        this.gun = gun;
        this.degree = 0.0f;
        this.direction = dir;
    }
}

public class SoldierSkill1 : Skill
{

    GameObject gunPrefab;
    Vector3 offset;
    private Gun gun;
    SoldierSkillState gunSoldierState;
    public SoldierSkill1(Soul soul) : base(soul, 10.0f, 20)
    {
        gun = new Gun(null, Vector2.right);
        gunPrefab = Resources.Load<GameObject>("Prefab/SoldierGun");
        gun.prefab = Resources.Load<GameObject>("Prefab/Projectile/SoldierProjectile2");

    }

    public override void start(InputManager input)
    {
        base.start(input);
        soul.Anime.Play("SKILL1PREPARE");
        offset = new Vector3(soul.MoveData.lookAt * -0.4f, 0.69f, 0.0f);
        GameObject obj = GameObject.Instantiate(gunPrefab, offset, Quaternion.identity);
        obj.transform.localScale = new Vector3(soul.MoveData.lookAt, 1.0f, 1.0f);
        obj.transform.SetParent(soul.mTransform, false);
        gun.gun = obj;
        gun.direction = new Vector2(soul.MoveData.lookAt, 0.0f);
        gunSoldierState = new SoldierSkillIdleState();
        gunSoldierState.start(soul, gun);
    }

    public override State handleInput(InputManager input)
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            isSkillAvailable = false;
            input.reset();
            return State.IDLE;
        }
        if (gunSoldierState.handleInput(soul, gun) != null)
        {
            gunSoldierState.end(soul, gun);
            gunSoldierState = gunSoldierState.handleInput(soul, gun);
            gunSoldierState.start(soul, gun);
        }
        return State.NULL;
    }

    public override void fixedUpdate(InputManager input)
    {
        gunSoldierState.fixedUpdate(soul, gun);
    }

    public override void update(InputManager input)
    {
        gunSoldierState.update(soul, gun);
    }

    public override void end(InputManager input)
    {
        isSkillAvailable = false;
        gunSoldierState = null;
        GameObject.Destroy(gun.gun);
        gun.gun = null;
    }
}

public abstract class SoldierSkillState
{
    public abstract void start(Soul soul, Gun gun);
    public abstract SoldierSkillState handleInput(Soul soul, Gun gun);
    public virtual void update(Soul soul, Gun gun)
    {
        switch (Input.GetAxisRaw("Vertical"))
        {
            case 1:
                if (gun.degree < 25)
                    gun.degree += 20.0f * Time.deltaTime;
                break;
            case -1:
                if (gun.degree > -25)
                    gun.degree -= 20.0f * Time.deltaTime;
                break;
        }
        gun.direction.x = soul.MoveData.lookAt * Mathf.Abs(Mathf.Cos(Mathf.Deg2Rad * gun.degree));
        gun.direction.y = Mathf.Sin(Mathf.Deg2Rad * gun.degree);
        gun.gun.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, gun.degree * soul.MoveData.lookAt));
    }

    public virtual void fixedUpdate(Soul soul, Gun gun) { }

    public virtual void end(Soul soul, Gun gun) { }
}

public class SoldierSkillIdleState : SoldierSkillState
{
    public override void start(Soul soul, Gun gun)
    {
        soul.Anime.Play("SKILL1IDLE");
        gun.gun.GetComponent<Animator>().Play("IDLE");
    }
    public override SoldierSkillState handleInput(Soul soul, Gun gun)
    {
        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0)
        {
            return new SoldierSkillWalkState();
        }
        else if (Input.GetKey(KeyCode.Z))
        {
            return new SoldierSkillAttackState();
        }
        else if (!soul.IsOnGround)
        {
            return new SoldierSkillFallState();
        }
        return null;
    }
}

public class SoldierSkillWalkState : SoldierSkillState
{
    private float moveDir = 0.0f;
    public override void start(Soul soul, Gun gun)
    {
        soul.Anime.Play("SKILL1WALK");
    }
    public override SoldierSkillState handleInput(Soul soul, Gun gun)
    {
        moveDir = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(moveDir) == 0)
        {
            return new SoldierSkillIdleState();
        }
        else if (Input.GetKey(KeyCode.Z))
        {
            return new SoldierSkillAttackState();
        }
        else if (!soul.IsOnGround)
        {
            return new SoldierSkillFallState();
        }
        return null;
    }
    public override void fixedUpdate(Soul soul, Gun gun)
    {
        soul.mTransform.position = Vector2.MoveTowards(soul.mTransform.position, soul.mTransform.position + new Vector3(moveDir * soul.Data.speed * Time.fixedDeltaTime, 0, 0), 0.8f);
    }
}

public class SoldierSkillAttackState : SoldierSkillState
{
    AudioClip audioClip;
    private float time;
    public override void start(Soul soul, Gun gun)
    {
        audioClip = Resources.Load<AudioClip>("Sound/Soldier/Attack/LastAttack");
        soul.Anime.Play("SKILL1IDLE");
        gun.gun.GetComponent<Animator>().Play("ATTACK");
        soul.Audio.clip = audioClip;
        soul.Audio.Play();
        time = 0.0f;
        createProjectile(soul, gun);
    }
    public override SoldierSkillState handleInput(Soul soul, Gun gun)
    {
        if (time > 0.25f)
            return new SoldierSkillIdleState();
        return null;
    }
    public override void update(Soul soul, Gun gun)
    {
        base.update(soul, gun);
        time += Time.deltaTime;
    }

    private void createProjectile(Soul soul, Gun gun)
    {
        GameObject obj = Object.Instantiate(gun.prefab, gun.gun.transform.GetChild(0).position, gun.gun.transform.rotation);
        obj.GetComponent<Projectile>().Initailize(soul.MoveData.lookAt, gun.direction, soul.Data.range, soul.Data.damage);
    }
}

public class SoldierSkillFallState : SoldierSkillState
{
    public override void start(Soul soul, Gun gun)
    {
        soul.Anime.Play("SKILL1FALL");
    }
    public override SoldierSkillState handleInput(Soul soul, Gun gun)
    {
        if (soul.IsOnGround)
            return new SoldierSkillIdleState();
        return null;
    }
}

public class SoldierSkill2 : Skill
{
    AudioClip audioClip;
    GameObject prefab;
    Vector3 offset;
    float time;
    bool isAttack;
    public SoldierSkill2(Soul soul) : base(soul, 5.0f, 20)
    {
        prefab = Resources.Load<GameObject>("Prefab/SoldierBeam");
        audioClip = Resources.Load<AudioClip>("Sound/Soldier/Skill/Beam");
        time = 0.0f;
    }
    public override void start(InputManager input)
    {
        base.start(input);
        isSkillAvailable = false;
        isAttack = false;
        offset = new Vector3(soul.MoveData.lookAt * 1.0f, 1.0f, 0.0f);
        soul.Anime.Play("SKILL2");
        soul.Audio.clip = audioClip;
        soul.Audio.Play();
    }

    public override State handleInput(InputManager input)
    {
        if (time >= 0.8f)
            return State.IDLE;
        return State.NULL;
    }

    public override void update(InputManager input)
    {
        time += Time.deltaTime;
        if (!isAttack && time >= 0.167f)
        {
            GameObject.Instantiate(prefab, soul.mTransform.position + offset, Quaternion.identity).transform.localScale = new Vector3(soul.MoveData.lookAt, 1.0f, 1.0f);
            isAttack = true;
        }
    }

    public override void fixedUpdate(InputManager input)
    {

    }

    public override void end(InputManager input)
    {
        time = 0.0f;
    }
}


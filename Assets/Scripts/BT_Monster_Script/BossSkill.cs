using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT;
public abstract class BossSkill
{
    protected EnemyData data;
    protected BTRunner bt;
    protected float skillRange;
    protected bool isFinished;
    public float SkillRange { get { return skillRange; } }
    public BossSkill(EnemyData data, float range)
    {
        this.data = data;
        this.skillRange = range;
        this.isFinished = false;
    }
    public abstract bool Tick();
}


public class BossSkill1 : BossSkill
{
    float time = 0.0f;
    float waitTime = 0.0f;
    bool isHit = false;
    public BossSkill1(EnemyData data, float range) : base(data, range) 
    {
        bt = new BTRunner
            (
                new BTRoot
                (
                    new BTSequenceNode
                    (
                        new List<BTNode>()
                        {
                            new BTActionNode(this.ChangeToDashReadyAnimation),
                            new BTActionNode(this.ChangeToDashAnimation),
                            new BTSelectorNode
                            (
                                new List<BTNode>()
                                {
                                    new BTConditionalDecoratorNode(this.IsGotHit, eAbortType.LOWPRIORITY,
                                        new BTSequenceNode
                                        (
                                            new List<BTNode>()
                                            {
                                                new BTActionNode(this.ChangeToStunAnimation),
                                                new BTActionNode(this.Wait)
                                            }
                                        )
                                    ),
                                    new BTActionNode(this.MoveToTarget)
                                }
                            ),
                            new BTActionNode(this.ChangeToAwakeAnimation),
                        }
                    )
                )
            );
        bt.Initialize();
    }
    public override bool Tick()
    {
        return bt.Tick();
    }
    public eNodeState ChangeToDashReadyAnimation()
    {
        data.lookAt = ((data.target.transform.position.x - this.data.mTransform.position.x) > 0.0f) ? 1.0f : -1.0f;
        data.Sprite.flipX = (data.lookAt > 0.0f) ? true : false;
        data.Anime.Play("SKILL1READY");
        return eNodeState.SUCCESS;
    }
    public eNodeState ChangeToDashAnimation()
    {
        if (data.Anime.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0)
        {
            data.Anime.Play("SKILL1DASH");
            return eNodeState.SUCCESS;
        }
        return eNodeState.RUNNING;
    }
    public bool IsGotHit()
    {
        if (isHit) return true;
        RaycastHit2D hit = Physics2D.BoxCast(data.mTransform.position + new Vector3(0, 2.5f, 0), new Vector2(3.0f, 5.0f), 0, Vector2.up, 0, (int)Layer.Player | (int)Layer.Ground);
        if (hit.collider != null)
        {
            isHit = true;
            if (hit.collider.CompareTag("Ground"))
            {
                data.mTransform.position = data.mTransform.position + new Vector3(-1 * data.lookAt, 0, 0);
                return true;
            }
            hit.collider.GetComponent<PlayerController>().Hit(DamageType.INTELLECTUALITY, 20);
            hit.collider.GetComponent<Rigidbody2D>().AddForce(new Vector2(data.lookAt * 10.0f, 0), ForceMode2D.Impulse);
            return true;
        }
        return false;
    }
    public eNodeState ChangeToStunAnimation()
    {
        data.Anime.Play("SKILL1STUN");
        waitTime = 3.0f;
        isHit = false;
        return eNodeState.SUCCESS;
    }
    public eNodeState ChangeToAwakeAnimation()
    {
        data.Anime.Play("SKILL1AWAKE");
        return eNodeState.SUCCESS;
    }

    public eNodeState MoveToTarget()
    {
        this.data.mTransform.position = Vector2.MoveTowards(this.data.mTransform.position, this.data.mTransform.position + new Vector3(Time.deltaTime * data.moveSpeed * 3.0f * data.lookAt, 0.0f, 0.0f), 0.8f);
        return eNodeState.RUNNING;
    }
    public eNodeState Wait()
    {
        time += Time.deltaTime;
        if (time <= waitTime)
            return eNodeState.RUNNING;
        time = 0.0f;
        return eNodeState.SUCCESS;
    }
}


public class BossSkill2 : BossSkill
{
    private GameObject shockwavePrefab;
    float time = 0.0f;
    float waitTime = 0.0f;
    public BossSkill2(EnemyData data, float range) : base(data, range) 
    {
        bt = new BTRunner
        (
            new BTRoot
            (
                new BTSequenceNode
                (
                    new List<BTNode>()
                    {
                        new BTActionNode(this.ChangeToAttackReadyAnimation),
                        new BTActionNode(this.ChangeToAttackAnimation),
                        new BTActionNode(this.CreateShockWave),
                        new BTActionNode(this.Wait),
                        new BTActionNode(this.ChangeToAwakeAnimation),
                    }
                )
            )
        );
        bt.Initialize();
        shockwavePrefab = Resources.Load<GameObject>("Prefab/Projectile/BossShockWave");
    }

    public override bool Tick()
    {
        return bt.Tick();
    }


    public eNodeState ChangeToAttackReadyAnimation()
    {
        data.Anime.Play("SKILL2READY");
        return eNodeState.SUCCESS;
    }

    public eNodeState ChangeToAttackAnimation()
    {
        if (data.Anime.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0)
        {
            data.Anime.Play("SKILL2ATTACK");
            return eNodeState.SUCCESS;
        }
        return eNodeState.RUNNING;
    }

    public eNodeState CreateShockWave()
    {
        GameObject.Instantiate(shockwavePrefab, data.mTransform.position + new Vector3(3.0f, 0, 0), Quaternion.identity);
        GameObject.Instantiate(shockwavePrefab, data.mTransform.position + new Vector3(-3.0f, 0, 0), Quaternion.identity);
        RaycastHit2D hit = Physics2D.BoxCast(data.mTransform.position + new Vector3(0, 0.5f, 0), new Vector2(10, 1), 0, Vector2.up, 0, (int)Layer.Player);
        if(hit.collider != null)
        {
            hit.collider.GetComponent<PlayerController>().Hit(DamageType.INTELLECTUALITY, 20);
            hit.collider.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 30.0f,ForceMode2D.Impulse);
        }
        waitTime = 0.3f;
        return eNodeState.SUCCESS;
    }

    public eNodeState ChangeToAwakeAnimation()
    {
        data.Anime.Play("SKILL2AWAKE");
        return eNodeState.SUCCESS;
    }

    public eNodeState Wait()
    {
        time += Time.deltaTime;
        if (time <= waitTime)
            return eNodeState.RUNNING;
        time = 0.0f;
        return eNodeState.SUCCESS;
    }
}

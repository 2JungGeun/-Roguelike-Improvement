using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT;

public class BossAction
{
    private EnemyData data;
    private List<BossSkill> skills = new List<BossSkill>();
    private BossSkill selectedSkill;
    float animationTime;
    public BossAction(EnemyData data)
    {
        this.data = data;
        skills.Add(new BossSkill1(data, 20.0f));
        skills.Add(new BossSkill2(data, 5.0f));
        //selectedSkill = new BossSkill1(data,10.0f);
    }
    private float time = 0.0f;

    public eNodeState Wait()
    {
        time += Time.deltaTime;
        if (time <= 1.0f)
            return eNodeState.RUNNING;
        time = 0.0f;
        return eNodeState.SUCCESS;
    }

    public bool gotATarget()
    {
        if (data.target == null)
        {
            RaycastHit2D hit = Physics2D.BoxCast(this.data.mTransform.position + new Vector3(0, 3, 0), new Vector2(40.0f, 6.0f), 0, Vector2.up, 0, 8);
            if (hit.collider != null)
            {
                data.target = hit.collider.GetComponent<PlayerController>();
                return true;
            }
            return false;
        }
        return true;
    }

    public bool IsTargetWithinDetectionRange()
    {
        if (Vector2.Distance(data.target.transform.position, this.data.mTransform.position) > 100.0f)
        {
            data.target = null;
            return false;
        }
        return true;
    }

    public bool IsAheadBlocked()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.data.mTransform.position + new Vector3(3.0f * data.lookAt, 1.0f, 0.0f), Vector2.right * data.lookAt, 1.0f, 64);
        if (hit.collider != null)
        {
            return true;
        }
        return false;
    }

    public bool IsWithinAttackRange()
    {
        if (Vector2.Distance(new Vector2(data.target.transform.position.x, 0.0f), new Vector2(this.data.mTransform.position.x, 0.0f)) < selectedSkill.SkillRange)
        {
            return true;
        }
        return false;
    }

    public eNodeState ChangeDirection()
    {
        data.Sprite.flipX = !data.Sprite.flipX;
        data.lookAt *= -1.0f;
        return eNodeState.SUCCESS;
    }

    public eNodeState SelectSkill()
    {
        if (Vector2.Distance(new Vector2(data.target.transform.position.x, 0.0f), new Vector2(this.data.mTransform.position.x, 0.0f)) < 10.0f)
        {
            selectedSkill = skills[1];
        }
        else
        {
            selectedSkill = skills[0];
        }
        return eNodeState.SUCCESS;
    }

    public eNodeState useSkill()
    {
        if(selectedSkill.Tick())
        {
            return eNodeState.SUCCESS;
        }
        return eNodeState.RUNNING;
    }

    public eNodeState MoveForward()
    {
        this.data.mTransform.position = Vector2.MoveTowards(this.data.mTransform.position, this.data.mTransform.position + new Vector3(Time.deltaTime * data.moveSpeed * data.lookAt, 0.0f, 0.0f), 0.8f);
        return eNodeState.RUNNING;
    }

    public eNodeState MoveToTarget()
    {
        data.lookAt = ((data.target.transform.position.x - this.data.mTransform.position.x) > 0.0f) ? 1.0f : -1.0f;
        data.Sprite.flipX = (data.lookAt > 0.0f) ? true : false;
        this.data.mTransform.position = Vector2.MoveTowards(this.data.mTransform.position, this.data.mTransform.position + new Vector3(Time.deltaTime * data.moveSpeed * data.lookAt, 0.0f, 0.0f), 0.8f);
        return eNodeState.RUNNING;
    }

    public eNodeState ChangeToIdleAnimation()
    {
        data.Anime.Play("IDLE");
        return eNodeState.SUCCESS;
    }

    public eNodeState ChangeToWalkAnimation()
    {
        data.Anime.Play("WALK");
        return eNodeState.SUCCESS;
    }

    public eNodeState Print()
    {
        if (data.Anime.GetCurrentAnimatorStateInfo(0).IsName("ATTACK"))
        {
            animationTime = data.Anime.GetCurrentAnimatorStateInfo(0).normalizedTime;
            Debug.Log(animationTime);
        }
        return eNodeState.SUCCESS;
    }
}

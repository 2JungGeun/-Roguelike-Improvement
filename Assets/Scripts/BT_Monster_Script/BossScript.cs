using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT;

public class EnemyData
{
    public float hp;
    public float damage;
    public float moveSpeed;
    public float lookAt;
    public PlayerController target;

}
public class BossScript : MonoBehaviour
{
    [SerializeField]
    private GameObject UIPrefab;
    [SerializeField]
    private GameObject canvas;
    private EnemyData data;
    public EnemyData Data { get { return data; } }
    BossAction action;
    BTRunner bt;
    public delegate void healthEventHandler();
    public healthEventHandler HealthEventHandler;

    void Awake()
    {
        data = new EnemyData();
        data.hp = 1000;
        data.moveSpeed = 30.0f;
        data.lookAt = GetComponent<SpriteRenderer>().flipX ? 1.0f : -1.0f;
        data.target = null;
        Instantiate(UIPrefab, canvas.transform).GetComponent<EnemyUI>().Initialze("Boss");
    }
    void Start()
    {
        action = new BossAction(data, GetComponent<Collider2D>(), GetComponent<Rigidbody2D>(), GetComponent<Transform>(), GetComponent<SpriteRenderer>(), GetComponent<Animator>(), GetComponent<AudioSource>());
        bt = new BTRunner
        (
            new BTRoot
            (
                new BTSelectorNode
                (
                    new List<BTNode>()
                    {
                        new BTConditionalDecoratorNode(action.gotATarget, eAbortType.BOTH,
                            new BTConditionalDecoratorNode(action.IsTargetWithinDetectionRange, eAbortType.SELF,

                                        new BTSelectorNode
                                        (
                                            new List<BTNode>()
                                            {
                                                new BTConditionalDecoratorNode(action.IsWithinAttackRange, eAbortType.BOTH,
                                                    new BTActionNode(action.ChangeToAttackAnimation)
                                                ),
                                                new BTSequenceNode
                                                (
                                                    new List<BTNode>()
                                                    {
                                                        new BTActionNode(action.ChangeToWalkAnimation),
                                                        new BTLoopNode
                                                        (
                                                            new BTActionNode(action.MoveToTarget)
                                                        )
                                                        
                                                    }
                                                )
                                            }
                                        )
                            )
                        ),
                        new BTSelectorNode
                        (
                            new List<BTNode>()
                            {
                                new BTConditionalDecoratorNode(action.IsAheadBlocked, eAbortType.LOWPRIORITY,
                                    new BTActionNode(action.ChangeDirection)
                                ),
                                new BTSequenceNode
                                (
                                    new List<BTNode>()
                                    {
                                        new BTActionNode(action.ChangeToWalkAnimation),
                                        new BTLoopNode
                                        (
                                            new BTSequenceNode
                                            (
                                                new List<BTNode>
                                                {
                                                new BTActionNode(action.FindTarget),
                                                new BTActionNode(action.MoveForward)
                                                }
                                            )
                                        )
                                    }
                                )
                            }
                        )
                    }
                )
            )
        );
        bt.Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        bt.Tick();
    }

    public void Hit(int damage)
    {
        data.hp -= damage;
        HealthEventHandler();
    }
}

public class BossAction
{
    private EnemyData data;
    protected Collider2D collider;
    public Collider2D Collider { get { return collider; } set { collider = value; } }
    protected Rigidbody2D rigid;
    public Rigidbody2D Rigid { get { return rigid; } set { rigid = value; } }
    protected Transform transform;
    public Transform mTransform { get { return transform; } set { transform = value; } }
    protected SpriteRenderer sprite;
    public SpriteRenderer Sprite { get { return sprite; } set { sprite = value; } }
    protected Animator animator;
    public Animator Anime { get { return animator; } set { animator = value; } }
    protected AudioSource audioSource;
    public AudioSource Audio { get { return audioSource; } set { audioSource = value; } }
    private int count = 0;
    public BossAction(EnemyData data, Collider2D collider, Rigidbody2D rigid, Transform transform, SpriteRenderer sprite, Animator anime, AudioSource audioSource)
    {
        this.data = data;
        this.collider = collider;
        this.rigid = rigid;
        this.transform = transform;
        this.sprite = sprite;
        this.animator = anime;
        this.audioSource = audioSource;
    }
    private float time = 0.0f;
    
    public eNodeState Wait()
    {
        time += Time.deltaTime;
        if (time <= 2.0f)
            return eNodeState.RUNNING;
        time = 0.0f;
        //Debug.Log("wait end");
        return eNodeState.SUCCESS;
    }

    public float skillRange;

    public bool gotATarget()
    {
        if (data.target == null) return false;
        return true;
    }

    public bool IsTargetWithinDetectionRange()
    {
        if (Vector2.Distance(data.target.transform.position, this.transform.position) > 15.0f)
        {
            data.target = null;
            return false;
        }
        return true;
    }

    public bool IsAheadBlocked()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position + new Vector3(3.0f * data.lookAt, 1.0f, 0.0f), Vector2.right * data.lookAt, 1.0f, 64);
        if (hit.collider != null)
        {
            //Debug.Log("Blocked");
            return true;

        }
        return false;
    }

    public bool IsWithinAttackRange()
    {
        if(Vector2.Distance(new Vector2(data.target.transform.position.x, 0.0f), new Vector2(this.transform.position.x,0.0f)) < 5.0f)
        {
            return true;
        }
        return false;
    }

    public eNodeState ChangeDirection()
    {
        sprite.flipX = !sprite.flipX;
        data.lookAt *= -1.0f;
        return eNodeState.SUCCESS;
    }

    public eNodeState FindTarget()
    {
        count++;
        if (count % 10 != 0)
        {
            return eNodeState.SUCCESS;
        }
        RaycastHit2D hit = Physics2D.BoxCast(this.transform.position + new Vector3(0, 5, 0), new Vector2(20.0f, 10.0f), 0, Vector2.up, 0, 8);
        if (hit.collider != null)
        {
            data.target = hit.collider.GetComponent<PlayerController>();
            //Debug.Log("Å¸°ÙÀ» Ã£À½");
        }
        count = 0;
        return eNodeState.SUCCESS;
    }

    public eNodeState MoveForward()
    {
        transform.Translate(new Vector3(data.lookAt * data.moveSpeed * Time.deltaTime, 0.0f, 0.0f));
        return eNodeState.SUCCESS;
    }

    public eNodeState MoveToTarget()
    {
        data.lookAt = ((data.target.transform.position.x - this.transform.position.x) > 0.0f) ? 1.0f : -1.0f;
        sprite.flipX = (data.lookAt > 0.0f) ? true : false;
        this.transform.position = Vector2.MoveTowards(this.transform.position, this.transform.position + new Vector3(0.1f * Time.deltaTime * data.moveSpeed * data.lookAt, 0.0f,0.0f),  0.8f);
        //if(this.transform.position.x == data.target.transform.position.x)
        return eNodeState.SUCCESS;
        //return eNodeState.RUNNING;
    }

    public eNodeState ChangeToIdleAnimation()
    {
        animator.Play("IDLE");
        return eNodeState.SUCCESS;
    }

    public eNodeState ChangeToWalkAnimation()
    {
        animator.Play("WALK");
        return eNodeState.SUCCESS;
    }

    public eNodeState ChangeToAttackAnimation()
    {
        animator.Play("ATTACK");
        return eNodeState.SUCCESS;
    }
}


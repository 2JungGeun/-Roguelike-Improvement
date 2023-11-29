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
        data.moveSpeed = 4.0f;
        data.lookAt = GetComponent<SpriteRenderer>().flipX ? 1.0f : -1.0f;
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
                                new BTSequenceNode
                                (
                                    new List<BTNode>()
                                    {
                                        new BTActionNode(action.ChangeToIdleAnimation),
                                        new BTActionNode(action.Wait),
/*                                        new BTSelectorNode
                                        (
                                            new List<BTNode>()
                                            {
                                                new BTConditionalDecoratorNode(action.Condition, eAbortType.LOWPRIORITY,
                                                    new BTActionNode(action.Print)
                                                ),
                                                new BTActionNode(action.Print)
                                            }
                                        ),
                                        new BTActionNode(action.Wait)*/
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
                                                new BTActionNode(action.DetectTarget),
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
        bt.Initailize();
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

    public bool gotATarget()
    {
        if (data.target == null) return false;
        return true;
    }

    public bool IsTargetWithinDetectionRange()
    {
        if(Vector2.Distance(data.target.transform.position, this.transform.position) > 30.0f)
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

    public eNodeState ChangeDirection()
    {
        sprite.flipX = !sprite.flipX;
        data.lookAt *= -1.0f;
        return eNodeState.SUCCESS;
    }

    public eNodeState DetectTarget()
    {
        RaycastHit2D hit = Physics2D.BoxCast(this.transform.position + new Vector3(0, 5, 0), new Vector2(20.0f, 10.0f), 0, Vector2.up, 0, 8);
        if (hit.collider != null)
        {
            data.target = hit.collider.GetComponent<PlayerController>();
            //Debug.Log("≈∏∞Ÿ¿ª √£¿Ω");
        }
        return eNodeState.SUCCESS;
    }

    public eNodeState MoveForward()
    {
        this.transform.position = Vector2.MoveTowards(this.transform.position, this.transform.position + new Vector3(data.lookAt * data.moveSpeed * Time.fixedDeltaTime, 0, 0), 0.8f);
        return eNodeState.SUCCESS;
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
}


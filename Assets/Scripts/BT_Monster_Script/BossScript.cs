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

}

public class BossScript : MonoBehaviour
{
    [SerializeField]
    private GameObject UIPrefab;
    [SerializeField]
    private GameObject canvas;
    private EnemyData data;
    public EnemyData Data { get { return data; } }
    private GameObject ui;
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
        data.target = null;
        data.Collider = GetComponent<Collider2D>();
        data.Rigid = GetComponent<Rigidbody2D>();
        data.mTransform = GetComponent<Transform>();
        data.Sprite = GetComponent<SpriteRenderer>();
        data.Anime = GetComponent<Animator>();
        data.Audio = GetComponent<AudioSource>();
        ui = Instantiate(UIPrefab, canvas.transform);
        ui.GetComponent<EnemyUI>().Initialze("Boss");
    }
    void Start()
    {
        action = new BossAction(data);
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
                                        new BTActionNode(action.SelectSkill),
                                        new BTSelectorNode
                                        (
                                            new List<BTNode>()
                                            {
                                                new BTConditionalDecoratorNode(action.IsWithinAttackRange, eAbortType.LOWPRIORITY,
                                                    new BTSequenceNode
                                                    (
                                                        new List<BTNode>()
                                                        {
                                                            new BTActionNode(action.useSkill),
                                                            new BTActionNode(action.ChangeToIdleAnimation)
                                                        }
                                                    )
                                                ),
                                                new BTSequenceNode
                                                (
                                                    new List<BTNode>()
                                                    {
                                                        new BTActionNode(action.ChangeToWalkAnimation),
                                                        new BTActionNode(action.MoveToTarget)
                                                    }
                                                )
                                            }
                                        ),
                                        new BTActionNode(action.Wait)
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
                                        new BTActionNode(action.MoveForward)
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
        if(data.hp < 0.0f)
        {
            Destroy(this.ui);
            Destroy(this.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(this.transform.position + new Vector3(0, 2.5f, 0), new Vector2(3.0f, 5.0f));
    }
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT;

public class EnemyData
{
    public float hp;
    public float damage;
    public float moveSpeed;

}
public class BossScript : MonoBehaviour
{
    [SerializeField]
    private GameObject UIPrefab;
    [SerializeField]
    private GameObject canvas;
    private int hp;
    public int HP { get { return hp; } }
    BossAction action;
    BTRunner bt;
    public delegate void healthEventHandler();
    public healthEventHandler HealthEventHandler;

    void Awake()
    {
        hp = 1000;
        Instantiate(UIPrefab, canvas.transform).GetComponent<EnemyUI>().Initialze("Boss");
    }
    void Start()
    {
        action = new BossAction(GetComponent<Collider2D>(), GetComponent<Rigidbody2D>(), GetComponent<Transform>(), GetComponent<SpriteRenderer>(), GetComponent<Animator>(), GetComponent<AudioSource>());
        bt = new BTRunner
        (
            new BTRoot
            (
                new BTSequenceNode
                (
                    new List<BTNode>()
                    {
                        new BTConditionalDecoratorNode(action.Condition, eAbortType.LOWPRIORITY,
                            new BTSelectorNode
                            (
                                new List<BTNode>()
                                {
                                    new BTActionNode(action.Print),
                                    new BTActionNode(action.Wait)
                                }
                            )
                        ),
                        new BTSequenceNode
                        (
                            new List<BTNode>()
                            {
                                new BTActionNode(action.Print),
                                new BTActionNode(action.Wait)
                            }
                        ),
                        new BTSelectorNode
                        (
                            new List<BTNode>()
                            {
                                new BTConditionalDecoratorNode(action.Condition, eAbortType.SELF,
                                    new BTActionNode(action.Wait)),
                                new BTActionNode(action.Wait)
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
        action.Tick();
        //bt.Tick();
    }

    public void Hit(int damage)
    {
        hp -= damage;
        HealthEventHandler();
    }
}

public class BossAction
{
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
    public BossAction(Collider2D collider, Rigidbody2D rigid, Transform transform, SpriteRenderer sprite, Animator anime, AudioSource audioSource)
    {
        this.collider = collider;
        this.rigid = rigid;
        this.transform = transform;
        this.sprite = sprite;
        this.animator = anime;
        this.audioSource = audioSource;
    }
    private float time = 0.0f;
    public void Tick()
    {

    }
    public eNodeState Print()
    {
        int num = Random.Range(0, 10);
        Debug.Log("print 积己等 箭磊 : " + num);
        if (num % 2 == 0)
            return eNodeState.SUCCESS;
        else
            return eNodeState.FAILURE;
    }

    public eNodeState Wait()
    {
        time += Time.deltaTime;
        if (time <= 2.0f)
            return eNodeState.RUNNING;
        time = 0.0f;
        Debug.Log("wait end");
        return eNodeState.SUCCESS;
    }

    public bool Condition()
    {
        int condition = Random.Range(0, 10);
        Debug.Log("condition 积己等 箭磊 : " + condition);
        if (condition % 2 == 0)
            return true;
        else
            return false;
    }
}


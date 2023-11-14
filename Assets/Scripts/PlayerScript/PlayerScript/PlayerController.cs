using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public enum DamageType
{
    HP,
    INTELLECTUALITY
}

public class InputManager
{
    public float moveDir;
    public bool isJumpKeyDown;
    public bool isDownJumpKeyDown;
    public bool isDashKeyDown;
    public bool isAttackKeyDown;
    public (bool, KeyCode) isSkillKeyDown;
    public InputManager()
    {
        moveDir = 0.0f;
        isJumpKeyDown = false;
        isDownJumpKeyDown = false;
        isDashKeyDown = false;
        isAttackKeyDown = false;
        isSkillKeyDown = (false, KeyCode.None);
    }
    public void reset()
    {
        moveDir = 0.0f;
        isJumpKeyDown = false;
        isDownJumpKeyDown = false;
        isDashKeyDown = false;
        isAttackKeyDown = false;
        isSkillKeyDown = (false, KeyCode.None);
    }
}

public class PlayerData
{
    public int hp;
    public int intellectuality;
    public int money;
    public PlayerData()
    {
        this.money = 0;
        this.hp = 3;
        this.intellectuality = 100;
    }
    public void AddMoney(int money)
    {
        this.money += money;
    }
    public void UseMoney(int money)
    {
        this.money -= money;
    }
    public void UseIntellectuality(int cost)
    {
        this.intellectuality -= cost;
    }
}

public class PlayerController : MonoBehaviour
{

    public delegate void healthEventHandler();
    public healthEventHandler HealthEventHandler;

    public delegate void skillCooldownEventHandler();
    public skillCooldownEventHandler SkillCooldownEventHandler;

    public delegate void soulSwapEventHandler();
    public soulSwapEventHandler SoulSwapEventHandler;

    public delegate void moneyEventHandler();
    public moneyEventHandler MoneyEventHandler;

    private PlayerData playerData = new PlayerData();
    public PlayerData PlayerData { get { return playerData; } }

    //Input���� ����
    private InputManager input = new InputManager();

    //soul
    private int currIndex = 0;
    public int MainIndex { get { return currIndex; } }
    private int subIndex = 1;
    public int SubIndex { get { return subIndex; } }
    private Soul currSoul;
    public Soul CurrSoul { get { return currSoul; } }
    private List<Soul> ownSouls;
    public List<Soul> OwnSouls { get { return ownSouls; } }
    private float time = 0.0f;
    // Start is called before the first frame update
    private void Awake()
    {
        ownSouls = new List<Soul>();
        //base ĳ���� �ʱ�ȭ
        InitializeSoul();
    }

    void Start()
    {
        HealthEventHandler += Death;
    }

    // Update is called once per frame
    void Update()
    {
        if (currSoul == null)
            return;

        if (playerData.intellectuality < 100)
        {
            time += Time.deltaTime;
            if (time >= 1.0f)
            {
                time = 0.0f;
                playerData.intellectuality += 2;
                HealthEventHandler();
            }
        }

        input.moveDir = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump") && currSoul.MoveData.jumpCount < currSoul.Data.availableJumpCount)
        {
            if (!Input.GetKey(KeyCode.DownArrow))
            {
                input.isJumpKeyDown = true;
            }
            else
            {
                input.isDownJumpKeyDown = true;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift) && currSoul.mCooldownTime.dashCoolingdown && currSoul.Data.isUseDash)
        {
            input.isDashKeyDown = true;
        }
        else if (Input.GetKey(KeyCode.Z))
        {
            input.isAttackKeyDown = true;
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwapSoul();
        }
        if (!input.isSkillKeyDown.Item1)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                if (currSoul.Skills.ContainsKey(KeyCode.X) && currSoul.Skills[KeyCode.X].CanUseSkill(playerData.intellectuality))
                    input.isSkillKeyDown = (true, KeyCode.X);
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                if (currSoul.Skills.ContainsKey(KeyCode.C) && currSoul.Skills[KeyCode.C].CanUseSkill(playerData.intellectuality))
                    input.isSkillKeyDown = (true, KeyCode.C);
            }
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            playerData.AddMoney(300);
            MoneyEventHandler();
        }
        SkillCooldownEventHandler();
        currSoul.HandleInput(input);
        currSoul.Update(input);
        if (ownSouls.Count == 2)
            ownSouls[subIndex].Update(input);
    }

    private void FixedUpdate()
    {
        currSoul.FixedUpdate(input);
    }

    public void InitializeSoul()
    {
        object[] args = new object[] { "Knight" };
        Type t = Type.GetType("Knight");
        ownSouls.Add((Soul)System.Activator.CreateInstance(t, args));
        ownSouls[currIndex].Initialize(this.GetComponent<Collider2D>(), this.GetComponent<Rigidbody2D>(), this.transform, this.GetComponent<SpriteRenderer>(), this.GetComponent<Animator>(), this.GetComponent<AudioSource>());
        currSoul = ownSouls[currIndex];
        this.GetComponent<Animator>().runtimeAnimatorController = Resources.Load("Animator/SoulAnimator/" + currSoul.Data.name + "_Anime") as RuntimeAnimatorController;
    }

    public void SwapSoul()
    {
        if (ownSouls.Count == 2)
        {
            currSoul.SwapingSoul(input);
            switch (currIndex)
            {
                case 0:
                    currIndex = 1;
                    subIndex = 0;
                    break;
                case 1:
                    currIndex = 0;
                    subIndex = 1;
                    break;
                default:
                    break;
            }
            currSoul = ownSouls[currIndex];
            this.GetComponent<Animator>().runtimeAnimatorController = Resources.Load("Animator/SoulAnimator/" + currSoul.Data.name + "_Anime") as RuntimeAnimatorController;
            SoulSwapEventHandler();
            input.reset();
            currSoul.Start(input);
        }
    }

    public void ModifySoul(string name, int selectedNum)
    {
        if (ownSouls.Count == 1)
        {
            object[] args = new object[] { name };
            Type t = Type.GetType(name);
            ownSouls.Add((Soul)System.Activator.CreateInstance(t, args));
            ownSouls[ownSouls.Count - 1].Initialize(this.GetComponent<Collider2D>(), this.GetComponent<Rigidbody2D>(), this.transform, this.GetComponent<SpriteRenderer>(), this.GetComponent<Animator>(), this.GetComponent<AudioSource>());
        }
        else
        {
            object[] args = new object[] { name };
            Type t = Type.GetType(name);
            ownSouls[subIndex] = (Soul)System.Activator.CreateInstance(t, args);
            ownSouls[subIndex].Initialize(this.GetComponent<Collider2D>(), this.GetComponent<Rigidbody2D>(), this.transform, this.GetComponent<SpriteRenderer>(), this.GetComponent<Animator>(), this.GetComponent<AudioSource>());
        }
        SoulSwapEventHandler();
    }

    public List<string> GetPlayerSoulNameList()
    {
        List<string> nameList = new List<string>();
        foreach (Soul soul in ownSouls)
        {
            nameList.Add(soul.Data.name);
        }
        return nameList;
    }

    public void Hit(DamageType damageType, int damage)
    {
        if (currSoul.soulState.GetType() == typeof(DeadState))
            return;

        switch (damageType)
        {
            case DamageType.HP:
                playerData.hp -= damage;
                break;
            case DamageType.INTELLECTUALITY:
                playerData.intellectuality -= damage;
                break;
        }
        HealthEventHandler();
        if (!isDead())
        {
            currSoul.Hit(input);
        }
        else
        {
            currSoul.Dead(input);
        }

    }

    public void GetMoney(int money)
    {
        playerData.AddMoney(money);
        MoneyEventHandler();
    }

    public void UseMoney(int money)
    {
        playerData.UseMoney(money);
        MoneyEventHandler();
    }

    private bool isDead()
    {
        if (playerData.hp <= 0 || playerData.intellectuality <= 0)
            return true;
        return false;
    }

    private void Death()
    {
        if (playerData.hp <= 0 || playerData.intellectuality <= 0)
            currSoul.Dead(input);
    }
}

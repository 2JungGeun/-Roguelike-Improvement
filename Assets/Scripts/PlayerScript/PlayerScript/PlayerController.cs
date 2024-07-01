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

public enum KeyAction
{
    NONE,
    LEFTMOVE,
    RIGHTMOVE,
    JUMP,
    DONWJUMP,
    DASH,
    BASIC_ATTACK,
    FIRST_SKILL,
    SECOND_SKILL,
    SOULSWAP,
    KEYCOUNT
}

public class KeySetting
{
    private Dictionary<KeyAction, KeyCode> keys = new Dictionary<KeyAction, KeyCode>();
    public Dictionary<KeyAction, KeyCode> Keys { get { return keys; } }

    public KeySetting()
    {
        KeyCode[] keyCodes = new KeyCode[] { KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.Space, KeyCode.DownArrow, KeyCode.LeftShift, KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.Tab };
        for (int i = 1; i < (int)KeyAction.KEYCOUNT; i++)
        {
            keys.Add((KeyAction)i, keyCodes[i-1]);
        }
    }

    public bool ModifyKey(KeyAction key, KeyCode value)
    {
        foreach (KeyValuePair<KeyAction, KeyCode> kvp in keys)
        {
            if (kvp.Equals(value))
            {
                return false;
            }
        }
        keys[key] = value;
        return true;
    }
}

public class InputHandle
{
    private KeySetting keySetting;

    public InputHandle()
    {
        keySetting = new KeySetting();
    }

    public KeyAction Update()
    {
        if (Input.GetKeyDown(keySetting.Keys[KeyAction.JUMP]))
        {
            if (!Input.GetKey(keySetting.Keys[KeyAction.DONWJUMP]))
            {
                return KeyAction.JUMP;
            }
            else
            {
                return KeyAction.DONWJUMP;
            }
        }
        else if (Input.GetKeyDown(keySetting.Keys[KeyAction.DASH]))
        {
            return KeyAction.DASH;
        }
        else if (Input.GetKeyDown(keySetting.Keys[KeyAction.SOULSWAP]))
        {
            return KeyAction.SOULSWAP;
        }
        else if (Input.GetKeyDown(keySetting.Keys[KeyAction.FIRST_SKILL]))
        {
            return KeyAction.FIRST_SKILL;
        }
        else if (Input.GetKeyDown(keySetting.Keys[KeyAction.SECOND_SKILL]))
        {
            return KeyAction.SECOND_SKILL;
        }
        else if (Input.GetKey(keySetting.Keys[KeyAction.BASIC_ATTACK]))
        {
            return KeyAction.BASIC_ATTACK;
        }
        else if (Input.GetKey(keySetting.Keys[KeyAction.LEFTMOVE]))
        {
            return KeyAction.LEFTMOVE;
        }
        else if (Input.GetKey(keySetting.Keys[KeyAction.RIGHTMOVE]))
        {
            return KeyAction.RIGHTMOVE;
        }
        return KeyAction.NONE;
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
    private InputHandle hInput = new InputHandle();
    private KeyAction keyAction = KeyAction.NONE;
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
        InitializeSoul();
    }

    private void Start()
    {
        GameManager.Instance.CreateGame();
        PacketManager.Instance.CreatePacket();
        HealthEventHandler += Death;
    }

    // Update is called once per frame
    private void Update()
    {
        if (currSoul == null)
            return;

        if (playerData.intellectuality < 100)
        {
            time += Time.deltaTime;
            if (time >= 5.0f)
            {
                time = 0.0f;
                if (playerData.intellectuality <= 90)
                    playerData.intellectuality += 10;
                else
                    playerData.intellectuality += 100 - playerData.intellectuality;
                HealthEventHandler();
            }
        }

        keyAction = hInput.Update();
        if(keyAction == KeyAction.SOULSWAP)
            SwapSoul();
        SkillCooldownEventHandler();
        currSoul.HandleInput(keyAction);
        currSoul.Update(keyAction);
        if (ownSouls.Count == 2)
            ownSouls[subIndex].Update(keyAction);

        if (Input.GetKeyDown(KeyCode.M))
        {
            playerData.AddMoney(300);
            MoneyEventHandler();
        }
    }

    private void FixedUpdate()
    {
        currSoul.FixedUpdate(keyAction);
    }

    private void InitializeSoul()
    {
        object[] args = new object[] { "Knight" };
        Type t = Type.GetType("Knight");
        ownSouls.Add((Soul)System.Activator.CreateInstance(t, args));
        ownSouls[currIndex].Initialize(this.GetComponent<Collider2D>(), this.GetComponent<Rigidbody2D>(), this.transform, this.GetComponent<SpriteRenderer>(), this.GetComponent<Animator>(), this.GetComponent<AudioSource>());
        currSoul = ownSouls[currIndex];
        this.GetComponent<Animator>().runtimeAnimatorController = Resources.Load("Animator/SoulAnimator/" + currSoul.Data.name + "_Anime") as RuntimeAnimatorController;
    }

    private void SwapSoul()
    {
        if (currSoul.soulState.GetType() != Type.GetType(currSoul.Data.name + "IdleState"))
            return;
        if (ownSouls.Count == 2)
        {
            currSoul.SwapingSoul(keyAction);
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
            currSoul.Start(keyAction);
        }
    }

    public void ModifySoul(string name)
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
            currSoul.Hit(keyAction);
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
        {
            currSoul.Dead(keyAction);
        }
    }
}

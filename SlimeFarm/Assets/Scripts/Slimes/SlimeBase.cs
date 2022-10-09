using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://www.youtube.com/watch?v=7jxS8HIny3Q
[CreateAssetMenu(fileName = "Slime", menuName ="Slime/Create new slime")]

public class SlimeBase : ScriptableObject
{

    [SerializeField] string nameSlime;

    [SerializeField] Sprite idleOneSprite;
    [SerializeField] Sprite idleTwoSprite;

    [SerializeField] SlimeType type;

    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int speed;

    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;

    [SerializeField] List<LearnableSkill> learnableSkills;

    public int GetExpForLevel(int level)
    {
        if (growthRate == GrowthRate.Fast)
        {
            return 4 * (level * level* level) / 5;
        }
        else if (growthRate == GrowthRate.Medium)
        {
            return level * level * level;
        }
        else if (growthRate == GrowthRate.Slow)
        {
            return 5 * (level * level * level) / 4;
        }

        return -1;
    }

    public string Name
    {
        get { return nameSlime; }
    }
    public Sprite IdleOneSprite
    {
        get { return idleOneSprite; }
    }
    public Sprite IdleTwoSprite
    {
        get { return idleTwoSprite; }
    }
    public SlimeType Type
    {
        get { return type; }
    }
    public int MaxHP
    {
        get { return maxHp; }
    }
    public int Attack
    {
        get { return attack; }
    }
    public int Defense
    {
        get { return defense; }
    }
    public int Speed
    {
        get { return speed; }
    }
    public List<LearnableSkill> LearnableSkills
    {
        get { return learnableSkills; }
    }
    public int ExpYield => expYield;

    public GrowthRate GrowthRate => growthRate;
}

[System.Serializable]

public class LearnableSkill // Revisar en futuro otro metodo (?)
{
    [SerializeField] SkillBase skillBase;
    [SerializeField] int level;

    public SkillBase Base
    {
        get { return skillBase; }
    }
    public int Level
    {
        get { return level; }
    }

}

// https://learn.unity.com/tutorial/enumeraciones#
public enum SlimeType
{
    None,       // Default
    Grass,      // Z1 - Planta
    Mud,        // Z1 - Barro
    Rock,       // Z2 - Roca
    Fire,       // Z2 - Fuego
    Water,      // Z3 - Agua
    Sand,       // Z3 - Arena
    Legendary,  // Z0 - Legendario
    Normal      // Base
}

public enum GrowthRate
{
    Fast, Medium, Slow
}

public enum Stat
{
    Attack,
    Defense,
    Speed
}

public class TypeTable
{
    static float[][] table =
    {
        //                        Gra Mud Roc Fir Wat San Leg Nor
        /* Grass */ new float [] {0.5f, 1f, 1f, 0.5f, 2f, 1f, 1f, 1f},
        /* Mud */   new float [] {1f, 0.5f, 1f, 1f, 1f, 2f, 1f, 1f},
        /* Rock */  new float [] {1f, 2f, 0.5f, 1f, 1f, 1f, 1f, 1f},
        /* Fire */  new float [] {2f, 1f, 1f, 0.5f, 0.5f, 1f, 1f, 1f},
        /* Water */ new float [] {0.5f, 1f, 1f, 2f, 0.5f, 1f, 1f, 1f},
        /* Sand */  new float [] {1f, 1f, 2f, 1f, 1f, 0.5f, 1f, 1f},
        /* Legen */ new float [] {2.5f, 2.5f, 2.5f, 2.5f, 2.5f, 2.5f, 0.5f, 2.5f},
        /* Normal */new float [] {1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f},
    };
    
    public static float GetCounters(SlimeType attackType, SlimeType defenseType)
    {
        if (attackType == SlimeType.None || defenseType == SlimeType.None)
        {
            return 1;
        }
        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return table[row][col];
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Skill", menuName ="Slime/Create new skill")]

public class SkillBase : ScriptableObject
{
    [SerializeField] string name;

    [SerializeField] SlimeType type;

    [SerializeField] int power;
    // [SerializeField] int accuracy;  De momento no lo descarto
    [SerializeField] int pp;
    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffects effects;
    [SerializeField] MoveTarget target;

    public string Name
    {
        get { return name; }
    }
    public SlimeType Type
    {
        get { return type; }
    }
    public int Power
    {
        get { return power; }
    }
    public int PP
    {
        get { return pp; }
    }
    public MoveCategory Category
    {
        get { return category; }
    }
    public MoveEffects Effects
    {
        get { return effects; }
    }
    public MoveTarget Target
    {
        get { return target; }
    }
}

[System.Serializable]
public class MoveEffects
{
    [SerializeField] List<StatBoost> boosts;

    public List<StatBoost> Boosts
    {
        get { return boosts; }
    }
}

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}

public enum MoveCategory
{
    Normal, Legendary
}

public enum MoveTarget
{
    Enemy, Self
}
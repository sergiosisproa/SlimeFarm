using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable] // https://docs.unity3d.com/Manual/script-Serialization.html y https://docs.unity3d.com/ScriptReference/Serializable.html 

public class Slime
{
    [SerializeField] SlimeBase _base;
    [SerializeField] int level;

    public SlimeBase Base {
        get { return _base; }
    }
    public int Level { 
        get { return level; }
    }

    public int Exp { get; set; }
    public int HP { get; set; }
    public List<Skill> Skills { get; set; }

    public Skill CurrentMove { get; set; }

    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();

    public void Init()
    {
        Skills = new List<Skill>();
        foreach (var skill in Base.LearnableSkills)
        {
            if (skill.Level <= Level)
            {
                Skills.Add(new Skill(skill.Base));
            }
        }

        Exp = Base.GetExpForLevel(Level);

        CalculateStats();
        HP = MaxHP;

        ResetStatBoost();
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, (int)((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, (int)((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, (int)((Base.Speed * Level) / 100f) + 5);

        MaxHP = (int)((Base.MaxHP * Level) / 100f) + 10;
    }

    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0 },
            {Stat.Defense, 0 },
            {Stat.Speed, 0 },
        };
    }

    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        // TODO: Logica de stat
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f};

        if (boost >= 0)
        {
            statVal = (int)(statVal * boostValues[boost]);
        }
        else
        {
            statVal = (int)(statVal / boostValues[-boost]);
        }

        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach(var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0)
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} went up!");
            }
            else
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell down!");
            }
        }
    }

    public bool CheckForLevelUp()
    {
        if (Exp > Base.GetExpForLevel(level+1))
        {
            level++;
            return true;
        }

        return false;
    }

    public LearnableSkill GetLearnableSkillAtCurrLevel()
    {
        return Base.LearnableSkills.Where(x => x.Level == level).FirstOrDefault();
    }

    public void LearnSkill(LearnableSkill skillToLearn)
    {
        Skills.Add(new Skill(skillToLearn.Base));
    }

    // https://kimsama.gitbooks.io/unity-quicksheet/content/usage-formula/
    public int MaxHP
    {
        get; private set;
    }
    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }
    public int Defense
    {
        get { return GetStat(Stat.Defense); } 
    }
    public int Speed
    {
        get { return GetStat(Stat.Speed); } 
    }
    // Si en (int) da fallos, cambiar por "Mathf.FloorToInt" 

    public DamageDetails TakeDamage(Skill skill, Slime attacker)
    {
        // Añadir critico? Random int 0-101, if <= (crit chance) activar bool crit y añadir a formula "modifiers"

        float type = TypeTable.GetCounters(skill.Base.Type, this.Base.Type);

        var damageDetails = new DamageDetails()
        {
            TypeEffectine = type,   //Añadir critico aquí tambien
            Killed = false
        };

        //https://bulbapedia.bulbagarden.net/wiki/Damage de nuevo, formula de pokemon
        float modifiers = Random.Range(0.85f, 1f) * type;
        float damageBase = ((2 * attacker.Level + 10) / 250f) * skill.Base.Power * ((float)attacker.Attack / Defense) + 2;
        int damage = (int)(damageBase * modifiers);

        HP -= damage;
        if (HP <= 0)
        {
            HP = 0;
            damageDetails.Killed = true;
        }
        return damageDetails;
    }

    public Skill GetRandomMove()
    {
        int r = Random.Range(0, Skills.Count);
        return Skills[r];
    }

    public void OnBattleOver()
    {
        ResetStatBoost();
    }
}
public class DamageDetails
{
    public bool Killed { get; set; }
    public float TypeEffectine { get; set; } // Si añado critico despues, añadir aquí tambien para su mensaje
}

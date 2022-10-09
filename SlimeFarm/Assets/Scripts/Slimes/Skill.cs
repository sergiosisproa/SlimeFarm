using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    public SkillBase Base { get; set; }
    public int PP { get; set; }

    public Skill(SkillBase sBase)
    {
        Base = sBase;
        PP = sBase.PP;
    }


}

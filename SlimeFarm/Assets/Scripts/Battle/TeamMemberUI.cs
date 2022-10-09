using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;

    [SerializeField] Color selectionColor;

    Slime _slime; 

    public void SetData(Slime slime)
    {
        _slime = slime;
        nameText.text = slime.Base.Name;
        levelText.text = "Lvl " + slime.Level;
        hpBar.SetHP((float)slime.HP / slime.MaxHP);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            nameText.color = selectionColor;
        }
        else
        {
            nameText.color = Color.black;
        }
    }
}

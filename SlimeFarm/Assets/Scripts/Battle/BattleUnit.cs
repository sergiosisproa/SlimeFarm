using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    //[SerializeField] SlimeBase _base;
    //[SerializeField] int level;           Para hardcodear pokemons, ya no hace falta
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

    public BattleHud Hud
    {
        get { return hud; }
    }

    public bool IsPlayerUnit
    {
        get { return isPlayerUnit; }
    }


    public Slime Slime { get; set; }


    public void Setup(Slime slime)
    {
        //Slime = new Slime(_base, level);  -More del hardcode
        Slime = slime;
        if (isPlayerUnit)
        {
            GetComponent<Image>().sprite = Slime.Base.IdleOneSprite;
        }
        else
        {
            GetComponent<Image>().sprite = Slime.Base.IdleTwoSprite;
        }
        hud.gameObject.SetActive(true);

        hud.SetData(slime);
    }

    public void Clear()
    {
        hud.gameObject.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject expBar;

    Slime _slime; // Coge en slime creado en el SetData para usarlo despues en el UpdateHP

    public void SetData(Slime slime)
    {
        _slime = slime;
        nameText.text = slime.Base.Name;
        SetLevel();
        hpBar.SetHP((float) slime.HP/slime.MaxHP);
        SetExp();
    }

    public void SetLevel()
    {
        levelText.text = "Lvl " + _slime.Level;
    }

    public void SetExp(bool reset=false)
    {
        if (expBar == null)
        {
            return;
        }
        if (reset=true)
        {
            expBar.transform.localScale = new Vector3(0, 1, 1);
        }

        float normalizedExp = GetNormalizeExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    float GetNormalizeExp()
    {
        int currLevelExp = _slime.Base.GetExpForLevel(_slime.Level);
        int nextLevelExp = _slime.Base.GetExpForLevel(_slime.Level+1);

        float normalizeExp = (float)(_slime.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizeExp);
    }

    public void UpdateHP()
    {
        hpBar.SetHP((float)_slime.HP / _slime.MaxHP);
    }
}

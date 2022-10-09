using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamScreen : MonoBehaviour
{
    [SerializeField] Text messageText;

    TeamMemberUI[] memberSlots;
    List<Slime> slimes;

    public void Init()
    {
        memberSlots = GetComponentsInChildren<TeamMemberUI>();   // IMPORTANTE: Es ComponentS con S al final en caso de array
    }

    public void SetTeamData(List<Slime> slimes)
    {
        this.slimes = slimes;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < slimes.Count)
            {
                memberSlots[i].SetData(slimes[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        messageText.text = "Choose a Slime!";
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < slimes.Count; i++)
        {
            if (i == selectedMember)
            {
                memberSlots[i].SetSelected(true);
            }
            else
            {
                memberSlots[i].SetSelected(false);
            }
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}

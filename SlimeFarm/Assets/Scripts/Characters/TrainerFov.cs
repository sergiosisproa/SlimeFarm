using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerFov : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(Player player)
    {
        GameController.Instance.OnEnterTrainersView(GetComponentInParent<Trainer>());
    }
}

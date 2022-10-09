using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { World, Battle, Dialog, Cutscene, Paused}

public class GameController : MonoBehaviour
{
    [SerializeField] Player playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    GameState state;

    GameState stateBeforePause;

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        battleSystem.OnBattleOver += EndBattle;

        DialogManager.Instance.OnShowDialog += () =>
        {
            state = GameState.Dialog;
        };
        
        DialogManager.Instance.OnCloseDialog += () =>
        {
            if (state == GameState.Dialog)
            {
                state = GameState.World;
            }
        };
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            stateBeforePause = state;
            state = GameState.Paused;
        }
        else
        {
            state = stateBeforePause;
        }
    }

    private void Update()
    {
        if (state == GameState.World)
        {
            playerController.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
    }

    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true); // Activa todo el battle system del arbol de unity
        worldCamera.gameObject.SetActive(false); // Apagar la camara del mundo

        var playerParty = playerController.GetComponent<SlimeParty>();
        var wildSlime = FindObjectOfType<MapZone>().GetComponent<MapZone>().GetRandomSlime();

        battleSystem.StartBattle(playerParty, wildSlime);

    }

    Trainer trainer;
    public void StartTrainerBattle(Trainer trainer)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true); // Activa todo el battle system del arbol de unity
        worldCamera.gameObject.SetActive(false); // Apagar la camara del mundo

        this.trainer = trainer;
        var playerParty = playerController.GetComponent<SlimeParty>();
        var trainerParty = trainer.GetComponent<SlimeParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);

    }

    public void OnEnterTrainersView(Trainer trainer)
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
    }

    void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }

        state = GameState.World;
        battleSystem.gameObject.SetActive(false); 
        worldCamera.gameObject.SetActive(true); 

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, TeamScreen, BattleOver}
public enum BattleAction { Move, SwitchSlime, UseItem, Run}

public class BattleSystem : MonoBehaviour
{

    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] TeamScreen teamScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;
    int currentAction;
    int currentMove;
    int currentMember;

    SlimeParty playerParty;
    SlimeParty trainerParty;
    Slime wildSlime;

    bool isTrainerBattle = false;
    Player player;
    Trainer trainer;

    int escapeAttempts;

    public void StartBattle(SlimeParty playerParty, Slime wildSlime)
    {
        this.playerParty = playerParty;
        this.wildSlime = wildSlime;
        player = playerParty.GetComponent<Player>();
        isTrainerBattle = false;

        StartCoroutine(SetUpBattle());
    }

    public void StartTrainerBattle(SlimeParty playerParty, SlimeParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<Player>();
        trainer = trainerParty.GetComponent<Trainer>();

        StartCoroutine(SetUpBattle());
    }

    public IEnumerator SetUpBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle)
        {
            // Wild Slime
            playerUnit.Setup(playerParty.GetAlivesSlime());
            enemyUnit.Setup(wildSlime);

            dialogBox.SetMoveNames(playerUnit.Slime.Skills);
            // https://docs.microsoft.com/es-es/dotnet/csharp/language-reference/tokens/interpolated Error: NO poner espacio entre $ y la "" o no funciona
            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Slime.Base.Name} appeared");
        }
        else
        {
            // Trainer Battle
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);

            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} challenges to a fight!");

            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemySlime = trainerParty.GetAlivesSlime();
            enemyUnit.Setup(enemySlime);
            yield return dialogBox.TypeDialog($"{trainer.Name} send out {enemySlime.Base.Name}");

            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerSlime = playerParty.GetAlivesSlime();
            playerUnit.Setup(playerSlime);
            yield return dialogBox.TypeDialog($"Go {playerSlime.Base.Name}!");


        }

        escapeAttempts = 0;
        teamScreen.Init();
        ActionSelection();
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Slimes.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionText(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionText(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveText(true);
    }

    void OpenTeamScreen()
    {
        state = BattleState.TeamScreen;
        teamScreen.SetTeamData(playerParty.Slimes);
        teamScreen.gameObject.SetActive(true);
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Move)
        {
            playerUnit.Slime.CurrentMove = playerUnit.Slime.Skills[currentMove];
            enemyUnit.Slime.CurrentMove = enemyUnit.Slime.GetRandomMove();

            bool playerGoesFirst = playerUnit.Slime.Speed >= enemyUnit.Slime.Speed;

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondSlime = secondUnit.Slime;

            // first turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Slime.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if (secondSlime.HP > 0)
            {
                // second turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Slime.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }

            if (state != BattleState.BattleOver)
            {
                ActionSelection();
            }

        }
        else
        {
            if (playerAction == BattleAction.SwitchSlime)
            {
                var selectedSlime = playerParty.Slimes[currentMember];
                state = BattleState.Busy;
                yield return SwitchSlime(selectedSlime);
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
            }

            var enemyMove = enemyUnit.Slime.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }

        if (state != BattleState.BattleOver)
        {
            ActionSelection();
        }
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Skill move)
    {
        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Slime.Base.Name} used {move.Base.Name}");

        yield return RunMoveEffects(move, sourceUnit.Slime, targetUnit.Slime);

        var damageDetails = targetUnit.Slime.TakeDamage(move, sourceUnit.Slime);
        targetUnit.Hud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);


        if (targetUnit.Slime.HP <= 0)
        {
            yield return HandleSlimeDead(targetUnit);
        }
    }

    IEnumerator HandleSlimeDead(BattleUnit deadUnit)
    {
        yield return dialogBox.TypeDialog($"{deadUnit.Slime.Base.Name} dead.");
        yield return new WaitForSeconds(2f);

        if (!deadUnit.IsPlayerUnit)
        {
            int expYield = deadUnit.Slime.Base.ExpYield;
            int enemyLevel = deadUnit.Slime.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.Slime.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Slime.Base.Name} gained {expGain} exp.");
            playerUnit.Hud.SetExp();

            //Check LevelUp
            while (playerUnit.Slime.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Slime.Base.Name} grew to level {playerUnit.Slime.Level}");

                //New Skill
                var newSkill = playerUnit.Slime.GetLearnableSkillAtCurrLevel();
                if (newSkill != null)
                {
                    playerUnit.Slime.LearnSkill(newSkill);
                    yield return dialogBox.TypeDialog($"{playerUnit.Slime.Base.Name} learned {newSkill.Base.Name}");
                    dialogBox.SetMoveNames(playerUnit.Slime.Skills);
                }

                playerUnit.Hud.SetExp(true);
            }

            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(deadUnit);
    }

    IEnumerator RunMoveEffects(Skill skill, Slime source, Slime target)
    {
        var effects = skill.Base.Effects;
        if (effects.Boosts != null)
        {
            if (skill.Base.Target == MoveTarget.Self)
            {
                source.ApplyBoosts(effects.Boosts);
            }
            else
            {
                target.ApplyBoosts(effects.Boosts);
            }
        }
        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        if (sourceUnit.Slime.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Slime.Base.Name} dead.");
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(sourceUnit);
        }
    }

    IEnumerator ShowStatusChanges(Slime slime)
    {
        while (slime.StatusChanges.Count > 0)
        {
            var message = slime.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    void CheckForBattleOver(BattleUnit killedUnit)
    {
        if (killedUnit.IsPlayerUnit)
        {
            var nextSlime = playerParty.GetAlivesSlime();
            if (nextSlime != null)
            {
                OpenTeamScreen();
            }
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            if (!isTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextSlime = trainerParty.GetAlivesSlime();
                if (nextSlime != null)
                {
                    StartCoroutine(SendNextTrainerSlime(nextSlime));
                }
                else
                {
                    BattleOver(true);
                }
            }

        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.TypeEffectine > 1)
        {
            yield return dialogBox.TypeDialog("A super effective hit!");
        }
        else if (damageDetails.TypeEffectine < 1)
        {
            yield return dialogBox.TypeDialog("A not effective hit");
        }
    }

    public void HandleUpdate() // Para llamar al update desde el GameController
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.TeamScreen)
        {
            HandleTeamSelection();
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentAction++;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentAction--;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 2;
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                //Items
            }
            else if (currentAction == 2)
            {
                prevState = state;
                OpenTeamScreen();
            }
            else if (currentAction == 3)
            {
                //Exit
                StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }

    void HandleMoveSelection()
    {
        if (playerUnit.Slime.Level <= 4)
        {
            currentMove = 0;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) && (playerUnit.Slime.Skills.Count > 1))
            {
                if (currentMove < 1)
                {
                    currentMove++;
                }
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) && (playerUnit.Slime.Skills.Count > 1))
            {
                if (currentMove > 0)
                {
                    currentMove--;
                }
            }
        }

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Slime.Skills[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveText(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveText(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    void HandleTeamSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentMember++;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentMember--;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMember += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMember -= 2;
        }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Slimes.Count - 1);

        teamScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Slimes[currentMember];
            if (selectedMember.HP <= 0)
            {
                teamScreen.SendMessage("You can´t choose a defeate Slime");
                return;
            }
            if (selectedMember == playerUnit.Slime)
            {
                teamScreen.SendMessage("You can´t choose the same Slime");
                return;
            }

            teamScreen.gameObject.SetActive(false);

            if (prevState == BattleState.ActionSelection)
            {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchSlime));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchSlime(selectedMember));
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            teamScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    IEnumerator SwitchSlime(Slime newSlime)
    {
        if (playerUnit.Slime.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Slime.Base.Name}");
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newSlime);
        dialogBox.SetMoveNames(newSlime.Skills);

        yield return dialogBox.TypeDialog($"Go {newSlime.Base.Name}!");

        state = BattleState.RunningTurn;
    }

    IEnumerator SendNextTrainerSlime(Slime nextSlime)
    {
        state = BattleState.Busy;

        enemyUnit.Setup(nextSlime);
        yield return dialogBox.TypeDialog($"{trainer.Name} send out {nextSlime.Base.Name}");

        state = BattleState.RunningTurn;
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't run from a trainer battles!");
            state = BattleState.RunningTurn;
            yield break;
        }

        escapeAttempts++;

        int playerSpeed = playerUnit.Slime.Speed;
        int enemySpeed = enemyUnit.Slime.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Ran away safely!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Ran away safely!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Cant escape, {enemyUnit.Slime.Base.Name}'s speed is very fast!");
                state = BattleState.RunningTurn;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCState { Idle, Walikng, Dialog }

public class NPC : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog;
    [SerializeField] List<Vector2> movementPatter;
    [SerializeField] float timeBetwennPatter;


    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;

    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void Interact(Transform initiator)
    {
        if (state == NPCState.Idle)
        {
            state = NPCState.Dialog;
            character.LookTowards(initiator.position);

            StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () =>
            {
                idleTimer = 0f;
                state = NPCState.Idle;
            }));
        }
    }

    private void Update()
    {

        if (state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > timeBetwennPatter)
            {
                idleTimer = 0f;
                if (movementPatter.Count > 0)
                {
                    StartCoroutine(Walk());
                }
            }
        }
        character.HandledUpdate();
    }

    IEnumerator Walk()
    {
        state = NPCState.Walikng;

        var oldPos = transform.position;

        yield return character.Move(movementPatter[currentPattern]);

        if (transform.position != oldPos)
        {
            currentPattern = (currentPattern + 1) % movementPatter.Count;
        }

        state = NPCState.Idle;
    }
}


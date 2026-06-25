using System.Collections;
using UnityEngine;

public class LompatTaliBotController : MonoBehaviour
{
    public LompatTaliSingleManager manager;
    public float minReactionDelay = 0.02f;
    public float maxReactionDelay = 0.12f;
    [Range(0f, 1f)] public float jumpChance = 0.9f;

    Coroutine botRoutine;

    void OnEnable()
    {
        if (botRoutine != null)
            StopCoroutine(botRoutine);

        botRoutine = StartCoroutine(BotLoop());
    }

    void OnDisable()
    {
        if (botRoutine != null)
        {
            StopCoroutine(botRoutine);
            botRoutine = null;
        }
    }

    IEnumerator BotLoop()
    {
        int lastHandledDangerPhase = -1;

        while (enabled)
        {
            if (manager == null || !manager.CanBotAct())
            {
                yield return null;
                continue;
            }

            if (!manager.IsRopeDangerPhaseActive || manager.DangerPhaseSequence == lastHandledDangerPhase)
            {
                yield return null;
                continue;
            }

            lastHandledDangerPhase = manager.DangerPhaseSequence;

            if (Random.value > jumpChance)
                continue;

            float reactionDelay = Random.Range(minReactionDelay, maxReactionDelay);
            yield return new WaitForSeconds(reactionDelay);

            if (manager == null || !manager.CanBotAct() || !manager.IsRopeDangerPhaseActive)
                continue;

            manager.TryBotJump();
        }
    }
}

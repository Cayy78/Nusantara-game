using System.Collections;
using UnityEngine;

public class PanjatPinangBotController : MonoBehaviour
{
    public PanjatPinangSingleManager manager;

    [Header("Bot Timing")]
    public float minDecisionDelay = 0.2f;
    public float maxDecisionDelay = 0.6f;

    [Header("Bot Accuracy")]
    [Range(0f, 1f)] public float mistakeChance = 0.25f;

    [Header("Zone Sampling")]
    [Range(0f, 0.2f)] public float zonePadding = 0.02f;

    private Coroutine botRoutine;

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
        while (enabled)
        {
            if (manager == null || !manager.CanBotAct())
            {
                yield return null;
                continue;
            }

            float decisionDelay = Random.Range(minDecisionDelay, maxDecisionDelay);
            yield return new WaitForSeconds(decisionDelay);

            if (manager == null || !manager.CanBotAct())
                continue;

            float rawValue = ChooseBotRawValue();

            manager.StartCharge(2);
            yield return new WaitForSeconds(GetHoldDuration(rawValue));

            if (manager == null || !manager.CanBotAct())
                continue;

            manager.ReleaseCharge(2, rawValue);
        }
    }

    float ChooseBotRawValue()
    {
        PanjatPinangPowerBar referenceBar = GetReferenceBar();
        PanjatPinangPowerBar.PowerZone targetZone = PanjatPinangPowerBar.PowerZone.Green;

        if (Random.value < mistakeChance)
            targetZone = GetMistakeZone(targetZone);

        GetZoneBounds(targetZone, referenceBar, out float minValue, out float maxValue);
        return Random.Range(minValue, maxValue);
    }

    float GetHoldDuration(float rawValue)
    {
        PanjatPinangPowerBar referenceBar = GetReferenceBar();

        float speed = referenceBar != null ? Mathf.Max(0.01f, referenceBar.speed) : 1f;
        return rawValue / speed;
    }

    PanjatPinangPowerBar GetReferenceBar()
    {
        if (manager == null)
            return null;

        if (manager.panjatPinangBar2 != null)
            return manager.panjatPinangBar2;

        return manager.panjatPinangBar1;
    }

    PanjatPinangPowerBar.PowerZone GetMistakeZone(PanjatPinangPowerBar.PowerZone targetZone)
    {
        switch (targetZone)
        {
            case PanjatPinangPowerBar.PowerZone.Green:
                return Random.value < 0.5f ? PanjatPinangPowerBar.PowerZone.Yellow : PanjatPinangPowerBar.PowerZone.Red;
            case PanjatPinangPowerBar.PowerZone.Yellow:
                return Random.value < 0.5f ? PanjatPinangPowerBar.PowerZone.Red : PanjatPinangPowerBar.PowerZone.Green;
            default:
                return Random.value < 0.5f ? PanjatPinangPowerBar.PowerZone.Yellow : PanjatPinangPowerBar.PowerZone.Green;
        }
    }

    void GetZoneBounds(PanjatPinangPowerBar.PowerZone zone, PanjatPinangPowerBar referenceBar, out float minValue, out float maxValue)
    {
        float redMax = referenceBar != null ? Mathf.Clamp01(referenceBar.redMax) : 0.3f;
        float yellowMax = referenceBar != null ? Mathf.Clamp01(referenceBar.yellowMax) : 0.7f;
        float padding = Mathf.Max(0f, zonePadding);

        switch (zone)
        {
            case PanjatPinangPowerBar.PowerZone.Red:
                minValue = padding;
                maxValue = Mathf.Max(minValue, redMax - padding);
                break;

            case PanjatPinangPowerBar.PowerZone.Yellow:
                minValue = Mathf.Clamp01(redMax + padding);
                maxValue = Mathf.Max(minValue, yellowMax - padding);
                break;

            default:
                minValue = Mathf.Clamp01(yellowMax + padding);
                maxValue = Mathf.Max(minValue, 1f - padding);
                break;
        }
    }
}

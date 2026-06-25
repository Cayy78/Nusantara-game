using UnityEngine;

public static class GameplayTimerFormatter
{
    public static string FormatElapsedTime(float elapsedTime)
    {
        int totalCentiseconds = Mathf.Max(0, Mathf.FloorToInt(elapsedTime * 100f));
        int seconds = totalCentiseconds / 100;
        int centiseconds = totalCentiseconds % 100;
        return $"{seconds:00}.{centiseconds:00}";
    }
}

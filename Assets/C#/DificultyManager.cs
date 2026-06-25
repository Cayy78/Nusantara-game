using UnityEngine;

public static class DifficultyPowerSettings
{
    public static float climbPower = 1f;
    public static float slipPower = 0.5f;

    public static void SetEasy()
    {
        climbPower = 1.2f;
        slipPower = 0.2f;
    }

    public static void SetMedium()
    {
        climbPower = 1f;
        slipPower = 0.5f;
    }

    public static void SetHard()
    {
        climbPower = 0.7f;
        slipPower = 0.8f;
    }
}

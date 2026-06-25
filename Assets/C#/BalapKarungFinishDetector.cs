using UnityEngine;

public class BalapKarungFinishDetector : MonoBehaviour, IFinishCheatDetector
{
    public BalapKarungSingleManager gameManager;
    public int playerIndex = 1;

    public bool IsCheatFinishConfigured => gameManager != null && playerIndex > 0;
    public int CheatPlayerIndex => playerIndex;

    void OnTriggerEnter2D(Collider2D other)
    {
        TryRegisterFinish(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryRegisterFinish(other);
    }

    void TryRegisterFinish(Collider2D other)
    {
        if (!other.CompareTag("Finish"))
            return;

        if (gameManager != null)
            gameManager.RegisterPlayerFinish(playerIndex);
    }
}

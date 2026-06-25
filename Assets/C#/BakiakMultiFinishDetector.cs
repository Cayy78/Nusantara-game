using UnityEngine;

public class BakiakMultiFinishDetector : MonoBehaviour, IFinishCheatDetector
{
    public BakiakMultiManager gameManager;
    public int playerIndex = 1;

    public bool IsCheatFinishConfigured => gameManager != null && playerIndex > 0;
    public int CheatPlayerIndex => playerIndex;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Finish"))
            return;

        if (gameManager != null)
            gameManager.RegisterPlayerFinish(playerIndex);
    }
}

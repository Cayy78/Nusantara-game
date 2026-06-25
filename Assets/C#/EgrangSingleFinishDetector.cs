using UnityEngine;

public class EgrangSingleFinishDetector : MonoBehaviour
{
    public EgrangSingleManager gameManager;
    public int playerIndex = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Finish"))
            return;

        if (gameManager != null)
            gameManager.RegisterPlayerFinish(playerIndex);
    }
}

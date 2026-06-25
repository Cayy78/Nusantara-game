using UnityEngine;

public class LompatTaliPlayerHitDetector : MonoBehaviour
{
    public int playerIndex = 1;
    public LompatTaliMultiManager manager;
    public LompatTaliSingleManager singleManager;

    void OnEnable()
    {
        ResolveManagers();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("LompatTaliRope"))
            return;

        ResolveManagers();

        if (manager != null)
            manager.RegisterPlayerHit(playerIndex);

        if (singleManager != null)
            singleManager.RegisterPlayerHit(playerIndex);
    }

    void ResolveManagers()
    {
        if (singleManager == null)
            singleManager = FindObjectOfType<LompatTaliSingleManager>();

        if (manager == null && singleManager == null)
            manager = FindObjectOfType<LompatTaliMultiManager>();
    }
}

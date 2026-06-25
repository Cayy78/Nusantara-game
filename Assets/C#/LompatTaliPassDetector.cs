using UnityEngine;

public class LompatTaliPassDetector : MonoBehaviour
{
    public int playerIndex = 1;
    public LompatTaliMultiManager manager;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("LompatTaliRope"))
            return;

        if (manager != null)
            manager.RegisterRopePass(playerIndex);
    }
}

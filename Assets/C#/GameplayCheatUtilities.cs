using UnityEngine;

public enum GameplayCheatAxis
{
    Horizontal,
    Vertical
}

public static class GameplayCheatUtilities
{
    public static bool HasConfiguredFinishDetector(Transform targetRoot, int expectedPlayerIndex)
    {
        if (targetRoot == null)
            return false;

        MonoBehaviour[] behaviours = targetRoot.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is not IFinishCheatDetector finishDetector)
                continue;

            if (!finishDetector.IsCheatFinishConfigured)
                continue;

            if (finishDetector.CheatPlayerIndex != expectedPlayerIndex)
                continue;

            return true;
        }

        return false;
    }

    public static void TeleportToFinish(Transform target, GameplayCheatAxis axis, float offsetFromFinish = 0f)
    {
        if (target == null)
            return;

        Transform finishTransform = FindNearestFinishTransform(target.position);
        if (finishTransform == null)
            return;

        Vector3 position = target.position;

        if (axis == GameplayCheatAxis.Horizontal)
        {
            float direction = Mathf.Sign(finishTransform.position.x - position.x);
            if (Mathf.Approximately(direction, 0f))
                direction = 1f;

            position.x = finishTransform.position.x - (direction * offsetFromFinish);
        }
        else
        {
            float direction = Mathf.Sign(finishTransform.position.y - position.y);
            if (Mathf.Approximately(direction, 0f))
                direction = 1f;

            position.y = finishTransform.position.y - (direction * offsetFromFinish);
        }

        target.position = position;

        Rigidbody2D body2D = target.GetComponent<Rigidbody2D>();
        if (body2D != null)
            body2D.velocity = Vector2.zero;
    }

    static Transform FindNearestFinishTransform(Vector3 origin)
    {
        GameObject[] finishObjects = GameObject.FindGameObjectsWithTag("Finish");
        Transform nearest = null;
        float bestSqrDistance = float.MaxValue;

        foreach (GameObject finishObject in finishObjects)
        {
            if (finishObject == null)
                continue;

            float sqrDistance = (finishObject.transform.position - origin).sqrMagnitude;
            if (sqrDistance >= bestSqrDistance)
                continue;

            bestSqrDistance = sqrDistance;
            nearest = finishObject.transform;
        }

        return nearest;
    }
}

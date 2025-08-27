using UnityEngine;

public static class ScreenUtils
{
    private static readonly float UPPER_BOUND = 1.25f;
    private static readonly float LOWER_BOUND = -0.25f;

    public static bool IsOutOfBounds(Vector3 position, Camera camera)
    {
        var viewportPoint = camera.WorldToViewportPoint(position);

        return viewportPoint.x < LOWER_BOUND ||
               viewportPoint.x > UPPER_BOUND ||
               viewportPoint.y < LOWER_BOUND ||
               viewportPoint.y > UPPER_BOUND;
    }
}
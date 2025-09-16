using UnityEngine;

public class CoordinateConverter
{
    public static Vector2 PolarToCartesian(float radius, float angle)
    {
        return new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
    }
}

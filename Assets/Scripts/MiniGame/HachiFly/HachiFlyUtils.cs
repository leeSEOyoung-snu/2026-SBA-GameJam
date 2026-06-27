using UnityEngine;

public static class HachiFlyUtils
{
    /// <summary>pos가 Camera.main 뷰포트 밖(margin 포함)이면 true</summary>
    public static bool IsOutsideCamera(Vector2 pos, float margin = 1f)
    {
        var cam = Camera.main;
        if (cam == null || !cam.orthographic) return false;

        float halfH = cam.orthographicSize + margin;
        float halfW = halfH * cam.aspect + margin;
        Vector2 local = pos - (Vector2)cam.transform.position;
        return Mathf.Abs(local.x) > halfW || Mathf.Abs(local.y) > halfH;
    }
}

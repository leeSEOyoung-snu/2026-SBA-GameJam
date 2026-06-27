using UnityEngine;

// 해치 V를 따라가는 카메라
public class HachiFlyCamera : MonoBehaviour
{
    private Transform target;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new(0f, 0f, -10f);

    public void SetTarget(Transform t) => target = t;

    private void LateUpdate()
    {
        if (target == null) return;

        var targetPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }
}

using System;
using UnityEngine;

// background GameObject에 붙이세요.
// Target Cam (Cinemachine Follow)의 실제 출력 카메라를 Camera 필드에 연결합니다.
public class HachiFlyParallax : MonoBehaviour
{
    [Serializable]
    public class Layer
    {
        public Transform target;        // 자식 SpriteRenderer의 Transform
        [Range(0f, 1f)]
        public float multiplier = 0.1f; // 0 = 고정, 1 = 카메라와 동일하게 이동 (역방향)
    }

    [SerializeField] private Camera cam;
    [SerializeField] private Layer[] layers;

    private Vector3 _prevCamPos;

    private void Start()
    {
        if (cam == null) cam = Camera.main;
        _prevCamPos = cam.transform.position;
    }

    private void FixedUpdate()
    {
        Vector3 delta = cam.transform.position - _prevCamPos;

        foreach (var layer in layers)
        {
            if (layer.target == null) continue;
            // multiplier=0: 월드 고정(화면상 반대로 흐름), multiplier=1: 카메라에 고정(화면상 정지)
            layer.target.position += new Vector3(delta.x, delta.y, 0f) * layer.multiplier;
        }

        _prevCamPos = cam.transform.position;
    }
}

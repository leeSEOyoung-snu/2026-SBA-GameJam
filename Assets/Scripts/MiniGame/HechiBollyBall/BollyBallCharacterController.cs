using UnityEngine;

// 각 플레이어를 자기 진영(왼쪽/오른쪽) 안에만 가두는 컨트롤러
[RequireComponent(typeof(Rigidbody2D))]
public class BollyBallCharacterController : MonoBehaviour
{
    [Header("해치 진영 X 범위 (왼쪽)")]
    [SerializeField] private float hachiXMin = -8.5f;
    [SerializeField] private float hachiXMax = -0.3f;

    [Header("플레이어 진영 X 범위 (오른쪽)")]
    [SerializeField] private float playerXMin = 0.3f;
    [SerializeField] private float playerXMax = 8.5f;

    [Header("공통 Y 범위")]
    [SerializeField] private float yMin = -4.5f;
    [SerializeField] private float yMax =  4.5f;

    private Rigidbody2D _rb;
    private bool _isHachi;

    public void Init(bool isHachi)
    {
        _isHachi = isHachi;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        var pos = _rb.position;

        float xMin = _isHachi ? hachiXMin : playerXMin;
        float xMax = _isHachi ? hachiXMax : playerXMax;

        pos.x = Mathf.Clamp(pos.x, xMin, xMax);
        pos.y = Mathf.Clamp(pos.y, yMin, yMax);

        _rb.position = pos;
    }
}

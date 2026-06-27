using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 고정된 포탑 — 주기적으로 플레이어를 향해 미사일 발사
[RequireComponent(typeof(Rigidbody2D))]
public class HachiFlyTurret : MonoBehaviour
{
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private float fireInterval = 2f;
    [SerializeField] private float destroyMargin = 2f;

    private Transform _player;
    private HachiFlyGame _game;
    private bool _enteredCamera;

    private void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void Init(Transform player)
    {
        _player = player;
        _game   = Object.FindAnyObjectByType<HachiFlyGame>();
        StartCoroutine(FireRoutine());
    }

    private void Update()
    {
        if (_game == null || _player == null) return;
        bool outside = HachiFlyUtils.IsOutsideCamera(transform.position, destroyMargin);
        if (!outside) _enteredCamera = true;
        if (_enteredCamera && outside) Destroy(gameObject);
    }

    private IEnumerator FireRoutine()
    {
        yield return new WaitForSeconds(fireInterval * 0.5f);

        while (true)
        {
            FireMissile();
            yield return new WaitForSeconds(fireInterval);
        }
    }

    private void FireMissile()
    {
        if (missilePrefab == null || _player == null) return;

        var obj = Instantiate(missilePrefab, transform.position, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(obj, gameObject.scene);
        obj.GetComponent<HachiFlyMissile>().Init(_player, _game);
    }
}

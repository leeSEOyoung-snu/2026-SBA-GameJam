using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterMoveDust : MonoBehaviour
{
    [SerializeField] private Sprite dustSprite;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, -0.25f, 0f);
    [SerializeField] private float moveThreshold = 0.05f;
    [SerializeField] private float spawnInterval = 0.08f;
    [SerializeField] private float lifetime = 0.28f;
    [SerializeField] private float startScale = 0.2f;
    [SerializeField] private float endScale = 0.55f;
    [SerializeField] private float randomX = 0.12f;
    [SerializeField] private int sortingOrder = -1;

    private Rigidbody2D _rb;
    private float _spawnTimer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (dustSprite == null || _rb == null)
            return;

        if (_rb.linearVelocity.sqrMagnitude < moveThreshold * moveThreshold)
        {
            _spawnTimer = 0f;
            return;
        }

        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer > 0f)
            return;

        _spawnTimer = spawnInterval;
        SpawnDust();
    }

    private void SpawnDust()
    {
        var dust = new GameObject("Move Dust");
        dust.transform.position = transform.position + spawnOffset + new Vector3(Random.Range(-randomX, randomX), 0f, 0f);
        SceneManager.MoveGameObjectToScene(dust, gameObject.scene);

        var renderer = dust.AddComponent<SpriteRenderer>();
        renderer.sprite = dustSprite;
        renderer.sortingOrder = sortingOrder;

        StartCoroutine(FadeDust(dust.transform, renderer));
    }

    private IEnumerator FadeDust(Transform dustTransform, SpriteRenderer renderer)
    {
        var elapsed = 0f;
        var baseColor = renderer.color;
        var randomRotation = Random.Range(0f, 360f);
        dustTransform.rotation = Quaternion.Euler(0f, 0f, randomRotation);

        while (elapsed < lifetime)
        {
            var t = elapsed / lifetime;
            var scale = Mathf.Lerp(startScale, endScale, t);
            dustTransform.localScale = new Vector3(scale, scale, 1f);
            renderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(dustTransform.gameObject);
    }
}

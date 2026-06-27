using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PhotoHechi : MonoBehaviour
{
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float spawnX = 10f;
    [SerializeField] private float exitX = -10f;

    public bool IsInPhotoZone { get; private set; }

    public event Action OnExited;

    private float _speed;
    private bool _active;

    public void Launch()
    {
        _speed = Random.Range(minSpeed, maxSpeed);
        transform.position = new Vector3(spawnX, transform.position.y, transform.position.z);
        gameObject.SetActive(true);
        _active = true;
    }

    private void Update()
    {
        if (!_active) return;

        transform.position += Vector3.left * (_speed * Time.deltaTime);

        if (transform.position.x <= exitX)
        {
            _active = false;
            gameObject.SetActive(false);
            OnExited?.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PhotoZone"))
            IsInPhotoZone = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PhotoZone"))
            IsInPhotoZone = false;
    }
}

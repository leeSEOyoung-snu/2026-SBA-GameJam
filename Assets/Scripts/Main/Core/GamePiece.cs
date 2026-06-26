using System.Collections;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;

    public void PlaceAt(Vector3 worldPosition)
    {
        transform.position = worldPosition;
    }

    public IEnumerator MoveTo(Vector3 worldPosition)
    {
        Vector3 start = transform.position;
        float duration = Vector3.Distance(start, worldPosition) / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(start, worldPosition, elapsed / duration);
            yield return null;
        }

        transform.position = worldPosition;
    }
}

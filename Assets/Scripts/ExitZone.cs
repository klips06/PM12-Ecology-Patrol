using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ExitZone : MonoBehaviour
{
    private void Awake() => GetComponent<BoxCollider2D>().isTrigger = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        if (GameManager.Instance != null) GameManager.Instance.TryExit();
    }
}

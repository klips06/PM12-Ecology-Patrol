using UnityEngine;

/// <summary>Вариант 01: каждый предмет имеет один из двух типов.</summary>
[RequireComponent(typeof(BoxCollider2D))]
public class EcologyItem : MonoBehaviour
{
    public enum ItemKind { RecyclablePackage, HazardousBattery }
    public ItemKind kind;
    private bool collected;

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null || collected) return;
        if (GameManager.Instance != null)
            GameManager.Instance.HandleItem(this);
    }

    public void AcceptPackage()
    {
        if (kind != ItemKind.RecyclablePackage || collected) return;
        collected = true;                     // Повторный контакт не учитывается.
        gameObject.SetActive(false);
    }

    public void ResetItem()
    {
        collected = false;
        gameObject.SetActive(true);
    }
}

using UnityEngine;

/// <summary>Движение игрока с клавиатуры и экранных кнопок.</summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField, Range(2f, 8f)] private float moveSpeed = 4.2f;
    private Rigidbody2D body;
    private Vector2 startPosition;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        startPosition = body.position;
    }

    private void FixedUpdate()
    {
        GameManager manager = GameManager.Instance;
        if (manager == null || !manager.IsPlaying)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vertical += 1f;

        Vector2 keyboard = new Vector2(horizontal, vertical);
        Vector2 direction = Vector2.ClampMagnitude(keyboard + manager.MobileDirection, 1f);
        body.linearVelocity = direction * moveSpeed;
    }

    public void ResetPlayer()
    {
        body.linearVelocity = Vector2.zero;
        body.position = startPosition;
        transform.position = new Vector3(startPosition.x, startPosition.y, transform.position.z);
    }
}

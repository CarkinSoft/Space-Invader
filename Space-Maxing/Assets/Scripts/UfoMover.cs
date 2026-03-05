using UnityEngine;

public class UfoMover : MonoBehaviour
{
    public float speed = 6f;
    public float destroyWhenPastX = 10f;

    private Vector2 direction = Vector2.right;

    public void Init(Vector2 moveDirection)
    {
        direction = moveDirection.normalized;
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x) > destroyWhenPastX)
            Destroy(gameObject);
    }
}
using UnityEngine;

public class SimplePlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private bool faceMoveDirection = true;
    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(x, 0f, z);
        if (input.sqrMagnitude < 0.0001f) return;
        Vector3 dir = input.normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
        if (faceMoveDirection)
        {
            transform.forward = dir;
        }
    }
}

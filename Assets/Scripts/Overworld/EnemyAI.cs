using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float detectionRange = 5f;
    public float speed = 2f;

    public Transform player;
    private Rigidbody2D rb;
    private Vector2 movement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance < detectionRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            movement = direction;
        }
        else
        {
            movement = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }
}

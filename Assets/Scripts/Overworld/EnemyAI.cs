using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float detectionRange = 5f;
    public float speed = 2f;
    public Transform patrolPointA;
    public Transform patrolPointB;
    public float waitTime = 1.5f;

    private float waitCounter = 0;
    private bool isWaiting = false;
    private Vector3 currentTarget;
    private bool playerDetected;
    public Transform player;
    private Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        currentTarget = patrolPointB.position;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < detectionRange)
        {
            playerDetected = true;
        } else if (distance > detectionRange + 1f)
        {
            playerDetected = false;
        }

        if (playerDetected)
        {
            // Vector2.MoveTowards(rb.position, player.position, speed * Time.fixedDeltaTime);
            MoveTowards(player.position);
        } else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (isWaiting)
        {
            rb.linearVelocity = Vector2.zero;
            waitCounter -= Time.deltaTime;
            if (waitCounter <= 0)
            {
                isWaiting = false;
                currentTarget = currentTarget == patrolPointA.position ? patrolPointB.position : patrolPointA.position;
            }
            return;
        }
        
        MoveTowards(currentTarget);

        if (Vector2.Distance(transform.position, currentTarget) < 0.2f)
        {
            isWaiting = true;
            waitCounter = waitTime;
        }
    }

    private void MoveTowards(Vector3 target)
    {
        Vector2 direction = (target - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }
}

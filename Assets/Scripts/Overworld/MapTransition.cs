using Unity.Cinemachine;
using UnityEngine;

public class MapTransition : MonoBehaviour
{
    [SerializeField] Collider2D mapBoundary;
    CinemachineConfiner2D confiner;
    [SerializeField] Direction direction;
    [SerializeField] Transform teleportTargetPosition;
    [SerializeField] float additivePos = 2;

    enum Direction { Up, Down, Left, Right, Teleport }

    private void Awake()
    {
        confiner = FindAnyObjectByType<CinemachineConfiner2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            FadeTransition(collision.gameObject);
        }
    }

    async void FadeTransition(GameObject player)
    {
        PauseController.SetPause(true);

        await ScreenFader.Instance.FadeOut();

        confiner.BoundingShape2D = mapBoundary;
        UpdatePlayerPosition(player);

        await ScreenFader.Instance.FadeIn();

        PauseController.SetPause(false);
    }

    private void UpdatePlayerPosition(GameObject player)
    {
        if (direction == Direction.Teleport)
        {
            player.transform.position = teleportTargetPosition.position;
            return;
        }

        Vector3 newPos = player.transform.position;

        switch (direction)
        {
            case Direction.Up:
                newPos.y += additivePos;
                break;
            case Direction.Down:
                newPos.y -= additivePos;
                break;
            case Direction.Left:
                newPos.x -= additivePos;
                break;
            case Direction.Right:
                newPos.x += additivePos;
                break;
        }

        player.transform.position = newPos;
    }
}

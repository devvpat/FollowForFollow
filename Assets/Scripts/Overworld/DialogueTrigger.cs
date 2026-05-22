using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (TryGetComponent(out NPC npc))
        {
            Debug.Log("Starting dialogue");
            npc.OnDialogueEnded += DisableTrigger;
            npc.Interact();
        }
    }

    private void DisableTrigger()
    {
        gameObject.SetActive(false);
    }
}

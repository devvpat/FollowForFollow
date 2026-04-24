using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[System.Serializable]
public class Dialogue
{
    public string name;
    public string dialogueText;
}

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private List<Dialogue> dialogues;
    [SerializeField] private InputActionReference inputActionReference;
    [SerializeField] private TextMeshProUGUI dialogueName;
    [SerializeField] private TextMeshProUGUI dialogueText;
    private int order;

    void Start(){
        order = -1;
    }

    void OnEnable(){
        inputActionReference.action.Enable();
        inputActionReference.action.performed += OnInteractPerformed;
        inputActionReference.action.canceled += OnInteractCanceled;
    }

    void OnDisable(){
        inputActionReference.action.performed -= OnInteractPerformed;
        inputActionReference.action.canceled -= OnInteractCanceled;
        inputActionReference.action.Disable();
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx){
        
    }

    private void OnInteractCanceled(InputAction.CallbackContext ctx){
        order += 1;
        if (order < dialogues.Count){
            dialogueName.text = dialogues[order].name;
            dialogueText.text = dialogues[order].dialogueText;
        }
    }

}

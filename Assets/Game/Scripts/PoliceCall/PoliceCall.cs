using UnityEngine;
using TMPro;
public class PoliceCall : MonoBehaviour
{
    public TMP_Text dialogueText;
    private void OnMouseDown(){
        Debug.Log("Calling the police....");
        dialogueText.text="Police Department: How I can help you";
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

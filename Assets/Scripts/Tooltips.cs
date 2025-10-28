using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode()]
public class Tooltips : MonoBehaviour
{
    public TextMeshProUGUI headerField;
    public TextMeshProUGUI contentField;

    public LayoutElement layoutElement;

    public float characterWrapLimit;

    private void Update()
    {
        float headerLength = headerField.text.Length;
        //Debug.Log("Header Length: " + headerLength);
        float contentLength = contentField.text.Length;
        // Debug.Log("Content Length: " + contentLength);

        layoutElement.enabled = (headerLength > characterWrapLimit || contentLength > characterWrapLimit) ? true : false; 
        //Debug.Log("Layout Element Enabled: " + layoutElement.enabled);
    }

    
}

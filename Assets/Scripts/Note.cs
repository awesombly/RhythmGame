using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if ( rectTransform == null )
        {
            Debug.LogError( "[Not.Start] RectTransform not found." );
            return;
        }
    }

    private void Update()
    {
        rectTransform.localPosition += Vector3.down * Time.deltaTime * 600.0f;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    public RectTransform endLine;
    public GameObject notePrefab;

    private Queue<GameObject> notes = new Queue<GameObject>();

    public void SpawnNote()
    {
        GameObject instance = Instantiate( notePrefab, gameObject.transform );
        if ( instance == null )
        {
            Debug.LogError( "[SapwnNote] instance is null." );
            return;
        }

        notes.Enqueue( instance );
    }
}

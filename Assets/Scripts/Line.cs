using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    public RectTransform hitLine;
    public GameObject notePrefab;

    private Queue<Note> notes = new Queue<Note>();
    
    enum EHitRate
    {
        PERFECT = 0,
        GOOD,
        BAD,
        ENUM_SIZE
    }
    private long[] hitRangeMilliSeconds = new long[ ( int )EHitRate.ENUM_SIZE ];

    private void Awake()
    {
        hitRangeMilliSeconds[ ( int )EHitRate.PERFECT ] = 100;
        hitRangeMilliSeconds[ ( int )EHitRate.GOOD ] = 200;
        hitRangeMilliSeconds[ ( int )EHitRate.BAD ] = 300;
    }

    private void Update()
    {
        if ( notes.Count <= 0 )
        {
            return;
        }

        Note note = notes.Peek();
        if ( note == null )
        {
            Debug.LogError( "[Line.Update] note is null." );
            return;
        }
    }

    public void SpawnNote( int waveIndex, long hitMilliSeconds )
    {
        GameObject instance = Instantiate( notePrefab, gameObject.transform );
        if ( instance == null )
        {
            Debug.LogError( "[SapwnNote] instance is null." );
            return;
        }

        Note note = instance.GetComponent<Note>();
        if ( note == null )
        {
            Debug.LogError( "[SpawnNote] note is null." );
            return;
        }

        note.waveIndex = waveIndex;
        note.hitMiliSceconds = hitMilliSeconds;
        note.targetPosition = hitLine.position;
        notes.Enqueue( note );
    }
}

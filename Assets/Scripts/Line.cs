using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Line : MonoBehaviour
{
    public KeyCode hitKeyCode;
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
    private long[] hitableInterval = new long[ ( int )EHitRate.ENUM_SIZE ];

    private void Awake()
    {
        hitableInterval[ ( int )EHitRate.PERFECT ] = 75;
        hitableInterval[ ( int )EHitRate.GOOD ] = 150;
        hitableInterval[ ( int )EHitRate.BAD ] = 225;
    }

    private void Update()
    {
        while( true )
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

            // HitLine을 지나서, BAD 판정 범위로 들어올시 MISS 처리
            int interval = ( int )( GameManager.Instance.noteSpace.elapsedMilliSeconds - ( note.hitMiliSceconds + note.spawnMiliSceconds ) );
            if ( interval > hitableInterval[ ( int )EHitRate.BAD - 1 ] )
            {
                note.gameObject.SetActive( false );
                Destroy( note.gameObject );
                notes.Dequeue();
                /// + Miss 처리

                continue;
            }

            if ( Input.GetKeyDown( hitKeyCode ) )
            {
                interval = Mathf.Abs( interval );

                for ( int i = 0; i < hitableInterval.Length; ++i )
                {
                    if ( interval <= hitableInterval[ i ] )
                    {
                        note.gameObject.SetActive( false );
                        Destroy( note.gameObject );
                        notes.Dequeue();
                        /// + 처리
                        Debug.Log( "Hit! " + (EHitRate)i );
                        return;
                    }
                }
            }

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

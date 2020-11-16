using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Line : MonoBehaviour
{
    public KeyCode hitKeyCode;
    public RectTransform hitLine;
    public GameObject notePrefab;

    public Queue<Note> notes = new Queue<Note>();
    public Queue<Note> backgrountNotes = new Queue<Note>();

    private long[] hitableInterval = new long[ ( int )HitInfo.EHitRate.HIT_ENUM_COUNT ];

    public delegate void DelHitNote( Note.NoteInfo noteInfo, HitInfo.EHitRate hitRate );
    public event DelHitNote OnHitNote;

    private void Awake()
    {
        hitableInterval[ ( int )HitInfo.EHitRate.PERFECT ] = 75;
        hitableInterval[ ( int )HitInfo.EHitRate.GOOD ] = 150;
        hitableInterval[ ( int )HitInfo.EHitRate.BAD ] = 225;
    }

    private void Update()
    {
        // 노트 처리
        while( true )
        {
            if ( notes.Count <= 0 )
            {
                break;
            }

            Note note = notes.Peek();
            if ( note == null )
            {
                Debug.LogError( "[Line.Update] note is null." );
                break;
            }

            // HitLine을 지나서, BAD 판정 범위로 들어올시 MISS 처리
            int interval = ( int )( GameManager.Instance.noteSpace.elapsedMilliSeconds - ( note.hitMiliSceconds + note.spawnMiliSceconds ) );
            if ( interval > hitableInterval[ ( int )HitInfo.EHitRate.BAD - 1 ] )
            {
                RemoveNote( note );
                OnHitNote?.Invoke( note.noteInfo, HitInfo.EHitRate.MISS );
                continue;
            }

            if ( Input.GetKeyDown( hitKeyCode ) )
            {
                interval = Mathf.Abs( interval );

                for ( int i = 0; i < hitableInterval.Length; ++i )
                {
                    if ( interval <= hitableInterval[ i ] )
                    {
                        RemoveNote( note );
                        OnHitNote?.Invoke( note.noteInfo, ( HitInfo.EHitRate )i );
                        break;
                    }
                }
            }

            break;
        }

        // 배경노트 처리
        while ( true )
        {
            if ( backgrountNotes.Count <= 0 )
            {
                break;
            }

            Note note = backgrountNotes.Peek();
            if ( note == null )
            {
                Debug.LogError( "[Line.Update] bgNote is null." );
                break;
            }

            // HitLine을 지날시 Hit 처리
            int interval = ( int )( GameManager.Instance.noteSpace.elapsedMilliSeconds - ( note.hitMiliSceconds + note.spawnMiliSceconds ) );
            if ( interval >= 0 )
            {
                RemoveNote( note );
                OnHitNote?.Invoke( note.noteInfo, HitInfo.EHitRate.BACKGOUND );
                continue;
            }

            break;
        }
    }

    public void SpawnNote( Note.NoteInfo noteInfo, long hitMilliSeconds )
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

        note.noteInfo = noteInfo;
        note.hitMiliSceconds = hitMilliSeconds;
        note.targetPosition = hitLine.position;

        int first = noteInfo.LineNumber / 10;
        if ( first == 1 )
        {
            notes.Enqueue( note );
        }
        else
        {
            backgrountNotes.Enqueue( note );
        }
    }

    public void Reset()
    {
        while ( notes.Count > 0 )
        {
            RemoveNote( notes.Peek() );
        }

        while ( backgrountNotes.Count > 0 )
        {
            RemoveNote( backgrountNotes.Peek() );
        }
    }

    private void RemoveNote( Note note )
    {
        if ( note == null )
        {
            Debug.LogError( "[RemoveNote] note is null." );
            return;
        }

        note.gameObject.SetActive( false );
        Destroy( note.gameObject );

        if ( notes.Count > 0 && notes.Peek() == note )
        {
            notes.Dequeue();
        }
        else
        {
            backgrountNotes.Dequeue();
        }
    }
}

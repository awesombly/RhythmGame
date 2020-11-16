﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Line : MonoBehaviour
{
    public KeyCode hitKeyCode;
    public RectTransform hitLine;
    public GameObject notePrefab;

    public Queue<Note> notes = new Queue<Note>();
    
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
                        return;
                    }
                }
            }

            return;
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
        notes.Enqueue( note );
    }

    public void RemoveNote( Note note )
    {
        if ( note == null )
        {
            Debug.LogError( "[RemoveNote] note is null." );
            notes.Dequeue();
            return;
        }

        note.gameObject.SetActive( false );
        Destroy( note.gameObject );
        notes.Dequeue();
    }
}

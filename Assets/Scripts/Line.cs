using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Line : MonoBehaviour
{
    public KeyCode keyCode;
    public RectTransform hitLine;
    public UnityEngine.UI.Image inputEffect;

    public GameObject notePrefab;
    public GameObject bgNotePrefab;
    public GameObject longNotePrefab;
    public GameObject longNoteBodyPrefab;
    public GameObject hitEffectPrefab;

    public Queue<Note> notes = new Queue<Note>();
    public Queue<Note> backgrountNotes = new Queue<Note>();

    private long[] hitableInterval = new long[ ( int )HitInfo.EHitRate.HIT_ENUM_COUNT ];
    private Note currentLongNote;

    public delegate void DelHitNote( NoteInfo noteInfo, HitInfo.EHitRate hitRate );
    public event DelHitNote OnHitNote;

    private void Awake()
    {
        hitableInterval[ ( int )HitInfo.EHitRate.PERFECT ] = 75;
        hitableInterval[ ( int )HitInfo.EHitRate.GOOD ] = 175;
        hitableInterval[ ( int )HitInfo.EHitRate.BAD ] = 225;
    }

    private void Start()
    {
        GameManager.Instance.noteSpace.OnVisibleBgNote += OnVisibleBgNote;
        OnHitNote += SpawnHitEffect;
    }

    private void Update()
    {
        inputEffect.enabled = Input.GetKey( keyCode );

        // 노트 처리
        while ( true )
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

                    if ( Input.GetKeyDown( keyCode ) )
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

        // 배경음 노트 처리
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

        if ( currentLongNote != null )
        {
            currentLongNote.rectTransform.offsetMax = new Vector2( currentLongNote.rectTransform.offsetMax.x, gameObject.transform.position.y - currentLongNote.rectTransform.position.y );
        }
    }

    public void SpawnNote( NoteInfo noteInfo, long hitMilliSeconds )
    {
        GameObject prefab = notePrefab;
        switch ( noteInfo.NoteType )
        {
            case NoteInfo.ENoteType.BACKGROUND:
            {
                prefab = bgNotePrefab;
            }
            break;

            case NoteInfo.ENoteType.LONG:
            {
                prefab = longNotePrefab;
            }
            break;
        }

        GameObject instance = Instantiate( prefab, gameObject.transform );
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

        switch ( noteInfo.NoteType )
        {
            case NoteInfo.ENoteType.BACKGROUND:
            {
                backgrountNotes.Enqueue( note );
                note.SetVisible( GameManager.Instance.noteSpace.isVisibleBgNote );
            }
            break;

            case NoteInfo.ENoteType.LONG:
            {
                notes.Enqueue( note );

                if ( currentLongNote == null )
                {
                    GameObject longNoteInstance = Instantiate( longNoteBodyPrefab, gameObject.transform );
                    if ( longNoteInstance == null )
                    {
                        Debug.LogError( "[SapwnNote] longNoteInstance is null." );
                        return;
                    }

                    Note longNote = longNoteInstance.GetComponent<Note>();
                    if ( longNote == null )
                    {
                        Debug.LogError( "[SpawnNote] longNote is null." );
                        return;
                    }

                    longNote.noteInfo = noteInfo;
                    longNote.hitMiliSceconds = hitMilliSeconds;
                    longNote.targetPosition = hitLine.position;

                    currentLongNote = longNote;
                    //notes.Enqueue( longNote );
                }
                else
                {
                    currentLongNote.rectTransform.offsetMax = new Vector2( currentLongNote.rectTransform.offsetMax.x, note.rectTransform.position.y - currentLongNote.rectTransform.position.y );
                    currentLongNote = null;
                }
            }
            break;

            default:
            {
                notes.Enqueue( note );
            }
            break;
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

    private void SpawnHitEffect( NoteInfo noteInfo, HitInfo.EHitRate hitRate )
    {
        if ( hitRate == HitInfo.EHitRate.BACKGOUND
            || hitRate == HitInfo.EHitRate.MISS )
        {
            return;
        }

        Instantiate( hitEffectPrefab, hitLine.transform );
    }

    private void OnVisibleBgNote( bool isVisible )
    {
        foreach ( Note note in backgrountNotes )
        {
            note.SetVisible( isVisible );
        }
    }
}

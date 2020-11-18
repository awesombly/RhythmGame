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
    public Queue<Note> longNotes = new Queue<Note>();

    private long[] hitableInterval = new long[ ( int )HitInfo.EHitRate.HIT_ENUM_COUNT ];
    private Note currentLongNote;
    private long prevLongNoteHitTime = 0;

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
            long interval = GameManager.Instance.noteSpace.elapsedMilliSeconds - ( note.hitMiliSceconds + note.spawnMiliSceconds );
            if ( interval > hitableInterval[ ( int )HitInfo.EHitRate.BAD - 1 ] )
            {
                RemoveNote( note );
                OnHitNote?.Invoke( note.noteInfo, HitInfo.EHitRate.MISS );
                continue;
            }

            bool isKeyInput = false;
            if ( note.noteInfo.NoteType == NoteInfo.ENoteType.LONG_END )
            {
                isKeyInput = Input.GetKeyUp( keyCode );
            }
            else
            {
                isKeyInput = Input.GetKeyDown( keyCode );
            }

            if ( isKeyInput )
            {
                interval = Mathf.Abs( ( int )interval );

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
            long interval = GameManager.Instance.noteSpace.elapsedMilliSeconds - ( note.hitMiliSceconds + note.spawnMiliSceconds );
            if ( interval >= 0 )
            {
                RemoveNote( note );
                OnHitNote?.Invoke( note.noteInfo, HitInfo.EHitRate.BACKGOUND );
                continue;
            }

            break;
        }

        // 롱노트바디 처리
        while ( true )
        {
            if ( longNotes.Count <= 0 )
            {
                break;
            }

            Note note = longNotes.Peek();
            if ( note == null )
            {
                Debug.LogError( "[Line.Update] longNote is null." );
                break;
            }

            if ( Input.GetKey( keyCode ) )
            {
                // HitLine 지나고, 키 입력중일시 콤보 처리
                long interval = GameManager.Instance.noteSpace.elapsedMilliSeconds - ( note.hitMiliSceconds + note.spawnMiliSceconds );
                if ( interval >= 0 )
                {
                    long deltaTime = GameManager.Instance.noteSpace.elapsedMilliSeconds - prevLongNoteHitTime;
                    if ( deltaTime >= GameManager.Instance.noteSpace.milliSecondsPerBit )
                    {
                        prevLongNoteHitTime = GameManager.Instance.noteSpace.elapsedMilliSeconds;
                        OnHitNote?.Invoke( note.noteInfo, HitInfo.EHitRate.DUMMY );
                    }
                }
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
                    // 롱노트 시작시
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

                    longNote.noteInfo = new NoteInfo();
                    longNote.noteInfo.LineIndex = noteInfo.LineIndex;
                    longNote.noteInfo.WaveIndex = 0;
                    longNote.noteInfo.NoteType = NoteInfo.ENoteType.LONG_BODY;
                    longNote.hitMiliSceconds = hitMilliSeconds;
                    longNote.targetPosition = hitLine.position;

                    currentLongNote = longNote;
                    longNotes.Enqueue( longNote );
                }
                else
                {
                    // 롱노트 종료시
                    note.linkedNote = currentLongNote;
                    note.noteInfo.NoteType = NoteInfo.ENoteType.LONG_END;
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

        if ( note.linkedNote != null && note.linkedNote != note )
        {
            RemoveNote( note.linkedNote );
        }

        note.gameObject.SetActive( false );
        Destroy( note.gameObject );

        switch ( note.noteInfo.NoteType )
        {
            case NoteInfo.ENoteType.BACKGROUND:
            {
                backgrountNotes.Dequeue();
            }
            break;

            case NoteInfo.ENoteType.LONG_BODY:
            {
                longNotes.Dequeue();
            }
            break;

            default:
            {
                notes.Dequeue();
            }
            break;
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

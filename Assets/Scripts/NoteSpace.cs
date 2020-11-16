using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpace : MonoBehaviour
{
    public List<Line> lines;

    private MusicStatus.MusicInfo musicInfo;
    System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

    [HideInInspector]
    public long elapsedMilliSeconds;
    private long milliSecondsPerNode;
    private long milliSecondsPerBit;

    private int currentNode;
    private int currentBit;
    private int currentTotalBit;

    public delegate void DelChangeResume( bool isResume );
    public event DelChangeResume OnChangeResume;

    private void Start()
    {
        GameManager.Instance.OnStartGame += OnStartGame;
        GameManager.Instance.OnEndGame += OnEndGame;
        gameObject.SetActive( false );
    }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.Escape ) )
        {
            if ( stopWatch.IsRunning )
            {
                stopWatch.Stop();
            }
            else
            {
                stopWatch.Start();
            }

            OnChangeResume?.Invoke( !stopWatch.IsRunning );
        }

        if ( !stopWatch.IsRunning )
        {
            return;
        }

        elapsedMilliSeconds = stopWatch.ElapsedMilliseconds;
        // ex) 66 = 1,000 / 15
        long elapsedBit = elapsedMilliSeconds / milliSecondsPerBit;

        while ( currentTotalBit < elapsedBit )
        {
            AddBitCount( 1 );
            
            if ( !musicInfo.NoteInfoList.ContainsKey( currentNode ) )
            {
                continue;
            }

            LinkedList<Note.NoteInfo> noteList = musicInfo.NoteInfoList[ currentNode ][ currentBit ];
            if ( noteList == null )
            {
                continue;
            }

            foreach( var noteInfo in noteList )
            {
                int second = noteInfo.LineNumber % 10;

                // 노트가 생성되고 HitLine에 도달할때까지의 시간
                long hitMilliSeconds = ( currentTotalBit + GameManager.Instance.musicStatus.noteDelayBit ) * milliSecondsPerBit - elapsedMilliSeconds;
                lines[ second - 1 ].SpawnNote( noteInfo, hitMilliSeconds );
            }
        }
    }

    private void AddBitCount( int value )
    {
        currentBit += value;

        int dividePerNode = ( MusicStatus.DividePerNode - 1 );
        if ( currentBit >= dividePerNode )
        {
            currentNode += currentBit / dividePerNode;
            currentBit = currentBit % dividePerNode;
        }

        // ex) 65 = ( 2 * 32 ) + 1
        currentTotalBit = ( currentNode * MusicStatus.DividePerNode ) + currentBit;
    }

    private void OnStartGame()
    {
        foreach ( Line line in lines )
        {
            line.Reset();
        }

        musicInfo = GameManager.Instance.musicStatus.GetMusicInfo();
        // ex) 480 = 60000 / 125
        milliSecondsPerNode = 60000 / musicInfo.Bpm;
        // ex) 15 = 480 / 32
        milliSecondsPerBit = milliSecondsPerNode / MusicStatus.DividePerNode * 4; // 임시로 4배속 처리
        currentNode = 0;
        currentBit = 0;
        currentTotalBit = 0;

        stopWatch.Reset();
        stopWatch.Start();
        OnChangeResume?.Invoke( false );
    }

    private void OnEndGame()
    {
        foreach ( Line line in lines )
        {
            line.Reset();
        }

        stopWatch.Reset();
        OnChangeResume?.Invoke( false );
    }
}

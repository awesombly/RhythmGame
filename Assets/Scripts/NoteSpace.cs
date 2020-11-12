using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpace : MonoBehaviour
{
    public int spawnToHitBit = 128;
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

    
    private void Start()
    {
        GameManager.Instance.OnStartGame += OnStartGame;
        gameObject.SetActive( false );
    }

    private void Update()
    {
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

            LinkedList<MusicStatus.MusicInfo.NoteInfo> noteList = musicInfo.NoteInfoList[ currentNode ][ currentBit ];
            if ( noteList == null )
            {
                continue;
            }

            foreach( var note in noteList )
            {
                int first = note.LineNumber / 10;
                int second = note.LineNumber % 10;

                if ( first != 1 )
                {
                    continue;
                }

                long hitMilliSeconds = ( currentTotalBit + spawnToHitBit ) * milliSecondsPerBit - elapsedMilliSeconds;
                lines[ second - 1 ].SpawnNote( note.WaveIndex, hitMilliSeconds );
            }
        }
    }

    private void AddBitCount( int value )
    {
        currentBit += value;

        int dividePerNode = ( MusicStatus.MusicInfo.DividePerNode - 1 );
        if ( currentBit >= dividePerNode )
        {
            currentNode += currentBit / dividePerNode;
            currentBit = currentBit % dividePerNode;
        }

        // ex) 65 = ( 2 * 32 ) + 1
        currentTotalBit = ( currentNode * MusicStatus.MusicInfo.DividePerNode ) + currentBit;
    }

    private void OnStartGame()
    {
        musicInfo = GameManager.Instance.musicStatus.GetMusicInfo();
        // ex) 480 = 60000 / 125
        milliSecondsPerNode = 60000 / musicInfo.Bpm;
        // ex) 15 = 480 / 32
        milliSecondsPerBit = milliSecondsPerNode / MusicStatus.MusicInfo.DividePerNode;
        currentNode = 0;
        currentBit = 0;

        stopWatch.Reset();
        stopWatch.Start();
    }
}

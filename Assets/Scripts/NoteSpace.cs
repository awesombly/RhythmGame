using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpace : MonoBehaviour
{
    [Serializable]
    public struct KeyInfo
    {
        public List<KeyCode> KeyCodes;
    }
    public List<KeyInfo> keyInfos;
    public List<Color> lineColors;

    public List<Line> lines;
    public UnityEngine.UI.Slider progressSlider;

    [Serializable]
    public struct HitRateForUI
    {
        public HitInfo.EHitRate HitRate;
        public UnityEngine.UI.Text CountText;
    }
    public List<HitRateForUI> hitRateTexts;

    [Serializable]
    public struct ScoreInfo
    {
        public HitInfo.EHitRate HitRate;
        public double ScoreRate;
    }
    public List<ScoreInfo> scoreByHitRate;
    public UnityEngine.UI.Text scoreText;
    public double maxScore;
    private double currentScore;
    private double CurrentScore
    {
        get { return currentScore; }
        set
        {
            currentScore = value;
            // 소수점 없는 자릿수 구분 표기. ex) 123,456
            scoreText.text = Mathf.RoundToInt( ( float )currentScore ).ToString( "N0" );
        }
    }

    private MusicStatus.MusicInfo musicInfo;
    private int maxTotalBit;
    private Dictionary<HitInfo.EHitRate, int/*count*/> currentHitCounts = new Dictionary<HitInfo.EHitRate, int/*count*/>();
    System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

    [HideInInspector]
    public long elapsedMilliSeconds;
    private long milliSecondsPerNode;
    [HideInInspector]
    public long milliSecondsPerBit;

    private int currentNode;
    private int currentBit;
    private int currentTotalBit;

    public delegate void DelChangeResume( bool isResume );
    public event DelChangeResume OnChangeResume;

    public delegate void DelVisibleBgNote( bool isVisible );
    public event DelVisibleBgNote OnVisibleBgNote;
    [HideInInspector]
    public bool isVisibleBgNote = false;

    private void Start()
    {
        GameManager.Instance.OnStartGame += OnStartGame;
        GameManager.Instance.OnEndGame += OnEndGame;

        foreach ( Line line in GameManager.Instance.noteSpace.lines )
        {
            line.OnHitNote += OnHitNote;
        }

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

        if ( Input.GetKeyDown( KeyCode.Insert ) )
        {
            isVisibleBgNote = !isVisibleBgNote;
            OnVisibleBgNote?.Invoke( isVisibleBgNote );
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

            LinkedList<NoteInfo> noteList = musicInfo.NoteInfoList[ currentNode ][ currentBit ];
            if ( noteList == null )
            {
                continue;
            }

            foreach ( var noteInfo in noteList )
            {
                // 노트가 생성되고 HitLine에 도달할때까지의 시간
                long hitMilliSeconds = ( currentTotalBit + GameManager.Instance.musicStatus.noteDelayBit ) * milliSecondsPerBit - elapsedMilliSeconds;
                lines[ noteInfo.LineIndex ].SpawnNote( noteInfo, hitMilliSeconds );
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

        progressSlider.value = ( float )currentTotalBit / maxTotalBit;
    }

    private void OnStartGame()
    {
        foreach ( Line line in lines )
        {
            line.Reset();
        }

        CurrentScore = 0;
        currentHitCounts.Clear();
        musicInfo = GameManager.Instance.musicStatus.GetMusicInfo();
        
        maxTotalBit = 0;
        foreach ( int node in musicInfo.NoteInfoList.Keys )
        {
            maxTotalBit = Mathf.Max( maxTotalBit, node * MusicStatus.DividePerNode );
        }

        foreach ( HitRateForUI info in hitRateTexts )
        {
            info.CountText.text = "0";
        }

        // 라인 활성화, 키설정
        {
            KeyInfo keyInfo = keyInfos.Find( info => { return info.KeyCodes.Count == musicInfo.EnableLines.Count; } );

            int enableLineIndex = 0;
            for ( int i = 0; i < lines.Count; ++i )
            {
                bool isEnable = musicInfo.EnableLines.Contains( i );
                lines[ i ].gameObject.SetActive( isEnable );

                if ( isEnable )
                {
                    if ( keyInfo.KeyCodes == null )
                    {
                        Debug.LogError( "[OnStartGame] keyCodes is null. lineCount = " + musicInfo.EnableLines.Count );
                        continue;
                    }

                    lines[ i ].keyCode = keyInfo.KeyCodes[ enableLineIndex ];

                    UnityEngine.UI.Image image = lines[ i ].GetComponent<UnityEngine.UI.Image>();
                    if ( image != null )
                    {
                        image.color = lineColors[ enableLineIndex % lineColors.Count ];
                    }

                    ++enableLineIndex;
                }
            }
        }

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

    private void OnHitNote( NoteInfo noteInfo, HitInfo.EHitRate hitRate )
    {
        if ( hitRate == HitInfo.EHitRate.BACKGOUND
            || hitRate == HitInfo.EHitRate.DUMMY )
        {
            return;
        }

        if ( !currentHitCounts.ContainsKey( hitRate ) )
        {
            currentHitCounts.Add( hitRate, 0 );
        }
        ++currentHitCounts[ hitRate ];
        HitRateForUI hitForUI = hitRateTexts.Find( info => { return info.HitRate == hitRate; } );
        hitForUI.CountText.text = currentHitCounts[ hitRate ].ToString();

        ScoreInfo scoreInfo = scoreByHitRate.Find( info => { return info.HitRate == hitRate; } );

        CurrentScore += ( maxScore / musicInfo.TotalNoteCount ) * scoreInfo.ScoreRate;
    }
}

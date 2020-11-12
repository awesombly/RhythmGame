using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.UIElements;
using UnityEngine;

public class MusicStatus : MonoBehaviour
{
    public static int DividePerNode = 32; // 한 마디를 몇 박자로 나눠 처리할건지

    private float noteSpeed;
    public float NoteSpeed
    {
        get { return noteSpeed; }
        set
        {
            noteSpeed = value;
            // 노트 생성시 4박자후 HitLine에 도착
            noteDelayBit = ( int )( DividePerNode * 4 / noteSpeed );
        }
    }
    public int noteDelayBit;

    public delegate void DelChangeNoteSpeed();
    public event DelChangeNoteSpeed OnChangeNoteSpeed;

    public struct MusicInfo
    {
        // HeaderData
        public string Genre;
        public string Title;
        public string Artist;
        public string StageFile;
        public int Bpm;
        public int Level;
        public int Difficulty;
        public Dictionary< int/*index*/, string/*wave*/ > WaveList;

        public struct NoteInfo
        {
            public int WaveIndex;
            public int LineNumber;
        }
        // MainData
        public Dictionary< int/*noteIndex*/, List< LinkedList< NoteInfo > > > NoteInfoList;

        public void SetHeaderInfo( string tag, string data )
        {
            if ( tag == null || data == null )
            {
                Debug.LogWarning( "[SetHeaderInfo] null parameter. tag = " + tag + ", data = " + data );
                return;
            }

            if ( tag == "#GENRE" )
            {
                Genre = data;
            }
            else if ( tag == "#TITLE" )
            {
                Title = data;
            }
            else if ( tag == "#ARTIST" )
            {
                Artist = data;
            }
            else if ( tag == "#STAGEFILE" )
            {
                StageFile = data;
            }
            else if ( tag == "#BPM" )
            {
                int.TryParse( data, out Bpm );
            }
            else if ( tag == "#PLAYLEVEL" )
            {
                int.TryParse( data, out Level );
            }
            else if ( tag == "#RANK" )
            {
                int.TryParse( data, out Difficulty );
            }
            else if ( tag == "#PLAYER" || tag == "#TOTAL" )
            {
                ///
            }
            else if ( tag.StartsWith( "#WAV" ) )
            {
                string hexIndex = tag.Substring( 4 );

                int index = Convert.ToInt32( hexIndex, 16 );

                if ( WaveList == null )
                {
                    WaveList = new Dictionary<int/*index*/, string/*wave*/ >();
                }

                WaveList[ index ] = data;
            }
            else if ( tag.StartsWith( "#BMP" ) )
            {
                // 처리 추가
                // +LNTYPE1
            }
            else
            {
                Debug.Log( "[SetHeaderInfo] tag = " + tag );
            }
        }

        public void SetMainInfo( string numberData, string noteData )
        {
            if ( numberData == null || noteData == null )
            {
                Debug.LogWarning( "[SetMainInfo] null parameter. numberData = " + numberData + ", data = " + noteData );
                return;
            }

            // numberData = #xxxyy
            // xxx : 마디(비트?) 번호, yy : 노트라인 번호
            int nodeNumber = int.Parse( numberData.Substring( 1, 3 ) );
            int lineNumber = int.Parse( numberData.Substring( 4, 2 ) );

            if ( NoteInfoList == null )
            {
                NoteInfoList = new Dictionary<int/*noteIndex*/, List<LinkedList<NoteInfo>>>();
            }

            if ( !NoteInfoList.ContainsKey( nodeNumber ) )
            {
                NoteInfoList.Add( nodeNumber, new List<LinkedList<NoteInfo>>() );
            }

            List<LinkedList<NoteInfo>> noteList = NoteInfoList[ nodeNumber ];
            if ( noteList.Count <= 0 )
            {
                noteList.Capacity = DividePerNode;
                // 마지막 노트는 다음 마디에서 재생하므로 제외
                noteList.AddRange( new LinkedList<NoteInfo>[ DividePerNode - 1 ] );
            }

            // noteData = 0000006E = 한 마디중 노트위치
            // 2자씩 끊어 나누며, 각 번호는 WaveList에 등록된 번호
            for ( int i = 0; i < noteData.Length; i += 2 )
            {
                int waveIndex = Convert.ToInt32( noteData.Substring( i, 2 ), 16 );
                if ( waveIndex == 0 )
                {
                    continue;
                }

                // DividePerNode에 따라 해당 노트가 어느 위치에 있을지 계산
                // ex) 16 = 2 * 32 / 4
                int noteIndex = ( i * DividePerNode ) / noteData.Length;

                if ( noteList[ noteIndex ] == null )
                {
                    noteList[ noteIndex ] = new LinkedList<NoteInfo>();
                }

                NoteInfo info;
                info.WaveIndex = waveIndex;
                info.LineNumber = lineNumber;
                noteList[ noteIndex ].AddLast( info );
            }
        }
    }
    private Dictionary<string/*Title*/, MusicInfo> musicInfos;

    [Serializable]
    public struct StatusUI
    {
        public UnityEngine.UI.Text Genre;
        public UnityEngine.UI.Text Title;
        public UnityEngine.UI.Text Artist;
        public UnityEngine.UI.Text Bpm;
        public UnityEngine.UI.Text Level;
        public UnityEngine.UI.Text Difficulty;

        public void SetText( MusicInfo info )
        {
            if ( Genre != null )
            {
                Genre.text = info.Genre;
            }

            if ( Title != null )
            {
                Title.text = info.Title;
            }

            if ( Artist != null )
            {
                Artist.text = info.Artist;
            }

            if ( Bpm != null )
            {
                Bpm.text = info.Bpm.ToString();
            }

            if ( Level != null )
            {
                Level.text = info.Level.ToString();
            }

            if ( Difficulty != null )
            {
                Difficulty.text = info.Difficulty.ToString();
            }
        }
    }
    [SerializeField]
    private StatusUI statusUI;

    [HideInInspector]
    public string selectedTitle;

    private void Awake()
    {
        if ( musicInfos == null )
        {
            musicInfos = new Dictionary<string/*Title*/, MusicInfo>();
        }
    }

    private void Start()
    {
        GameManager.Instance.musicList.OnSelectMusic += OnSelectMusic;
        NoteSpeed = 1.0f;
    }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.PageUp ) )
        {
            NoteSpeed += 0.1f;
            OnChangeNoteSpeed?.Invoke();
            Debug.Log( "NoteSpeed = " + NoteSpeed );
        }

        if ( Input.GetKeyDown( KeyCode.PageDown ) )
        {
            NoteSpeed = Mathf.Max( noteSpeed - 0.1f, 0.1f );
            OnChangeNoteSpeed?.Invoke();
            Debug.Log( "NoteSpeed = " + NoteSpeed );
        }
    }

    public string ReadBmsFile( string bmsPath )
    {
        if ( musicInfos.ContainsKey( bmsPath ) )
        {
            return "";
        }

        FileInfo fileInfo = new FileInfo( bmsPath );
        if ( fileInfo == null )
        {
            Debug.LogError( "file not found." );
            return "";
        }

        MusicInfo info = new MusicInfo();
        StreamReader streamReader = fileInfo.OpenText();

        // HeaderData 파싱
        while ( !streamReader.EndOfStream )
        {
            string line = streamReader.ReadLine();
            if ( line == null )
            {
                Debug.LogError( "[ReadBmsFile] line is null." );
                return "";
            }

            if ( line.StartsWith( "*---------------------- MAIN DATA FIELD" ) )
            {
                break;
            }

            if ( line.Length <= 0 || !line.StartsWith( "#" ) )
            {
                continue;
            }

            int separator = line.IndexOf( ' ' );
            if ( separator <= 0 )
            {
                continue;
            }

            // ex) #GENRE Epic Dance -> "#GENRE", "Epic Dance"
            info.SetHeaderInfo( line.Substring( 0, separator ), line.Substring( separator + 1 ) );
        }

        if ( info.Title.Length <= 0 )
        {
            Debug.LogError( "[ReadBmsFile] Title is empty." );
            return "";
        }

        // MainData 파싱
        while ( !streamReader.EndOfStream )
        {
            string line = streamReader.ReadLine();
            if ( line == null )
            {
                Debug.LogError( "[ReadBmsFile] line is null." );
                return "";
            }

            if ( line.Length <= 0 || !line.StartsWith( "#" ) )
            {
                continue;
            }

            int separator = line.IndexOf( ':' );
            if ( separator <= 0 )
            {
                continue;
            }

            // ex) #01154:00000000000012000000001200000000 -> "#01154", "00000000000012000000001200000000"
            info.SetMainInfo( line.Substring( 0, separator ), line.Substring( separator + 1 ) );
        }

        musicInfos[ info.Title ] = info;

        return info.Title;
    }

    public MusicInfo GetMusicInfo( string title = "" )
    {
        if ( title.Length <= 0 )
        {
            if ( !musicInfos.ContainsKey( selectedTitle ) )
            {
                Debug.LogError( "[GetMusicInfo] music not found. selectedTitle = " + selectedTitle );
                return new MusicInfo();
            }
            return musicInfos[ selectedTitle ];
        }

        if ( !musicInfos.ContainsKey( title ) )
        {
            Debug.LogError( "[GetMusicInfo] music not found. title = " + title );
            return new MusicInfo();
        }

        return musicInfos[ title ];
    }

    private void OnSelectMusic( string title )
    {
        selectedTitle = title;
        statusUI.SetText( GetMusicInfo( title ) );
    }
}

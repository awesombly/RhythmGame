using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.UIElements;
using UnityEngine;

public class MusicStatus : MonoBehaviour
{
    struct MusicInfo
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
            else if ( tag.StartsWith( "#BPM" ) )
            {
                // 처리 추가
                // +LNTYPE1
            }
            else
            {
                Debug.Log( "[SetHeaderInfo] tag = " + tag );
            }
        }
    }
    Dictionary<string/*Title?*/, MusicInfo> MusicInfos;

    FileInfo fileInfo;
    StreamReader streamReader;

    void Start()
    {
        ReadBmsHeaderFile( "Assets/Musics/_utterfly_shade/BS_4KEY_Normal.bml" );
    }

    void Update()
    {
        
    }

    public void ReadBmsHeaderFile( string bmsPath )
    {
        fileInfo = new FileInfo( bmsPath );
        if ( fileInfo == null )
        {
            Debug.LogError( "file not found." );
            return;
        }

        MusicInfo info = new MusicInfo();
        streamReader = fileInfo.OpenText();

        // HeaderData 파싱
        while ( !streamReader.EndOfStream )
        {
            string line = streamReader.ReadLine();
            if ( line == null )
            {
                Debug.LogError( "[ReadBmsHeaderFile] line is null." );
                return;
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
            Debug.LogError( "[ReadBmsHeaderFile] Title is empty." );
            return;
        }

        MusicInfos[ info.Title ] = info;
    }
}

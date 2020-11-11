using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MusicList : MonoBehaviour
{
    public MusicStatus musicStatus;
    public GameObject content;
    public GameObject musicListElement;

    public string musicListPath;


    private LinkedList<GameObject> musicList;
    private GameObject selectedMusic;
    
    void Awake()
    {
        if ( musicList == null )
        {
            musicList = new LinkedList<GameObject>();
        }
    }

    void Start()
    {
        ReadMusicList( musicListPath );
    }

    void Update()
    {
        
    }

    public void ReadMusicList( string filePath )
    {
        if ( musicStatus == null )
        {
            return;
        }

        FileInfo fileInfo = new FileInfo( filePath );
        if ( fileInfo == null )
        {
            Debug.LogError( "file not found." );
            return;
        }

        StreamReader streamReader = fileInfo.OpenText();
        // 첫줄은 기본 경로로 사용
        string basePath = streamReader.ReadLine();

        while ( !streamReader.EndOfStream )
        {
            string line = streamReader.ReadLine();
            if ( line == null )
            {
                Debug.LogError( "[ReadMusicList] line is null." );
                return;
            }

            if ( line.Length <= 0 )
            {
                continue;
            }

            MusicStatus.MusicInfo musicInfo = musicStatus.GetMusicInfo( basePath + line );
            AddMusicList( musicInfo.Title );
        }
    }

    public void AddMusicList( string title )
    {
        if ( content == null )
        {
            Debug.LogError( "[AddMusicList] content is null." );
            return;
        }

        GameObject instance = Instantiate( musicListElement, content.transform );
        instance.name = title;

        UnityEngine.UI.Text text = instance.GetComponentInChildren<UnityEngine.UI.Text>();
        if ( text != null )
        {
            text.text = title;
        }

        musicList.AddLast( instance );
    }
}

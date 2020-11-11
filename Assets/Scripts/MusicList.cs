using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MusicList : MonoBehaviour
{
    public GameObject content;
    public GameObject musicElementPrefab;

    public string musicListPath;

    private LinkedList<GameObject> musicList;
    private GameObject selectedMusic;

    public delegate void DelSelectMusic( string title );
    public event DelSelectMusic OnSelectMusic;

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

        if ( musicList.Count <= 0 )
        {
            Debug.LogError( "[MusicList.Start] musicList is empty." );
            return;
        }
        SelectMusicElement( musicList.First.Value );
    }

    public void ReadMusicList( string filePath )
    {
        if ( GameManager.Instance.musicStatus == null )
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

            string title = GameManager.Instance.musicStatus.ReadBmsFile( basePath + line );
            MusicStatus.MusicInfo musicInfo = GameManager.Instance.musicStatus.GetMusicInfo( title );
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

        GameObject instance = Instantiate( musicElementPrefab, content.transform );
        instance.name = title;

        UnityEngine.UI.Text text = instance.GetComponentInChildren<UnityEngine.UI.Text>();
        if ( text != null )
        {
            text.text = title;
        }

        musicList.AddLast( instance );
    }

    public void SelectMusicElement( GameObject target )
    {
        if ( selectedMusic != null )
        {
            MusicElement prevElement = selectedMusic.GetComponent<MusicElement>();
            if ( prevElement == null )
            {
                Debug.LogError( "[SelectMusicElement] invalid selectedMusic. name = " + selectedMusic.name );
                return;
            }
            prevElement.selectedImage.SetActive( false );
        }

        selectedMusic = target;
        if ( selectedMusic == null )
        {
            OnSelectMusic?.Invoke( "" );
            return;
        }

        MusicElement targetElement = selectedMusic.GetComponent<MusicElement>();
        if ( targetElement == null )
        {
            Debug.LogError( "[SelectMusicElement] invalid target. name = " + selectedMusic.name );
            return;
        }
        targetElement.selectedImage.SetActive( true );

        OnSelectMusic?.Invoke( selectedMusic.name );
    }
}

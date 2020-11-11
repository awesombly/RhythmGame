using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static public GameManager Instance { get; set; }

    public MusicList musicList;
    public MusicStatus musicStatus;
    public NoteSpace noteSpace;

    public delegate void DelStartGame();
    public event DelStartGame OnStartGame;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if ( Input.GetKeyDown( KeyCode.T ) )
        {
            StartGame();
        }
    }

    void StartGame()
    {
        musicList.gameObject.SetActive( false );
        noteSpace.gameObject.SetActive( true );
        OnStartGame?.Invoke();
    }
}

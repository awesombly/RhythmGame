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
    public HitInfo hitInfo;

    public delegate void DelStartGame();
    public event DelStartGame OnStartGame;

    public delegate void DelEndGame();
    public event DelEndGame OnEndGame;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.T ) )
        {
            StartGame();
        }

        if ( Input.GetKeyDown( KeyCode.R ) )
        {
            EndGame();
        }
    }

    private void StartGame()
    {
        musicList.gameObject.SetActive( false );
        noteSpace.gameObject.SetActive( true );
        OnStartGame?.Invoke();
    }

    private void EndGame()
    {
        noteSpace.gameObject.SetActive( false );
        musicList.gameObject.SetActive( true );
        OnEndGame?.Invoke();
    }
}

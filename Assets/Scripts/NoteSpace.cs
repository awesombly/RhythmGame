using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpace : MonoBehaviour
{
    public List<Line> lines;

    MusicStatus.MusicInfo musicInfo;

    void Start()
    {
        GameManager.Instance.OnStartGame += OnStartGame;
    }

    public void OnStartGame()
    {
        musicInfo = GameManager.Instance.musicStatus.GetMusicInfo();
    }
}

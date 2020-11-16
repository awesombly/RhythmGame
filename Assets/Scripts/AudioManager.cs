using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private Dictionary<int/*index*/, AudioClip/*wave*/ > waveClipList = new Dictionary<int/*index*/, AudioClip/*wave*/ >();
    private LinkedList<AudioSource> audioSources = new LinkedList<AudioSource>();

    void Start()
    {
        GameManager.Instance.OnStartGame += OnGameStart;

        foreach ( Line line in GameManager.Instance.noteSpace.lines )
        {
            line.OnHitNote += OnHitNote;
        }
    }

    private void OnGameStart()
    {
        waveClipList.Clear();

        MusicStatus.MusicInfo musicInfo = GameManager.Instance.musicStatus.GetMusicInfo();
        foreach ( var pair in musicInfo.WaveList )
        {
            // 확장자 제거
            string path = pair.Value.Substring( 0, pair.Value.IndexOf( '.' ) );

            AudioClip audioClip = Resources.Load( path ) as AudioClip;
            if ( audioClip == null )
            {
                Debug.LogError( "[OnGameStart] audioClip is null. path = " + path );
                continue;
            }

            waveClipList.Add( pair.Key, audioClip );
        }
    }

    private void OnHitNote( Note.NoteInfo noteInfo, HitInfo.EHitRate hitRate )
    {
        if ( !waveClipList.ContainsKey( noteInfo.WaveIndex ) )
        {
            return;
        }
        if ( hitRate == HitInfo.EHitRate.MISS )
        {
            return;
        }

        AudioSource audioSource = GetAudioSource();
        audioSource.clip = waveClipList[ noteInfo.WaveIndex ];
        audioSource.Play();
    }

    private AudioSource GetAudioSource()
    {
        foreach ( AudioSource audio in audioSources )
        {
            if ( !audio.isPlaying )
            {
                return audio;
            }
        }

        AudioSource newAudioSource = gameObject.AddComponent<AudioSource>();
        newAudioSource.playOnAwake = false;
        audioSources.AddLast( newAudioSource );

        return newAudioSource;
    }
}

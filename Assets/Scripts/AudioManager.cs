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
            string path = Path.GetFileNameWithoutExtension( pair.Value );
            AudioClip audioClip = Resources.Load( path ) as AudioClip;
            if ( audioClip == null )
            {
                Debug.LogError( "[OnGameStart] audioClip is null. path = " + path );
                continue;
            }

            waveClipList.Add( pair.Key, audioClip );
        }
    }

    private void OnHitNote( NoteInfo noteInfo, HitInfo.EHitRate hitRate )
    {
        if ( hitRate == HitInfo.EHitRate.MISS
            || hitRate == HitInfo.EHitRate.DUMMY )
        {
            return;
        }

        if ( !waveClipList.ContainsKey( noteInfo.WaveIndex ) )
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
        newAudioSource.volume = 0.35f;
        audioSources.AddLast( newAudioSource );

        return newAudioSource;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicElement : MonoBehaviour
{
    public GameObject selectedImage;

    [HideInInspector]
    public MusicList musicList;

    public void OnSelected()
    {
        if ( musicList == null )
        {
            Debug.LogError( "[OnSelected] musicList is null." );
            return;
        }

        musicList.SelectMusicElement( gameObject );
    }
}

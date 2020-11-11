using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicElement : MonoBehaviour
{
    public GameObject selectedImage;

    public void OnSelected()
    {
        if ( GameManager.Instance.musicList == null )
        {
            Debug.LogError( "[OnSelected] musicList is null." );
            return;
        }

        GameManager.Instance.musicList.SelectMusicElement( gameObject );
    }
}

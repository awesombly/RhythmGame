using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NoteInfo
{
    public enum ENoteType
    {
        BACKGROUND = 0,
        NORMAL = 1,
        NORMAL2,
        NORMAL3,
        NORMAL4,
        LONG,
        NORMAL6,
        NORMAL7,
        NORMAL8,
        NORMAL9,
    }

    public int WaveIndex;
    public int LineIndex;
    public ENoteType NoteType;
}

public class Note : MonoBehaviour
{
    public NoteInfo noteInfo;

    [HideInInspector]
    public RectTransform rectTransform;
    [HideInInspector]
    public long hitMiliSceconds;
    [HideInInspector]
    public long spawnMiliSceconds;

    [HideInInspector]
    public Vector3 targetPosition;
    private Vector3 spawnPosition;

    private UnityEngine.UI.Image image;

    private void Awake()
    {
        image = GetComponent<UnityEngine.UI.Image>();
        if ( image == null )
        {
            Debug.LogError( "[Note.Start] image not found." );
            return;
        }

        rectTransform = GetComponent<RectTransform>();
        if ( rectTransform == null )
        {
            Debug.LogError( "[Note.Start] RectTransform not found." );
            return;
        }

        spawnPosition = rectTransform.position;
        spawnMiliSceconds = GameManager.Instance.noteSpace.elapsedMilliSeconds;
    }

    private void Update()
    {
        // hit 판정까지 시간 비율 ( 0.0 : 스폰시, 1.0 : 정확한 hit 타이밍 )
        // deltaTime을 이용한 이동방식은 정확한 계산이 안되어 hit까지 시간으로 계산함
        float rate = ( float )( ( GameManager.Instance.noteSpace.elapsedMilliSeconds - spawnMiliSceconds ) / ( double )hitMiliSceconds );

        // Lerp() 사용시 1.0 이상은 계산이 안됨
        rectTransform.position = spawnPosition + ( targetPosition - spawnPosition ) * rate;
    }

    public void SetVisible( bool isVisible )
    {
        image.enabled = isVisible;
    }
}

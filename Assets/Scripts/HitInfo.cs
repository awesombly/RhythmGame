using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class HitInfo : MonoBehaviour
{
    public enum EHitRate
    {
        PERFECT = 0,
        GOOD,
        BAD,
        HIT_ENUM_COUNT,
        MISS,
        BACKGOUND,
    }

    public GameObject perfectDisplay;
    public GameObject goodDisplay;
    public GameObject badDisplay;
    public GameObject missDisplay;

    public GameObject resumeDisplay;

    public GameObject comboDisplay;
    public UnityEngine.UI.Text comboText;

    public float hitDisplayTime;
    private float remainedDisplayTime;

    private int comboCount = 0;
    private int ComboCount
    {
        get { return comboCount; }
        set
        {
            comboCount = value;
            comboText.text = comboCount.ToString();

            if ( comboCount != 0 )
            {
                comboDisplay.SetActive( true );
            }
        }
    }

    private void Start()
    {
        foreach( Line line in GameManager.Instance.noteSpace.lines )
        {
            line.OnHitNote += OnHitNote;
        }

        GameManager.Instance.noteSpace.OnChangeResume += OnChangeResume;
    }

    private void Update()
    {
        if ( remainedDisplayTime > 0.0f )
        {
            remainedDisplayTime -= Time.deltaTime;
            if ( remainedDisplayTime <= 0.0f )
            {
                InactiveDisplays();
                comboDisplay.SetActive( false );
            }
        }
    }

    private void InactiveDisplays()
    {
        perfectDisplay.SetActive( false );
        goodDisplay.SetActive( false );
        badDisplay.SetActive( false );
        missDisplay.SetActive( false );
    }

    private void OnHitNote( NoteInfo noteInfo, EHitRate hitRate )
    {
        if ( hitRate == EHitRate.BACKGOUND )
        {
            return;
        }

        InactiveDisplays();
        remainedDisplayTime = hitDisplayTime;

        switch ( hitRate )
        {
            case EHitRate.PERFECT:
            {
                ++ComboCount;
                perfectDisplay.SetActive( true );
            } break;

            case EHitRate.GOOD:
            {
                ++ComboCount;
                goodDisplay.SetActive( true );
            } break;

            case EHitRate.BAD:
            {
                ++ComboCount;
                badDisplay.SetActive( true );
            } break;

            case EHitRate.MISS:
            {
                ComboCount = 0;
                missDisplay.SetActive( true );
            } break;

            default:
            {
                UnityEngine.Debug.LogError( "[OnHitNote] invalid hitRate. " + hitRate );
            } break;
        }
    }

    private void OnChangeResume( bool isResume )
    {
        resumeDisplay.SetActive( isResume );
    }
}

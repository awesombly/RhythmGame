﻿using System;
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
    }
    
    public GameObject perfectDisplay;
    public GameObject goodDisplay;
    public GameObject badDisplay;
    public GameObject missDisplay;

    public float hitDisplayTime;
    private float remainedDisplayTime;

    private void Start()
    {
        foreach( Line line in GameManager.Instance.noteSpace.lines )
        {
            line.OnHitNote += OnHitNote;
        }
    }

    private void Update()
    {
        if ( remainedDisplayTime > 0.0f )
        {
            remainedDisplayTime -= Time.deltaTime;
            if ( remainedDisplayTime <= 0.0f )
            {
                InactiveDisplays();
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

    private void OnHitNote( EHitRate hitRate )
    {
        InactiveDisplays();
        remainedDisplayTime = hitDisplayTime;

        switch ( hitRate )
        {
            case EHitRate.PERFECT:
            {
                perfectDisplay.SetActive( true );
            } break;

            case EHitRate.GOOD:
            {
                goodDisplay.SetActive( true );
            } break;

            case EHitRate.BAD:
            {
                badDisplay.SetActive( true );
            } break;

            case EHitRate.MISS:
            {
                missDisplay.SetActive( true );
            } break;

            default:
            {
                UnityEngine.Debug.LogError( "[OnHitNote] invalid hitRate. " + hitRate );
            } break;
        }
    }
}

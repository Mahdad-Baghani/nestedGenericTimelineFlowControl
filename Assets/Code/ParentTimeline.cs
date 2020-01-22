﻿using neo.timelineExtensions;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[RequireComponent(typeof(Animator))]
public class ParentTimeline : TimelineConditional
{
    public GameObject m_nestedCassetteTimeline;
    private PlayableDirector m_timelineDirector;
    private GameObject m_generatedNestedTimelineObject;

    private void Start()
    {
        timelineConditionMet = false;
        m_timelineDirector = GetComponent<PlayableDirector>();
        StartTimelineInstance();
    }

    public void StartTimelineInstance()
    {
        // instantiate the nested timeline
        m_generatedNestedTimelineObject = Instantiate(m_nestedCassetteTimeline, transform);
        var nestedTimeline = m_generatedNestedTimelineObject.GetComponent<PlayableDirector>();
        nestedTimeline.stopped += (director) =>
        {
            // when the nested timeline finished playing, the parent is allowed to move forward the set marker
            timelineConditionMet = true;
            Destroy(m_generatedNestedTimelineObject);
        };

        // create parent timeline bindings
        foreach (var track in m_timelineDirector.playableAsset.outputs)
        {
            if (track.sourceObject is ControlTrack)
            {
                ControlTrack ct = (ControlTrack)track.sourceObject;
                if (ct.name == "nestedTimeline")
                {
                    foreach (TimelineClip timelineClip in ct.GetClips())
                    {
                        //timelineClip.duration = nestedTimeline.duration;
                        ControlPlayableAsset playableAsset = (ControlPlayableAsset)timelineClip.asset;
                        playableAsset.postPlayback = ActivationControlPlayable.PostPlaybackState.Revert;
                        playableAsset.updateDirector = false;
                        playableAsset.updateParticle = false;

                        // set the reference of the nested timeline to the parent playable asset
                        m_timelineDirector.SetReferenceValue(playableAsset.sourceGameObject.exposedName, nestedTimeline.gameObject);
                        // rebind the playableGraph of the parent timeline director
                        m_timelineDirector.RebindPlayableGraphOutputs();
                    }
                }
            }
            // the following is not valid cause I no longer change the duration of the clips
            //// the elevate animation should not disappear,
            ////I change the duration of the timelineclip of Control Track for nested timeline and I have to change this one as well
            //if (track.streamName == "treasureBoxAnimTrack")
            //{
            //    AnimationTrack at = (AnimationTrack)track.sourceObject;
            //    foreach (TimelineClip clip in at.GetClips())
            //    {
            //        if (clip.displayName == "treasurebox-elevateTreasures")
            //        {
            //            clip.duration = nestedTimeline.duration;
            //            break;
            //        }
            //    }
            //}
        }

        // now I can play the timeline
        m_timelineDirector.Play();
    }
}

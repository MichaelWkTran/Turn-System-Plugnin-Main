using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[CreateAssetMenu(fileName = "Move", menuName = "Turn System/Create New Move")]
public class Move : ScriptableObject
{
    [SerializeField, TextArea] string m_Description; //The description of the move
    [SerializeField] TimelineAsset m_timeline; //Stores move animation and how it impacts the units (this includes damaging the units)
    [SerializeField] string m_MoveProperties; //Move special affects (Optional)
    [SerializeField] float m_Speed; //Impacts what order the units attack in
    [SerializeField] float m_Accuracy; //The chance on whether the move would hit

    public TimelineAsset Timeline { get { return m_timeline; } }
    public string Description { get { return m_Description; } }
    public string Properties { get { return m_MoveProperties; } }
    public float Speed { get { return m_Speed; } }
    public float Accuracy { get { return m_Accuracy; } }

    public void SetUpPlayableDirector(PlayableDirector _director, UnitStats _executorUnit, UnitStats _targetUnit)
    {
        void AssignAnimationBinding(UnitStats _unitStats, PlayableBinding _playableBinding)
        {
            Animator animator = _unitStats.GetComponent<Animator>();
            _director.SetGenericBinding(_playableBinding.sourceObject, animator);

            var clips = (_playableBinding.sourceObject as AnimationTrack).GetClips();
            foreach (var clip in clips)
            {
                

                //Check whether the selected clip is an AnimationPlayableAsset
                AnimationPlayableAsset animationAsset = clip.asset as AnimationPlayableAsset;
                if (animationAsset == null) continue;

                //
                animationAsset.clip = null;

                //Check whether the animation clip can be replaced
                UnitStats.UnitAnimation unitAnimation = Array.Find(_unitStats.m_unitAnimations, i => i.m_name == clip.displayName);
                if (unitAnimation.m_name == "") continue;

                //Replace the animation
                animationAsset.clip = unitAnimation.m_clip;

                //GetChildTracks
            }
        }

        //Loop through all bindings in the director
        foreach (var playableAssetOutput in _director.playableAsset.outputs) switch (playableAssetOutput.streamName)
        {
            //Set the unit and animator bindings
            case "Executor Unit Stats": _director.SetGenericBinding(playableAssetOutput.sourceObject, _executorUnit); break;
            case "Target Unit Stats": _director.SetGenericBinding(playableAssetOutput.sourceObject, _targetUnit); break;
            case "Executor Animator": AssignAnimationBinding(_executorUnit, playableAssetOutput); break;
            case "Target Animator": AssignAnimationBinding(_targetUnit, playableAssetOutput); break;
        }
    }

    public static void BindToDirector(string _streamName, UnityEngine.Object _objectToBind)
    {
        //Check whether the stream name is valid for the turn system
        switch (_streamName)
        {
            case "Executor Unit Stats":
            case "Target Unit Stats":
            case "Executor Animator":
            case "Target Animator":
            case "Turn System Signal Track":
                Debug.LogError
                (
                    "The stream name, " + "_streamName," + "can not be of the listed names:\n" +
                    "Executor Unit Stats\n" +
                    "Target Unit Stats\n" +
                    "Executor Animator\n" +
                    "Target Animator\n" +
                    "Turn System Signal Track"
                );
                return;
        }

        //Find the playable binding specified by _streamName
        PlayableBinding playableBinding;
        if (!FindObjectOfType<TurnSystem>().GetComponent<PlayableDirector>().TryGetPlayableBindingByName(_streamName, out playableBinding)) return;

        //Bind the object
        playableBinding.sourceObject = _objectToBind;
    }
}

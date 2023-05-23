using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[CreateAssetMenu(fileName = "Untitled Move", menuName = "Turn System/Create New Move")]
public class Move : ScriptableObject
{
    [SerializeField, TextArea] string m_description; //The description of the move
    [SerializeField] TimelineAsset m_timeline; //Stores move animation and how it impacts the units (this includes damaging the units)
    [SerializeField] float m_speed; //Impacts what order the units attack in
    [SerializeField] float m_accuracy; //The chance on whether the move would hit

    public TimelineAsset Timeline { get { return m_timeline; } }
    public string Description { get { return m_description; } }
    public float Speed { get { return m_speed; } }
    public float Accuracy { get { return m_accuracy; } }

    public void SetUpPlayableDirector(PlayableDirector _director, Unit _executorUnit, Unit _targetUnit)
    {
        void AssignAnimationBinding(Unit _Unit, PlayableBinding _playableBinding)
        {
            void ReplaceTrackClips(AnimationTrack _track)
            {
                var clips = _track.GetClips();
                foreach (var clip in clips)
                {
                    //Check whether the selected clip is an AnimationPlayableAsset
                    AnimationPlayableAsset animationAsset = clip.asset as AnimationPlayableAsset;
                    if (animationAsset == null) continue;

                    //Clear the track clip
                    animationAsset.clip = null;

                    //Check whether the animation clip can be replaced
                    Unit.UnitAnimation unitAnimation = Array.Find(_Unit.m_unitAnimations, i => i.m_name == clip.displayName);
                    if (unitAnimation.m_name == "") continue;

                    //Replace the animation
                    animationAsset.clip = unitAnimation.m_clip;
                }
            }

            //Set the unit animator to director bindings
            Animator animator = _Unit.GetComponent<Animator>();
            _director.SetGenericBinding(_playableBinding.sourceObject, animator);

            //Get the animation track and replace its animation clips
            var animationTrack = _playableBinding.sourceObject as AnimationTrack;
            ReplaceTrackClips(animationTrack);

            //Get the animation override tracks and replace its animation clips
            var childTracks = animationTrack.GetChildTracks();
            foreach (var overrideTrack in childTracks) ReplaceTrackClips(overrideTrack as AnimationTrack);
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

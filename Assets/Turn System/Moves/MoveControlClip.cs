using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class MoveControlClip : PlayableAsset,  ITimelineClipAsset
{
    public ClipCaps clipCaps { get { return ClipCaps.None; } }
    [SerializeField] MovePlayableBehaviour m_template = new MovePlayableBehaviour();
    
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        return ScriptPlayable<MovePlayableBehaviour>.Create(graph, m_template);
    }
}

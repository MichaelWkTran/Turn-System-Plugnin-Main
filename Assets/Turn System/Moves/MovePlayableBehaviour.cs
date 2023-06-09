using System;
using UnityEngine;
using UnityEngine.Playables;
using static Move;

// A behaviour that is attached to a playable
[Serializable]
public class MovePlayableBehaviour : PlayableBehaviour
{
    [SerializeField] float m_health;
    [SerializeField] float m_attack;
    [SerializeField] float m_defence;
    [SerializeField] float m_speed;
    [SerializeField] float m_evasion;
    [SerializeField] float m_accuracy;
    [SerializeField] string[] m_moveProperties;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (!Application.isPlaying) return;
        var unit = info.output.GetUserData() as Unit;
        
        //Apply Stat Modifier
        unit.Health += m_health;
        unit.Attack += m_attack;
        unit.Defence += m_defence;
        unit.Speed += m_speed;
        unit.Evasion += m_evasion;
        unit.Accuracy += m_accuracy;
        foreach (string moveProperty in m_moveProperties)
            typeof(MoveProperties).GetMethod(moveProperty).
                Invoke(null, new object[] { info.output.GetUserData() });        
        unit.OnUpdate.Invoke();
    }
}

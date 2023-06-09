using UnityEngine;

public class MovePlayablePrebab : MonoBehaviour
{
    public struct ObjectBinding
    {
        public string m_streamName;
        public Object m_objectToBind;

    }
    public ObjectBinding[] m_objectsToBind;

    void Awake()
    {
        //Bind objects
        foreach (var objectBinding in m_objectsToBind)
            Move.BindToDirector(objectBinding.m_streamName, objectBinding.m_objectToBind); 
    }
}

using System.Collections.Generic;
using System.Linq;

public class StateMachine
{
    public delegate void StateEventDelegate();
    public class State
    {
        public State(StateEventDelegate _start = null, StateEventDelegate _update = null, StateEventDelegate _exit = null)
        {
            void SetStateEvent(ref StateEventDelegate _stateEvent, StateEventDelegate _stateEventParam)
            {
                if (_stateEventParam == null) _stateEvent += () => { };
                else _stateEvent = _stateEventParam;
            }

            SetStateEvent(ref m_start, _start);
            SetStateEvent(ref m_update, _update);
            SetStateEvent(ref m_exit, _exit);
        }
        public StateEventDelegate m_start;
        public StateEventDelegate m_update;
        public StateEventDelegate m_exit;
    }
    
    public Dictionary<string, State> m_states;
    protected State m_currentState = null;
    public State CurrentState
    {
        get { return m_currentState; }
        set
        {
            //Check whether the current state is set
            if (m_currentState == null) return;

            //Check whether the value to assign exists in the machine
            if (!m_states.ContainsValue(value)) return;

            //Execute state events
            m_currentState.m_exit();
            value.m_start();

            //Update current state
            m_currentState = value;
        }
    }
    public string CurrentStateName
    {
        get
        {
            //Check whether the current state is set
            if (m_currentState == null) return "";
            
            //Returns the key of the state
            return m_states.FirstOrDefault(i => i.Value == CurrentState).Key;
        }
    }

    public void Start(State _startState)
    {
        if (_startState == null) return;
        m_currentState = _startState;
        m_currentState.m_start();
    }

    public void Update()
    {
        if (m_currentState == null) return;

        //Update current state
        m_currentState.m_update();
    }

    public void End()
    {
        if (m_currentState != null) return;
        m_currentState.m_exit();
        m_currentState = null;
    }
}
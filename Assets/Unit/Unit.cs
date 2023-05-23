using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Unit : MonoBehaviour
{
    public enum UnitType
    {
        Player, //Player unit types will appear on the player side of the battle
        Enemy, //Enemy unit types will appear on the enemy side of the battle and are required to be defeated to end the battle
        Neutral, //Neutral unit types will appear on the enemy side of the battle but are not required to be defeated to end the battle
        NeutralPlayer, //Same as neutral but spawns on the player side
    }

    public string m_unitName;
    public UnitType m_unitType;
    public float m_experience;
    public UnityEvent OnUpdate;

    //Health
    [SerializeField] float m_maxHealth;   [ReadOnly, SerializeField] float m_health;
    [SerializeField] float m_minAttack; [SerializeField] float m_maxAttack;   [ReadOnly, SerializeField] float m_attack;
    [SerializeField] float m_minDefence; [SerializeField] float m_maxDefence;  [ReadOnly, SerializeField] float m_defence;
    [SerializeField] float m_minSpeed; [SerializeField] float m_maxSpeed;    [ReadOnly, SerializeField] float m_speed;
    [SerializeField] float m_minEvasion; [SerializeField] float m_maxEvasion;  [ReadOnly, SerializeField] float m_evasion;
    [SerializeField] float m_minAccuracy; [SerializeField] float m_maxAccuracy; [ReadOnly, SerializeField] float m_accuracy;

    //Moves List
    public List<Move> m_unitMoves;

    //Battle varribles
    public Move m_moveSelected;
    public Unit m_targetUnit;

    //Animations
    [System.Serializable] public struct UnitAnimation
    {
        public string m_name;
        public AnimationClip m_clip;
    }
    public UnitAnimation[] m_unitAnimations;

    //Methods
    void Awake()
    {
        m_health   = m_maxHealth;
        m_attack   = m_maxAttack;  
        m_defence  = m_maxDefence; 
        m_speed    = m_maxSpeed;   
        m_evasion  = m_maxEvasion; 
        m_accuracy = m_maxAccuracy;
    }

    void Start()
    {
        TurnSystem turnSystem = FindObjectOfType<TurnSystem>();
        turnSystem.AddUnit(this);
    }

    void OnDestroy()
    {
        TurnSystem turnSystem = FindObjectOfType<TurnSystem>();
        if (turnSystem != null)
        {
            //Remove the unit from turnsystem list if destroyed
            switch(m_unitType)
            {
                case UnitType.Player: turnSystem.m_players.Remove(this); break;
                case UnitType.Enemy: turnSystem.m_enemies.Remove(this); break;
                default: turnSystem.m_neutral.Remove(this); break;
            }
        }
    }

    void SetMaxFloatStat(float _value, ref float _currentVariable, ref float _maxVariable)
    {
        _maxVariable = _value;
        _currentVariable = Mathf.Clamp(_currentVariable, 0.0f, _maxVariable);
    }

    void SetFloatStat(float _value, ref float _currentVariable, float _maxVariable, float _minVariable = 0.0f)
    {
        _currentVariable = Mathf.Clamp(_value, _minVariable, _maxVariable);
    }

    //Get Set Methods
    public float MaxHealth
    {
        get { return m_maxHealth; }
        set { SetMaxFloatStat(value, ref m_health, ref m_maxHealth); }
    }

    public float Health
    {
        get { return m_health; }
        set { SetFloatStat(value, ref m_health, m_maxHealth); }
    }

    public float MaxAttack
    {
        get { return m_maxAttack; }
        set { SetMaxFloatStat(value, ref m_health, ref m_maxHealth); }
    }

    public float Attack
    {
        get { return m_attack; }
        set { SetFloatStat(value, ref m_attack, m_maxAttack); }
    }

    public float MaxDefence
    {
        get { return m_maxDefence; }
        set { SetMaxFloatStat(value, ref m_defence, ref m_maxDefence); }
    }

    public float Defence
    {
        get { return m_defence; }
        set { SetFloatStat(value, ref m_defence, m_maxDefence); }
    }

    public float MaxSpeed
    {
        get { return m_maxSpeed; }
        set { SetMaxFloatStat(value, ref m_speed, ref m_maxSpeed); }
    }

    public float Speed
    {
        get { return m_speed; }
        set { SetFloatStat(value, ref m_speed, m_maxSpeed); }
    }

    public float MaxEvasion
    {
        get { return m_maxEvasion; }
        set { SetMaxFloatStat(value, ref m_evasion, ref m_maxEvasion); }
    }

    public float Evasion
    {
        get { return m_evasion; }
        set { SetFloatStat(value, ref m_evasion, m_maxEvasion); }
    }

    public float MaxAccuracy
    {
        get { return m_maxAccuracy; }
        set { SetMaxFloatStat(value, ref m_accuracy, ref m_maxAccuracy); }
    }

    public float Accuracy
    {
        get { return m_accuracy; }
        set { SetFloatStat(value, ref m_accuracy, m_maxAccuracy); }
    }

    public void SetMoveSelected(int _moveIndex)
    {
        m_moveSelected = m_unitMoves[_moveIndex];
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

[RequireComponent(typeof(PlayableDirector), typeof(SignalReceiver))]
public class TurnSystem : MonoBehaviour
{
    #region Classes, Enums and Structs
    public enum BattleState { SelectMove, SelectTarget, ExecuteMoves, }

    //The position of a player unit
    [Serializable] public class PlayerPositionsStruct
    {
        public Vector3[] m_positions;
    }

    //Turn system UI
    [Serializable] protected struct CommandUI
    {
        public RectTransform m_pannel;
        public Button m_backButton;
    }
    [Serializable] protected struct FightUI
    {
        public RectTransform m_pannel;
        public Button m_moveButtonPrefab;
        public VerticalLayoutGroup m_moveLayoutGroup;
    }
    [Serializable] protected struct ItemUI
    {
        public RectTransform m_pannel;
    }
    [Serializable] protected struct SelectTargetUI
    {
        public RectTransform m_pannel;
    }

    #endregion
    
    public static TurnSystem m_singleton { get; private set; }
    StateMachine m_machine = new StateMachine();
    public BattleState CurrentBattleState
    {
        get
        {
            return (BattleState)Enum.Parse(typeof(BattleState), m_machine.CurrentStateName);
        }
    } //What is state is the turn system currently in
    public uint m_turnNumber { get; private set; } = 0U; //Number of turns performed since the battle as begun

    //Player and enemy information
    [Header("Player and Enemy information")]
    public List<Unit> m_players, m_enemies, m_neutral; //The stored player & enemy units
    [SerializeField] PlayerPositionsStruct[] m_playerPositions; //The position of the player units when a battle starts
    public PlayerPositionsStruct[] PlayerPositions
    {
        get { return m_playerPositions; }
        set
        {
            m_playerPositions = value;
            CheckPlayerPositionsValidity();
        }
    }

    //Unit and Move Selection information
    int m_playerSelectIndex = 0; //The index of the player unit who the player is currently selecting a move for

    //Other Variables
    PlayableDirector m_director;
    
    //BattleUI
    [Header("UI")]
    [SerializeField] RectTransform m_battleUI;
    [SerializeField] CommandUI m_commandUI;
    [SerializeField] FightUI m_fightUI;
    [SerializeField] ItemUI m_itemUI;
    [SerializeField] SelectTargetUI m_selectTargetUI;

#if UNITY_EDITOR
    [Header("Debug")]
    public int m_playerPositionsDebugIndex;
    public Unit m_selectedTarget;
#endif

    void Awake()
    {
        //Ensure that this component is a singleton
        if (m_singleton == null) m_singleton = this;
        else
        {
            Debug.LogWarning("There must only be one TurnSystem object in a scene. To maintain this, the TurnSystem component in " + gameObject.name + " is destroyed");
            Destroy(this);
        }

        //Assign Components
        m_director = GetComponent<PlayableDirector>();
    }

    void Start()
    {
        //Find Units
        List<Unit> neutralPlayers = new List<Unit>();
        foreach (Unit unit in FindObjectsOfType<Unit>(true))
        {
            AddUnit(unit);
            if (unit.m_unitType == Unit.UnitType.NeutralPlayer) neutralPlayers.Add(unit);
        }
        
        //Ensure GoToPreviousTurnButton is disabled
        m_commandUI.m_backButton.gameObject.SetActive(false);

        //Prepare Players
        for (int i = 0; i < m_players.Count; i++)
        {
            Unit unit = m_players[i];

            //Sets the player positions
            unit.transform.position = m_playerPositions[m_players.Count-1].m_positions[i];
        }

        //Prepare Neuteral Players
        for (int i = 0; i < neutralPlayers.Count; i++)
        {
            Unit unit = neutralPlayers[i];

            //Sets the neuteral positions
            unit.transform.position = m_playerPositions[m_players.Count + neutralPlayers.Count - 2].m_positions[i];
        }

        //Set up state machine
        m_machine.m_states = new Dictionary<string, StateMachine.State>
        {
            { BattleState.SelectMove.ToString(),   new StateMachine.State(SelectMoveStart, null, SelectMoveExit) },
            { BattleState.SelectTarget.ToString(), new StateMachine.State(SelectTargetStart, null, SelectTargetExit) },
            { BattleState.ExecuteMoves.ToString(), new StateMachine.State(ExecuteMovesStart) }
        };

        //Start state machine
        m_machine.Start(m_machine.m_states[BattleState.SelectMove.ToString()]);
    }

    void Update()
    {
        //Update State Machine
        m_machine.Update();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        //Draw Unit Gizmos for player positions
        {
            PlayerPositionsStruct playerPositionsClass = m_playerPositions[m_playerPositionsDebugIndex];
            for (int posIndex = 0; posIndex < playerPositionsClass.m_positions.Length; posIndex++)
            {
                GUIStyle guiStye = new GUIStyle();
                guiStye.fontSize = 30;
                guiStye.normal.textColor = Color.white;

                Gizmos.DrawIcon(playerPositionsClass.m_positions[posIndex], "Turn System/Unit Gizmo.png", false);
                Handles.Label(playerPositionsClass.m_positions[posIndex], posIndex.ToString(), guiStye);
            }
        }
    }
#endif

    #region State Methods
    #region Select Move
    void SelectMoveStart()
    {
        //Reset turn system
        if (m_playerSelectIndex >= m_players.Count) m_playerSelectIndex = 0;

        //Enables/Disables UI
        m_battleUI.gameObject.SetActive(true);
        m_commandUI.m_pannel.gameObject.SetActive(true);
        m_commandUI.m_backButton.gameObject.SetActive(m_playerSelectIndex > 0);

        //Skip Player Units that have no health
        bool isFirstUnit = m_playerSelectIndex <= 0;
        for (; m_playerSelectIndex < m_players.Count; m_playerSelectIndex++)
        {
            if (m_players[m_playerSelectIndex].Health >= 0.0f) break;

            //Lose the game if all players have no health
            if (!isFirstUnit) continue;
            if (m_playerSelectIndex + 1 >= m_players.Count) { Lost(); return; }
        }

        //Update fight UI
        foreach (Transform child in m_fightUI.m_moveLayoutGroup.transform) Destroy(child.gameObject);
        for (int i = 0; i < m_players[m_playerSelectIndex].m_unitMoves.Count; i++)
        {
            Button moveButton = Instantiate(m_fightUI.m_moveButtonPrefab, m_fightUI.m_moveLayoutGroup.transform);
            Move move = m_players[m_playerSelectIndex].m_unitMoves[i];

            int moveIndex = i;
            moveButton.onClick.AddListener(delegate { SelectMove(m_players[m_playerSelectIndex].m_unitMoves[moveIndex]); });
            moveButton.GetComponentInChildren<Text>().text = move.name;
        }
    }

    void SelectMoveExit()
    {
        //Close Select Moves UI
        m_commandUI.m_pannel.gameObject.SetActive(false);
        m_fightUI.m_pannel.gameObject.SetActive(false);
        m_itemUI.m_pannel.gameObject.SetActive(false);
    }

    public void SelectMove(Move _move)
    {
        //Dont select move if not in the appropriate state
        if (m_machine.CurrentStateName != BattleState.SelectMove.ToString()) return;

        //Set selected move
        m_players[m_playerSelectIndex].m_moveSelected = _move;

        //Move to the select target state
        m_machine.CurrentState = m_machine.m_states[BattleState.SelectTarget.ToString()];
    }

    public void CommandBackButtonClick()
    {
        //Go to Select Move from a previous player unit while ignoring players that have no health
        for (m_playerSelectIndex--; m_playerSelectIndex > 0; m_playerSelectIndex--)
            if (m_players[m_playerSelectIndex].Health > 0.0f)
                { m_machine.CurrentState = m_machine.m_states[BattleState.SelectMove.ToString()]; return; }
    }
    #endregion

    #region Select Target
    void SelectTargetStart()
    {
        //Enables/Disables UI
        m_commandUI.m_pannel.gameObject.SetActive(false);
        m_fightUI.m_pannel.gameObject.SetActive(false);
        m_selectTargetUI.m_pannel.gameObject.SetActive(true);
    }

    void SelectTargetExit()
    {
        m_selectTargetUI.m_pannel.gameObject.SetActive(false);
    }

    public void SelectTarget(Unit _selectedTarget)
    {
        //Dont select target if not in the appropriate state
        if (m_machine.CurrentStateName != BattleState.SelectTarget.ToString()) return;
        
        //Set the target unit for the player
        m_players[m_playerSelectIndex].m_targetUnit = _selectedTarget;
        m_playerSelectIndex++;

        //If all players have selected a moves and a target, then move to the execute moves state
        if (m_playerSelectIndex >= m_players.Count) m_machine.CurrentState = m_machine.m_states[BattleState.ExecuteMoves.ToString()];
        //Move to the select move state for the next player unit
        else m_machine.CurrentState = m_machine.m_states[BattleState.SelectMove.ToString()];
    }

    public void SelectTargetBackButtonClick()
    {
        //Dont execute if not in the appropriate state
        if (m_machine.CurrentStateName != BattleState.SelectTarget.ToString()) return;

        //Reselect move of the player unit
        m_machine.CurrentState = m_machine.m_states[BattleState.SelectMove.ToString()];
    }
    #endregion

    #region Execute Moves
    void ExecuteMovesStart()
    {
        //Set AI for units
        UnitAI[] unitAIs = FindObjectsOfType<UnitAI>(false);
        foreach (UnitAI unitAI in unitAIs) unitAI.SelectMovesTargetAI();
        
        //Order Units in order of speed
        Unit[] unitTurnOrder = new Unit[m_players.Count + m_enemies.Count + m_neutral.Count];
        m_players.CopyTo(unitTurnOrder, 0);
        m_enemies.CopyTo(unitTurnOrder, m_players.Count);
        m_neutral.CopyTo(unitTurnOrder, m_players.Count + m_enemies.Count);

        Array.Sort
        (
            unitTurnOrder,
            delegate (Unit _left, Unit _right)
            {
                float GetUnitSpeed(ref Unit _unit) { return _unit.Speed + _unit.m_moveSelected.Speed; }
                return (int)((GetUnitSpeed(ref _right) - GetUnitSpeed(ref _left)) * 100);
            }
        );

        //Disables BattleUI
        m_battleUI.gameObject.SetActive(false);

        //Trigger moves for all units
        IEnumerator ExecuteMovesCoroutine()
        {
            //Loop though all units
            for (int unitTurnIndex = 0; unitTurnIndex < unitTurnOrder.Length; unitTurnIndex++)
            {
                Unit executorUnit = unitTurnOrder[unitTurnIndex];
                Unit targetUnit = executorUnit.m_targetUnit;

                //Skip units that have no health
                if (executorUnit.Health <= 0.0f) continue;
                //Skip units that have no target to attack
                if (targetUnit == null) continue;
                //Skip units that have no move assigned
                if (executorUnit.m_moveSelected == null) continue;
                //Skip moves that have no timeline assigned
                if (executorUnit.m_moveSelected.Timeline == null) continue;

                //Play move animations of units
                m_director.playableAsset = executorUnit.m_moveSelected.Timeline;
                executorUnit.m_moveSelected.SetUpPlayableDirector(m_director, executorUnit, targetUnit);
                m_director.Play();

                //Wait for the animation to finish
                bool isMoveFinished = false;
                void DirectorStopEvent(PlayableDirector _director) { isMoveFinished = true; }
                
                m_director.stopped += DirectorStopEvent;
                yield return new WaitUntil(() => isMoveFinished == true);
                m_director.stopped -= DirectorStopEvent;
                
                //Ensure the animation has finished
                m_director.Stop();
            }

            //Transition to select target state
            m_machine.CurrentState = m_machine.m_states[BattleState.SelectMove.ToString()];
        }

        StartCoroutine(ExecuteMovesCoroutine());
    }
    #endregion

    public void Won()
    {
        m_machine.End();
        Debug.Log("Won");
    }

    public void Lost()
    {
        m_machine.End();
        Debug.Log("Lost");
    }
    #endregion

    public void CheckPlayerPositionsValidity()
    {
        int playerPositionIndex = 0;
        foreach (PlayerPositionsStruct playerPosition in m_playerPositions)
        {
            if (playerPosition.m_positions.Length != playerPositionIndex + 1)
                Array.Resize(ref playerPosition.m_positions, playerPositionIndex + 1);

            playerPositionIndex++;
        }
    }

    public void AddUnit(Unit _unit)
    {
        static void AddToList(ref List<Unit> _list, Unit _unit)
        {
            if (_list.Contains(_unit)) return;
            _list.Add(_unit);
        }

        switch (_unit.m_unitType)
        {
            case Unit.UnitType.Player: AddToList(ref m_players, _unit); break;
            case Unit.UnitType.Enemy:  AddToList(ref m_enemies, _unit); break;
            default:                   AddToList(ref m_neutral, _unit); break;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TurnSystem))]
public class TurnSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TurnSystem turnSystem = (TurnSystem)target;
        
        //Check player position validity
        turnSystem.CheckPlayerPositionsValidity();

        //Set player positions debug index
        turnSystem.m_playerPositionsDebugIndex = Mathf.Clamp(turnSystem.m_playerPositionsDebugIndex, 0, turnSystem.PlayerPositions.Length-1);

        //Select a target unit
        if (GUILayout.Button("Set Target Unit"))
        {
            turnSystem.SelectTarget(turnSystem.m_selectedTarget);
        }
    }

    protected virtual void OnSceneGUI()
    {
        TurnSystem turnSystem = (TurnSystem)target;

        //Only show below when turn system is selected in editor
        if (Selection.Contains(turnSystem)) return;

        //Set player position via handles
        {
            TurnSystem.PlayerPositionsStruct[] playerPositions = turnSystem.PlayerPositions;

            EditorGUI.BeginChangeCheck();
            TurnSystem.PlayerPositionsStruct playerPositionsClass = playerPositions[turnSystem.m_playerPositionsDebugIndex];
            for (int posIndex = 0; posIndex < playerPositionsClass.m_positions.Length; posIndex++)
            {
                playerPositionsClass.m_positions[posIndex] = Handles.PositionHandle
                    (playerPositionsClass.m_positions[posIndex], Quaternion.identity);
            }

            if (EditorGUI.EndChangeCheck()) turnSystem.PlayerPositions = playerPositions;
        }   
    }
}
#endif
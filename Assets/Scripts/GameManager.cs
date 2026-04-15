using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    // EVENTS & EVENTARGS for Events
    public event EventHandler<OnClickedOnGridPositionEventArgs> OnCLickOnGridPosition;

    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public event EventHandler OnGameStarted;
    public event EventHandler OnCurrentPlayablePlayerTypeChanged;
    public event EventHandler<OnGameWInEventArgs> OnGameWin;

    public class OnGameWInEventArgs : EventArgs
    {
        /*public Vector2Int centerGridPosition;*/
        public Line line;
        public PlayerType winPlayerType;
    }

    public event EventHandler OnRematch;
    public event EventHandler OnGameTied;
    public event EventHandler OnScoreChanged;
    public event EventHandler OnPlacedObject;


    // ENUMS
    public enum PlayerType
    {
        None,
        Cross,
        Circle,
    }

    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB,
    }

    public struct Line
    {
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPosition;
        public Orientation orientation;
    }

    // VARIABLES
    private PlayerType localPlayerType;
    private NetworkVariable<PlayerType> currentPlayablePlayerType = new NetworkVariable<PlayerType>();
    // NOTE: just some of the options you could use for the = new NetworkVariable<PlayerType>(PlayerType.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // which is what we are doing as PlayerType.none is the default, as well as everyone can read and only server can write. so () is the same
    private PlayerType[,] playerTypeArray;
    private List<Line> lineList;

    private NetworkVariable<int> playerCrossScore = new NetworkVariable<int>();
    private NetworkVariable<int> playerCircleScore = new NetworkVariable<int>();


    // METHODS
    private void Awake()
    {
        if (Instance)
        {
            Debug.LogError("More than one GameManager instance!");
            //Destroy(this);
        }

        Instance = this;

        playerTypeArray = new PlayerType[3, 3];

        lineList = new List<Line>
        {
            // HORIZONTAL LINES
            new Line // horizontal bottom row
            {
                gridVector2IntList = new List<Vector2Int>
                    { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), },
                centerGridPosition = new Vector2Int(1, 0),
                orientation = Orientation.Horizontal
            },
            new Line // horizontal middle row
            {
                gridVector2IntList = new List<Vector2Int>
                    { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Horizontal
            },
            new Line // horizontal top row
            {
                gridVector2IntList = new List<Vector2Int>
                    { new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2), },
                centerGridPosition = new Vector2Int(1, 2),
                orientation = Orientation.Horizontal
            },

            // VERTICAL LINES
            new Line // vertical left column
            {
                gridVector2IntList = new List<Vector2Int>
                    { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), },
                centerGridPosition = new Vector2Int(0, 1),
                orientation = Orientation.Vertical
            },
            new Line // vertical middle column
            {
                gridVector2IntList = new List<Vector2Int>
                    { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2), },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Vertical
            },
            new Line // vertical right column
            {
                gridVector2IntList = new List<Vector2Int>
                    { new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(2, 2), },
                centerGridPosition = new Vector2Int(2, 1),
                orientation = Orientation.Vertical
            },

            // CORNER LINES
            new Line // top left to bottom right corner line
            {
                gridVector2IntList = new List<Vector2Int>
                    { new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2), },
                    
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalA
            },
            new Line // bottom left to top right corner line
            {
                gridVector2IntList = new List<Vector2Int>
                    { new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(2, 0), },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalB
            },

        };
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("Client ID: " + NetworkManager.Singleton.LocalClientId);
        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            localPlayerType = PlayerType.Cross;

        }
        else
        {
            localPlayerType = PlayerType.Circle;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        currentPlayablePlayerType.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) =>
        {
            OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
        };

        playerCrossScore.OnValueChanged += (int prevScore, int newScore) =>
        {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
        };

        playerCircleScore.OnValueChanged += (int prevScore, int newScore) =>
        {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
        };

    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            // Start Game
            currentPlayablePlayerType.Value = PlayerType.Cross;

            TriggerOnGameStartedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        if (playerType != currentPlayablePlayerType.Value)
        {
            return;
        }

        if (playerTypeArray[x, y] != PlayerType.None)
        {
            return;
        }

        playerTypeArray[x, y] = playerType;
        TriggerOnPlacedObjectRpc();

        Debug.Log($"ClickedOnGridPositionRpc {x}, {y}");

        // event invoke
        OnCLickOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs{ x = x, y = y, playerType = playerType });

        switch (currentPlayablePlayerType.Value)
        {
            default:
            case PlayerType.Cross:
                currentPlayablePlayerType.Value = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayablePlayerType.Value = PlayerType.Cross;
                break;
        }

        TestWinner();


        // NOTE: this was removed because we changed to a network Variable which we are using its own event
        // left for educational use
        //TriggerOnPlayablePlayerTypeChangedRpc();
    }

    /*
    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnPlayablePlayerTypeChangedRpc()
    {
        OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
    }
    */

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnPlacedObjectRpc()
    {
        OnPlacedObject?.Invoke(this, EventArgs.Empty);
    }

    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine(
            playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
            playerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
            playerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]
            
        );
    }

    private bool TestWinnerLine(PlayerType aPlayerType, PlayerType bPlayerType, PlayerType cPlayerType)
    {
        return
            aPlayerType != PlayerType.None &&
            aPlayerType == bPlayerType &&
            bPlayerType == cPlayerType;
    }


    private void TestWinner()
    {
        /*foreach (Line line in lineList)*/
        for (int i = 0; i < lineList.Count; i++)
        {
            Line line = lineList[i];

            if(TestWinnerLine(line))
            {
                Debug.Log("Winner! horizontal bottom line");
                currentPlayablePlayerType.Value = PlayerType.None;
                PlayerType winPlayerType = playerTypeArray[line.centerGridPosition.x, line.centerGridPosition.y];

                switch (winPlayerType)
                {
                    case PlayerType.Cross:
                        playerCrossScore.Value++;
                        break;
                    case PlayerType.Circle:
                        playerCircleScore.Value++;
                        break;
                }

                TriggerOnGameWinRpc(i, winPlayerType);

                return;
            }
        }

        bool hasTie = true;
        for (int x = 0; x < playerTypeArray.GetLength(0); x++)
        {
            for (int y = 0; y < playerTypeArray.GetLength(1); y++)
            {
                if (playerTypeArray[x, y] == PlayerType.None)
                {
                    hasTie = false;

                    break;
                }
            }
        }

        if (hasTie)
        {
            TriggerOnGameTiedRpc();
        }

    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameTiedRpc()
    {
        OnGameTied?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinRpc(int lineIndex, PlayerType winPlayerType)
    {
        Line line = lineList[lineIndex];

        OnGameWin?.Invoke(this, new OnGameWInEventArgs
        {
            line = line,
            winPlayerType = winPlayerType,

            // Left this for extra data to learn from if i need to reference it
            //centerGridPosition = new Vector2Int(line.gridVector2IntList[1].x, line.gridVector2IntList[1].y)
        });
    }

    
    [Rpc(SendTo.Server)]
    public void RematchRpc()
    {
        for (int x = 0; x < playerTypeArray.GetLength(0); x++)
        {
            for (int y = 0; y < playerTypeArray.GetLength(1); y++)
            {
                playerTypeArray[x, y] = PlayerType.None;
            }
        }

        currentPlayablePlayerType.Value = PlayerType.Cross;

        TriggerOnRematchRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnRematchRpc()
    {
        OnRematch?.Invoke(this, EventArgs.Empty);
    }


    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return currentPlayablePlayerType.Value;
    }

    public void GetScores(out int playerCrossScore, out int playerCircleScore)
    {
        playerCrossScore = this.playerCrossScore.Value;
        playerCircleScore = this.playerCircleScore.Value;

    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PlayerIOClient;
using TMPro;

public class ChatEntry
{
    public string text = "";
    public bool mine = true;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Prefabs")]
    public GameObject Player;
    public GameObject PlayerPrefab;
    public GameObject SpectatorPrefab;

    [Header("Properties")]
    public UiManager Ui;
    [SerializeField] BallController _ballControl;
    [SerializeField] Transform[] _SpawnLevels;
    [SerializeField] int _actualLevel = 0;

    // UI stuff & other
    Connection _pioconnection;
    Dictionary<string, Transform> _players = new();
    List<Transform> _playersList = new();
    List<Message> _msgList = new(); //  Messsage queue implementation
    ArrayList _entries = new();
    Vector2 _scrollPosition;
    Rect _window = new Rect(10, 10, 300, 150);
    string _inputField = "";
    string _infomsg = "";
    int _turn = 0;
    bool _joinedroom = false;

    void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            Debug.LogError("2 Instances");

        Application.runInBackground = true;

        // Create a random userid 
        System.Random random = new System.Random();
        string userid = "Guest" + random.Next(0, 10000);
        Player.name = userid;

        Debug.Log("Starting");

        PlayerIO.Authenticate(
            "golfgamejam-ymeiulkxaeydrmww43x9aq",            //Your game id
            "public",                               //Your connection id
            new Dictionary<string, string> {        //Authentication arguments
				{ "userId", userid },
            },
            null,                                   //PlayerInsight segments
            delegate (Client client)
            {
                Debug.Log("Successfully connected to Player.IO");
                _infomsg = "Successfully connected to Player.IO";

                Debug.Log("Create ServerEndpoint");
                // Comment out the line below to use the live servers instead of your development server
                //client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);

                Debug.Log("CreateJoinRoom");
                //Create or join the room 
                client.Multiplayer.CreateJoinRoom(
                    "UnityDemoRoom",                    //Room id. If set to null a random roomid is used
                    "UnityRoom",                        //The room type started on the server
                    true,                               //Should the room be visible in the lobby?
                    null,
                    null,
                    delegate (Connection connection)
                    {
                        Debug.Log("Joined Room.");
                        _infomsg = "Joined Room.";
                        // We successfully joined a room so set up the message handler
                        _pioconnection = connection;
                        _pioconnection.OnMessage += Handlemessage;
                        _joinedroom = true;
                    },
                    delegate (PlayerIOError error)
                    {
                        Debug.Log("Error Joining Room: " + error.ToString());
                        _infomsg = error.ToString();
                    }
                );
            },
            delegate (PlayerIOError error)
            {
                Debug.Log("Error connecting: " + error.ToString());
                _infomsg = error.ToString();
            }
        );
    }

    void FixedUpdate()
    {
        // process message queue
        foreach (Message m in _msgList)
        {
            switch (m.Type)
            {
                case "PlayerJoined":
                    GameObject newPlayer = null;

                    if (_players.Count < 4)
                    {
                        if (m.GetBoolean(1))
                        {
                            newPlayer = Player;
                            GameManager.Instance.Ui.ChangeTextUi(_ballControl.name, 0);
                        }
                        else
                            newPlayer = Instantiate(PlayerPrefab);

                        newPlayer.transform.position = _SpawnLevels[_actualLevel].position;

                        newPlayer.name = m.GetString(0);

                        //First player connected
                        if (!_players.ContainsKey(newPlayer.name))
                        {
                            _players.Add(newPlayer.name, newPlayer.transform);
                            _playersList.Add(newPlayer.transform);
                            if (_players.Count <= 1)
                            {
                                _playersList[0].GetComponent<Ball>().IsTurn = true;
                                Ui.ChangeHitUi(true);
                            }
                        }
                    }
                    else if (m.GetBoolean(1))
                    {
                        Application.Quit();
                        UnityEditor.EditorApplication.isPlaying = false;
                    }
                    break;
                case "Move":
                    if (m.GetString(0) != Player.name && _players.ContainsKey(m.GetString(0)))
                    {
                        Transform obj = _players[m.GetString(0)];
                        string[] position = m.GetString(1).Split(" ");
                        string[] rotation = m.GetString(2).Split(" ");

                        obj.position = new Vector3(float.Parse(position[0]), float.Parse(position[1]), float.Parse(position[2]));
                        obj.rotation = Quaternion.Euler(float.Parse(rotation[0]), float.Parse(rotation[1]), float.Parse(rotation[2]));
                    }
                    break;
                case "Chat":
                    ChatText(m.GetString(0) + " says: " + m.GetString(1), false);
                    break;
                case "ChatSystem":
                    ChatText(m.GetString(1), false);
                    break;
                case "PlayerLeft":
                    // remove characters from the scene when they leave

                    if (_players.ContainsKey(m.GetString(0)))
                    {
                        var player = _players[m.GetString(0)];
                        _players.Remove(m.GetString(0));
                        Destroy(player.gameObject);
                    }
                    break;
                case "IsArrived":
                    _players[m.GetString(0)].GetComponent<Ball>().IsArrived = m.GetBoolean(1);
                    break;
                case "NextLevel":
                    _actualLevel++;

                    if (_actualLevel >= _SpawnLevels.Length)
                        _actualLevel = 0;

                    for (int i = 0; i < _playersList.Count; i++)
                    {
                        _playersList[i].gameObject.SetActive(true);
                        _playersList[i].position = _SpawnLevels[_actualLevel].position;
                        _playersList[i].GetComponent<Ball>().IsArrived = false;
                    }
                    break;
                case "ChangeTurn":
                    _turn++;
                    if (_turn >= _playersList.Count)
                        _turn = 0;

                    _playersList[_turn].GetComponent<Ball>().IsTurn = true;
                    break;
            }
        }

        // clear message queue after it's been processed
        _msgList.Clear();
    }

    public void MovePieces(string name, Vector3 position, Vector3 rotation)
    {
        if (_pioconnection == null)
            return;

        string pos = $"{position.x.ToString()} {position.y.ToString()} {position.z.ToString()}";
        string rot = $"{rotation.x} {rotation.y} {rotation.z}";
        _pioconnection.Send("Move", name, pos, rot);
    }

    public void ChangeTurn()
    {
        _pioconnection.Send("ChangeTurn");
    }

    public void IsArrived(string name, bool value)
    {
        _pioconnection.Send("IsArrived", name, value);
    }

    public void CheckEndLevel()
    {
        bool isLevelFinished = true;

        foreach (var player in _players)
        {
            if (!player.Value.GetComponent<Ball>().IsArrived)
                isLevelFinished = false;
        }

        if (isLevelFinished)
        {
            _pioconnection.Send("NextLevel");
        }
    }

    #region Ui
    void Handlemessage(object sender, Message m)
    {
        _msgList.Add(m);
    }

    void OnGUI()
    {
        _window = GUI.Window(1, _window, GlobalChatWindow, "Chat");
        GUI.Label(new Rect(10, 160, 150, 20), "Toadstools picked: none");
        if (_infomsg != "")
        {
            GUI.Label(new Rect(10, 180, Screen.width, 20), _infomsg);
        }
    }

    void GlobalChatWindow(int id)
    {
        if (!_joinedroom)
        {
            return;
        }

        GUI.FocusControl("Chat input field");

        // Begin a scroll view. All rects are calculated automatically - 
        // it will use up any available screen space and make sure contents flow correctly.
        // This is kept small with the last two parameters to force scrollbars to appear.
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        foreach (ChatEntry entry in _entries)
        {
            GUILayout.BeginHorizontal();
            if (!entry.mine)
            {
                GUILayout.Label(entry.text);
            }
            else
            {
                GUILayout.Label(entry.text);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(3);
        }
        // End the scrollview we began above.
        GUILayout.EndScrollView();

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return && _inputField.Length > 0)
        {
            ChatText(Player.transform.name + " says: " + _inputField, true);
            _pioconnection.Send("Chat", _inputField);
            _inputField = "";
        }
        GUI.SetNextControlName("Chat input field");
        _inputField = GUILayout.TextField(_inputField);

        GUI.DragWindow();
    }

    void ChatText(string str, bool own)
    {
        var entry = new ChatEntry();
        entry.text = str;
        entry.mine = own;

        _entries.Add(entry);

        if (_entries.Count > 50)
            _entries.RemoveAt(0);

        _scrollPosition.y = 1000000;
    }
    #endregion Ui

    private void OnDestroy()
    {
        _pioconnection.Disconnect();
    }
}

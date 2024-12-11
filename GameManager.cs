using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PlayerIOClient;
using TMPro;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting;

public class ChatEntry
{
    public string text = "";
    public bool mine = true;
}

public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject Player;
    public GameObject PlayerPrefab;

    [Header("Other")]
    public UiManager Ui;

    // UI stuff & other
    Connection _pioconnection;
    List<Transform> _players = new();
    List<Message> _msgList = new(); //  Messsage queue implementation
    ArrayList _entries = new();
    Vector2 _scrollPosition;
    Rect _window = new Rect(10, 10, 300, 150);
    string _inputField = "";
    string _infomsg = "";
    bool _joinedroom = false;

    void Start()
    {
        Application.runInBackground = true;

        // Create a random userid 
        System.Random random = new System.Random();
        string userid = "Guest" + random.Next(0, 10000);
        Player.name = userid;

        Debug.Log("Starting");

        PlayerIO.Authenticate(
            "localprojectlesson-gkjgymhxxuygsorwtsnxg",            //Your game id
            "public",                               //Your connection id
            new Dictionary<string, string> {        //Authentication arguments
				{ "userId", userid },
            },
            null,                                   //PlayerInsight segments
            delegate (Client client)
            {
                Debug.Log("Successfully connected to Player.IO");
                _infomsg = "Successfully connected to Player.IO";

                //_target.transform.Find("NameTag").GetComponent<TextMesh>().text = userid;
                //_target.transform.name = userid;

                Debug.Log("Create ServerEndpoint");
                // Comment out the line below to use the live servers instead of your development server
                client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);

                Debug.Log("CreateJoinRoom");
                //Create or join the room 
                client.Multiplayer.CreateJoinRoom(
                    "UnityDemoRoom",                    //Room id. If set to null a random roomid is used
                    "UnityRoom",                   //The room type started on the server
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

    void Handlemessage(object sender, Message m)
    {
        _msgList.Add(m);
    }

    void FixedUpdate()
    {
        // process message queue
        foreach (Message m in _msgList)
        {
            switch (m.Type)
            {
                case "PlayerJoined":
                    GameObject newplayer = null;

                    if (m.GetBoolean(3))
                        newplayer = Player;
                    else
                        newplayer = Instantiate(PlayerPrefab);

                    newplayer.transform.position = new Vector3(m.GetFloat(1), 0, m.GetFloat(2));

                    newplayer.name = m.GetString(0);
                    _players.Add(newplayer.transform);
                    break;
                case "Move":
                    Transform piece = _piecesOnBoard[m.GetString(0)];
                    piece.position = new Vector3(m.GetFloat(1), 0, m.GetFloat(2));
                    break;
                case "Chat":
                    if (m.GetString(0) != "Server")
                    {
                        GameObject chatplayer = GameObject.Find(m.GetString(0));

                        chatplayer.transform.GetComponentInChildren<TextMeshProUGUI>().color = Color.White;
                        chatplayer.transform.GetComponentInChildren<TextMeshProUGUI>().text = m.GetString(1);
                        chatplayer.transform.GetComponentInChildren<ClearChat>().LastUpdate = Time.time;
                    }
                    ChatText(m.GetString(0) + " says: " + m.GetString(1), false);
                    break;
                case "ChatSystem":
                    ChatText(m.GetString(1), false);
                    break;
                case "PlayerLeft":
                    // remove characters from the scene when they leave
                    GameObject playerd = GameObject.Find(m.GetString(0));
                    Destroy(playerd);
                    break;
            }
        }

        // clear message queue after it's been processed
        _msgList.Clear();
    }

    public void MovePieces(string name, Vector3 piecePosition)
    {
        _pioconnection.Send("Move", name, piecePosition.x, piecePosition.z);
    }

    #region Ui
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
            GameObject chatplayer = GameObject.Find(Player.transform.name);
            chatplayer.transform.GetComponentInChildren<TextMeshProUGUI>().color = Color.White;
            chatplayer.transform.GetComponentInChildren<TextMeshProUGUI>().text = _inputField;
            chatplayer.transform.GetComponentInChildren<ClearChat>().LastUpdate = Time.time;

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

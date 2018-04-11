/* * * * * * * * * * * * * * * * * * * * *
 * 
 *    Title: "項目"
 * 
 *    Dsecription:
 *                  功能: 
 *                   1. 
 * 
 *     Author: iLYuSha
 *     
 *     Date: 2018.03.24
 *     
 *     Modify:
 *                  03.24 修改: 
 *                   1. 
 *     
 * * * * * * * * * * * * * * * * * * * * */
using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using ExitGames.Client.Photon;

namespace DemoSpace
{
    public class NewLobby : MonoBehaviour 
    {
        [Header("UGUI")]
        public Text textServerPUN;
        public GameObject panelLobby;
        public InputField inputPlayerName;
        public InputField inputRoomName;
        public Text textRealtimePlayerInfo;
        public Text textRealtimeRoomInfo;

        public void CreateRoom()
        {
            PhotonNetwork.CreateRoom(inputRoomName.text, new RoomOptions() { MaxPlayers = 20 }, null);
        }
        public void JoinRoom()
        {
            PhotonNetwork.JoinRoom(inputRoomName.text);
        }

        private string roomName = "myRoom";

        private Vector2 scrollPos = Vector2.zero;

        private bool connectFailed = false;

        public static readonly string SceneNameMenu = "Lobby";

        public static readonly string SceneNameGame = "InGame";

        private string errorDialog;
        private double timeToClearDialog;

        public string ErrorDialog
        {
            get { return this.errorDialog; }
            private set
            {
                this.errorDialog = value;
                if (!string.IsNullOrEmpty(value))
                {
                    this.timeToClearDialog = Time.time + 4.0f;
                }
            }
        }

        public void Awake()
        {
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.automaticallySyncScene = true;

            // the following line checks if this client was just created (and not yet online). if so, we connect
            if (PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated)
            {
                // Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
                PhotonNetwork.ConnectUsingSettings("0.9");
            }

            // generate a name for this player, if none is assigned yet
            if (String.IsNullOrEmpty(PhotonNetwork.playerName))
            {
                PhotonNetwork.playerName = "Guest" + Random.Range(1, 9999);
            }

            // if you wanted more debug out, turn this on:
            // PhotonNetwork.logLevel = NetworkLogLevel.Full;
            panelLobby.SetActive(false);
        }

        private void Update()
        {
            if (!PhotonNetwork.connected)
            {
                if (PhotonNetwork.connecting)
                    textServerPUN.text = "Connecting to: " + PhotonNetwork.ServerAddress;
                else
                    textServerPUN.text = "Not connected. Check console output.Detailed connection state: " + PhotonNetwork.connectionStateDetailed + " Server: " + PhotonNetwork.ServerAddress;


                if (this.connectFailed)
                {
                    textServerPUN.text = "Connection failed. Check setup and use Setup Wizard to fix configuration.\n" +
                        String.Format("Server: {0}", new object[] { PhotonNetwork.ServerAddress }) + "\n" +
                        "AppId: " + PhotonNetwork.PhotonServerSettings.AppID.Substring(0, 8) + "****";

                    //if (GUILayout.Button("Try Again", GUILayout.Width(100)))
                    //{
                    //    this.connectFailed = false;
                    //    PhotonNetwork.ConnectUsingSettings("0.9");
                    //}
                }
                return;
            }

            textRealtimePlayerInfo.text = PhotonNetwork.countOfPlayers + " users are online in " + PhotonNetwork.countOfRooms + " rooms.";
            if (PhotonNetwork.GetRoomList().Length == 0)
                textRealtimeRoomInfo.text = "Currently no games are available./nRooms will be listed here, when they become available.";
            else
            {
                textRealtimeRoomInfo.text = PhotonNetwork.GetRoomList().Length + " rooms available:";

                //this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);

                //foreach (RoomInfo roomInfo in PhotonNetwork.GetRoomList())
                //{
                //    GUILayout.BeginHorizontal();
                //    GUILayout.Label(roomInfo.Name + " " + roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers);
                //    if (GUILayout.Button("Join", GUILayout.Width(150)))
                //    {
                //        PhotonNetwork.JoinRoom(roomInfo.Name);
                //    }
                //}
            }
        }

        // We have two options here: we either joined(by title, list or random) or created a room.
        public void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom");
        }

        public void OnPhotonCreateRoomFailed()
        {
            ErrorDialog = "Error: Can't create room (room name maybe already used).";
            Debug.Log("OnPhotonCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
        }

        public void OnPhotonJoinRoomFailed(object[] cause)
        {
            ErrorDialog = "Error: Can't join room (full or unknown room name). " + cause[1];
            Debug.Log("OnPhotonJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
        }

        public void OnPhotonRandomJoinFailed()
        {
            ErrorDialog = "Error: Can't join random room (none found).";
            Debug.Log("OnPhotonRandomJoinFailed got called. Happens if no room is available (or all full or invisible or closed). JoinrRandom filter-options can limit available rooms.");
        }

        public void OnCreatedRoom()
        {
            Debug.Log("OnCreatedRoom");
            PhotonNetwork.LoadLevel(SceneNameGame);
        }

        public void OnDisconnectedFromPhoton()
        {
            Debug.Log("Disconnected from Photon.");
        }

        public void OnFailedToConnectToPhoton(object parameters)
        {
            this.connectFailed = true;
            Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.ServerAddress);
        }

        public void OnConnectedToMaster()
        {
            Debug.Log("As OnConnectedToMaster() got called, the PhotonServerSetting.AutoJoinLobby must be off. Joining lobby by calling PhotonNetwork.JoinLobby().");
            PhotonNetwork.JoinLobby();
            panelLobby.SetActive(true);
        }
    }
}
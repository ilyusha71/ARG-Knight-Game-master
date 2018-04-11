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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace DemoSpace
{
    public enum Identity
    {
        Master = 0,
        Client =1,
    }
    public class InGame : Photon.MonoBehaviour
    {
        //public Transform playerPrefab;
        public Text textIdentity;
        public Text textRoomName;
        public Text textPlayerName;
        public Text textPlayerCode;

        private bool ready;

        public void Awake()
        {
            // in case we started this demo with the wrong scene being active, simply load the menu scene
            if (!PhotonNetwork.connected)
            {
                SceneManager.LoadScene(Lobby.SceneNameMenu);
                return;
            }

            textIdentity.text = PhotonNetwork.isMasterClient ? Identity.Master.ToString() : Identity.Client.ToString();
            textRoomName.text = PhotonNetwork.room.Name;
            textPlayerName.text = PhotonNetwork.playerName;
            textPlayerCode.text = "Unknown";
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            //PhotonNetwork.Instantiate(this.playerPrefab.name, transform.position, Quaternion.identity, 0);
        }

        public void Ready()
        {
            ready = true;
            photonView.RPC("ClientReady", PhotonTargets.MasterClient, true);
        }

        [PunRPC]
        public void ClientReady(PhotonMessageInfo who)
        {
            if (PhotonNetwork.isMasterClient)
            {

            }
            else
            {

            }
            ready = true;
        }





















        public void OnGUI()
        {
            if (GUILayout.Button("Return to Lobby"))
            {
                PhotonNetwork.LeaveRoom();  // we will load the menu level when we successfully left the room
            }
        }

        public void OnMasterClientSwitched(PhotonPlayer player)
        {
            Debug.Log("OnMasterClientSwitched: " + player);

            string message;
            InRoomChat chatComponent = GetComponent<InRoomChat>();  // if we find a InRoomChat component, we print out a short message

            if (chatComponent != null)
            {
                // to check if this client is the new master...
                if (player.IsLocal)
                {
                    message = "You are Master Client now.";
                }
                else
                {
                    message = player.NickName + " is Master Client now.";
                }


                chatComponent.AddLine(message); // the Chat method is a RPC. as we don't want to send an RPC and neither create a PhotonMessageInfo, lets call AddLine()
            }
        }

        public void OnLeftRoom()
        {
            Debug.Log("OnLeftRoom (local)");

            // back to main menu
            SceneManager.LoadScene(WorkerMenu.SceneNameMenu);
        }

        public void OnDisconnectedFromPhoton()
        {
            Debug.Log("OnDisconnectedFromPhoton");

            // back to main menu
            SceneManager.LoadScene(WorkerMenu.SceneNameMenu);
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            Debug.Log("OnPhotonInstantiate " + info.sender);    // you could use this info to store this or react
        }

        public void OnPhotonPlayerConnected(PhotonPlayer player)
        {
            Debug.Log("OnPhotonPlayerConnected: " + player);
        }

        public void OnPhotonPlayerDisconnected(PhotonPlayer player)
        {
            Debug.Log("OnPlayerDisconneced: " + player);
        }

        public void OnFailedToConnectToPhoton()
        {
            Debug.Log("OnFailedToConnectToPhoton");

            // back to main menu
            SceneManager.LoadScene(WorkerMenu.SceneNameMenu);
        }
    }
}
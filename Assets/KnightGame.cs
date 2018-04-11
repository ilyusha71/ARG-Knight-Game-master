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
    public enum Player
    {
        Player1 = 1,
        Player2 = 2,
        Player3 = 3,
        Player4 = 4,
        Player5 = 5,
        Player6 = 6,
        Player7 = 7,
        Player8 = 8,
        Player9 = 9,
        Player10 = 10,
        Player11 = 11,
        Player12 = 12,
    }
    public enum Character
    {       
        King = 0,
        Knight = 1,
        Duke = 2,
    }
    public enum Faction
    {
        Stella = 0,
        Sola = 1,
        Luna = 2,
    }
    public enum CharacterCht
    {
        國王 = 0,
        騎士 = 1,
        公爵 = 2,
    }
    public class PlayerInfo
    {
        public Character character;
        public Faction faction;
        public int gold;
    }
    public struct Kingdom
    {
        public Player king;
        public List<Player> duke;
        public int castle;
    }
    public enum GameStep
    {
        Regist = 0,
        Deal = 1, // 分配
        Confirm = 2,
    }
    public class KnightGame : MonoBehaviour 
    {
        public int nowIndex;
        public Player nowPlayer;
        GameStep step;
        public Kingdom sola;
        public Kingdom luna;
        public List<PlayerInfo> partyPlayer = new List<PlayerInfo>();


        public Text textPlayerIndex;
        public Text textPlayerFaction;
        public Text textPlayerChracter;


        private void Awake()
        {
            if (ArduinoController.instance == null)
            {

                PlayerPrefs.SetInt("lastScene", SceneManager.GetActiveScene().buildIndex + 100);
                SceneManager.LoadScene("Arduino Controller");
            }
        }

        private void Update()
        {
            if (ArduinoController.msgQueue.Count > 0)
                ArduinoMsg();
            if(Input.GetKeyDown(KeyCode.Return))
                nowIndex = 0;
            
            if (step == GameStep.Regist)
                RegistPlayer();
            else if (step == GameStep.Deal)
                DealCharacter();
            else if (step == GameStep.Confirm)
                ConfirmCharacter();
        }
        void ArduinoMsg()
        {
            for (int i = 0; i < ArduinoController.msgQueue.Count; i++)
            {
                string msg = ArduinoController.msgQueue.Dequeue();
                if (msg.Contains("is Player "))
                {
                    nowIndex = int.Parse(msg.Replace("is Player ", ""));
                    nowPlayer = (Player)nowIndex;
                }
            }
        }

        void RegistPlayer()
        {
            if (nowIndex <= 0)
                return;
            for (int i = 0; i < nowIndex; i++)
            {
                PlayerInfo player = new PlayerInfo();
                partyPlayer.Add(player);
            }
            Debug.Log(partyPlayer.Count);
            step = GameStep.Deal;
        }

        void DealCharacter()
        {
            for (int i = 0; i < partyPlayer.Count; i++)
            {
                PlayerInfo everyPlayer;
                everyPlayer = partyPlayer[i];
                everyPlayer.character = Character.Knight;
                everyPlayer.faction = Faction.Stella;
            }

            int solaKing = Random.Range(0, partyPlayer.Count);
            PlayerInfo solaPlayer = partyPlayer[solaKing];
            solaPlayer.character = Character.King;
            solaPlayer.faction = Faction.Sola;

            int lunaKing;
            do
            {
                lunaKing = Random.Range(0, partyPlayer.Count);
            }
            while (lunaKing == solaKing);
            PlayerInfo lunaPlayer = partyPlayer[lunaKing];
            lunaPlayer.character = Character.King;
            lunaPlayer.faction = Faction.Luna;

            step = GameStep.Confirm;
            nowIndex = 0;

            for (int i = 0; i < partyPlayer.Count; i++)
            {
                Debug.LogWarning(i+1);
                Debug.Log(partyPlayer[i].faction);
                Debug.Log(partyPlayer[i].character);

            }
        }
        void ConfirmCharacter()
        {
            if (nowIndex >0 && nowIndex <= partyPlayer.Count)
            {
                PlayerInfo player = partyPlayer[nowIndex - 1];
                textPlayerIndex.text = nowPlayer.ToString();
                textPlayerFaction.text = player.faction.ToString();
                textPlayerChracter.text = player.character.ToString();
            }
            else
            {
                textPlayerIndex.text = "";
                textPlayerFaction.text = "";
                textPlayerChracter.text = "";
            }
            
        }
    }
}
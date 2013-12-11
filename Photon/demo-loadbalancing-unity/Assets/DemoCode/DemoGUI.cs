using System.Collections.Generic;
using ExitGames.Client.Photon.LoadBalancing;
using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class DemoGUI : MonoBehaviour
{
    public DemoGame GameInstance;

    void Start()
    {
        Application.runInBackground = true;
        CustomTypes.Register();

        this.GameInstance = new DemoGame();
        this.GameInstance.MasterServerAddress = "app-eu.exitgamescloud.com:5055";
        this.GameInstance.AppId = "<insert your app id here>";  // using Photon Cloud: Find the AppId in the Dashboard. Self-hosting: Use any other string.
        this.GameInstance.AppVersion = "1.0";
        this.GameInstance.PlayerName = "unityPlayer";
        this.GameInstance.Connect();
    }

    void Update()
    {
        this.GameInstance.Service();

        // "back" button of phone will quit
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        } 
    }

    void OnGUI()
    {
        GUILayout.Label("State: " + GameInstance.State.ToString());

        if (!string.IsNullOrEmpty(this.GameInstance.ErrorMessageToShow))
        {
            GUILayout.Label(this.GameInstance.ErrorMessageToShow);
        }

        switch (GameInstance.State)
        {
            case ClientState.JoinedLobby:
                this.OnGUILobby();
                break;
            case ClientState.Joined:
                this.OnGUIJoined();
                break;
        }
    }

    private void OnGUILobby()
    {
        GUILayout.Label("Lobby Screen");
        GUILayout.Label(string.Format("Players in rooms: {0} looking for rooms: {1}  rooms: {2}", this.GameInstance.PlayersInRoomsCount, this.GameInstance.PlayersOnMasterCount, this.GameInstance.RoomsCount));

        if (GUILayout.Button("Create"))
        {
            this.GameInstance.OpJoinRandomRoom(null, 0);
        }

        GUILayout.Label("Rooms to choose from: " + this.GameInstance.RoomInfoList.Count);
        foreach (RoomInfo roomInfo in this.GameInstance.RoomInfoList.Values)
        {
            if (GUILayout.Button(roomInfo.Name))
            {
                this.GameInstance.OpJoinRoom(roomInfo.Name);
            }
        }
    }

    private void OnGUIJoined()
    {
        // we are in a room, so we can access CurrentRoom and it's Players

        GUILayout.Label("Room Screen. Players: " + this.GameInstance.CurrentRoom.Players.Count);
        GUILayout.Label("Room: " + this.GameInstance.CurrentRoom + " Server: " + this.GameInstance.GameServerAddress);
        GUILayout.Label("last move: " + this.GameInstance.lastMoveEv + " ev count: " + this.GameInstance.evCount);

        foreach (Player player in this.GameInstance.CurrentRoom.Players.Values)
        {
            GUILayout.Label("Player: " + player);
        }

        if (GUILayout.Button("Move"))
        {
            this.GameInstance.SendMove();
        }

        if (GUILayout.Button("Rename"))
        {
            this.GameInstance.LocalPlayer.Name = string.Format("unityPlayer{0:00}", "unityPlayer" + Random.Range(0, 100));
        }

        // creates a random property of this room with a random value to set
        if (GUILayout.Button("Set Room Property"))
        {
            Hashtable randomProps = new Hashtable();
            randomProps[Random.Range(0, 2).ToString()] = Random.Range(0, 2).ToString();
            this.GameInstance.CurrentRoom.SetCustomProperties(randomProps);
        }

        // creates a random property of this player with a random value to set
        if (GUILayout.Button("Set Player Property"))
        {
            Hashtable randomProps = new Hashtable();
            randomProps[Random.Range(0, 2).ToString()] = Random.Range(0, 2).ToString();
            this.GameInstance.LocalPlayer.SetCustomProperties(randomProps);
        }

        if (GUILayout.Button("Leave"))
        {
            this.GameInstance.OpLeaveRoom();
        }
    }
}

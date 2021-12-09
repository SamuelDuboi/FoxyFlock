using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class NetworkManagerRace : NetworkRoomManager
{
   [HideInInspector] public int numberOfPlayer = 0;
    public Transform firstPlayer;
    public Transform secondPlayerTransform;

    public GameObject[] startSpawn;
    public int[] avatarsSprite = new int[2];
    public PlayerMovementMulti playerController;
    private GrabManagerMulti[] grabManagers;
    public GameObject player2Canvas;
    public static NetworkManagerRace instance;
    private GameObject[] players = new GameObject[2];
    private int InitNumberOfPlayer;
    private int number;

    public override void Awake()
    {
        base.Awake();

        if (instance == null)
            instance = this;
        else
            Destroy(instance.gameObject);
    }
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // increment the index before adding the player, so first player starts at 1
        clientIndex++;

        if (IsSceneActive(RoomScene))
        {
            if (roomSlots.Count == maxConnections)
                return;

            allPlayersReady = false;

            // Debug.LogFormat(LogType.Log, "NetworkRoomManager.OnServerAddPlayer playerPrefab:{0}", roomPlayerPrefab.name);

            GameObject newRoomGameObject = OnRoomServerCreateRoomPlayer(conn);
            if (newRoomGameObject == null)
            {
                newRoomGameObject = Instantiate(roomPlayerPrefab.gameObject, Vector3.right * 10 * number, Quaternion.identity);
                newRoomGameObject.GetComponentInChildren<RoomPlayer>().number = number;
                number++;
            }

            NetworkServer.AddPlayerForConnection(conn, newRoomGameObject);
        }
        else
            OnRoomServerAddPlayer(conn);
        InitNumberOfPlayer++;
    }
    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnection conn, GameObject roomPlayer)
    {
        int index = 0;
        firstPlayer = GameObject.FindGameObjectWithTag("FirstPlayerPos").transform;
        secondPlayerTransform = GameObject.FindGameObjectWithTag("SecondPlayerPos").transform;

        Transform start = numberOfPlayer == 0 ? firstPlayer : secondPlayerTransform;
        GameObject player = Instantiate(playerPrefab, start.position, start.rotation);

        if (numberOfPlayer == 0)
        {
            playerController = player.GetComponent<PlayerMovementMulti>();
        }
        else
        {
            index = 1;
        }
        player.name = "player " + index;
        players[numberOfPlayer] = player;
        numberOfPlayer++;
        // player.GetComponent<ControllerKeyBoard>().playerId = numberOfPlayer;
        if(index ==InitNumberOfPlayer-1)
        StartCoroutine(WaitToSpawn(conn, players));
        return player;
    }
    IEnumerator WaitToSpawn(NetworkConnection conn, GameObject[] players)
    {
        yield return new WaitForSeconds(1f);
         //playerController.CmdSpawnManager(player);
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < InitNumberOfPlayer; i++)
        {
            if (grabManagers == null)
                grabManagers = new GrabManagerMulti[2];
            grabManagers[i] = players[i].GetComponentInChildren<GrabManagerMulti>();
            if(InitNumberOfPlayer>1)
            playerController.CmdInitUI(i, players[i],true,avatarsSprite[i]);
            else
            {
                playerController.CmdInitUI(i, players[i], false, avatarsSprite[i]);
            }
            grabManagers[i].InitPool(players[i], playerController);
        }
       
    }

    public void Win(int playerId)
    {
        if (playerId == 0)
            playerController.CmdWin1();
        else
            playerController.CmdWin2();
    }
}
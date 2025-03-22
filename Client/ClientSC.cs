using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;

public class ClientSC : NetworkBehaviour
{
    [SerializeField] private List<GameObject> characters = new List<GameObject>();
    [SerializeField] private GameObject canvasObj;
    [SerializeField] private GameObject characterSelectionScreen;
    [SerializeField] private GameObject teamSelectionScreen;

    private GameObject player;

    public int teamIndex;
    
    public override void OnNetworkSpawn()
    {
        if(!IsOwner)
            canvasObj.SetActive(false);
        else //Players first need to select a team.
            characterSelectionScreen.SetActive(false);
    }

    public void ChooseTeamA()
    {
        teamIndex = 0;
        teamSelectionScreen.SetActive(false);
        characterSelectionScreen.SetActive(true);
    }

    public void ChooseTeamB()
    {
        teamIndex = 1;
        teamSelectionScreen.SetActive(false);
        characterSelectionScreen.SetActive(true);
    }
    
    public void SpawnStriker()
    {
        SpawnServerRpc(0);
        canvasObj.SetActive(false);
    }

    public void SpawnWarrior()
    {
        SpawnServerRpc(1);
        canvasObj.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnServerRpc(int spawnIndex)
    {
        switch (teamIndex)
        {
            case 0:
                player = Instantiate(characters[spawnIndex], SpawnPoints.Instance.GetRandomSpawnPoint_TeamA().position, Quaternion.identity);
                player.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);
                CharacterBaseClass playerSCA = player.GetComponent<CharacterBaseClass>();
                playerSCA.teamIndex = 0;
                break;
            case 1:
                player = Instantiate(characters[spawnIndex], SpawnPoints.Instance.GetRandomSpawnPoint_TeamB().position, Quaternion.identity);
                player.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);
                CharacterBaseClass playerSC = player.GetComponent<CharacterBaseClass>();
                playerSC.teamIndex = 1;
                break;
        }
    }
}

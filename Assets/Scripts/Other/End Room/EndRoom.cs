using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndRoom : MonoBehaviour
{
    public Podium[] podiums;
    public Material[] playerMats;
    public Ragdoll ragdollPrefab;
    public Transform playerTP;
    private List<DeadBodySpawnData> deadBodySpawnData;
    void Start()
    {
        deadBodySpawnData = new List<DeadBodySpawnData>();
        GameManager.Instance.OnGameEnd += GetGameResults;
    }

    private void GetGameResults()
    {
        PlayerResults[] results = GameManager.Instance.BuildPlayerResults(out int numOfPlayers);

        for (int i = 0; i < results.Length; i++)
        {
            PlayerResults result = results[i];
            Podium podium = podiums[i];
            if (result == null)
            {
                podium.playerModel.gameObject.SetActive(false);
                podium.username.text = "";
                podium.statsText.text = "";
                podium.numText.text = "";
                podium.light.gameObject.SetActive(false);
                continue;
            }

            // if (i == 0)
            // {
            //     podium.playerModel.Play("Dance");
            // }

            podium.renderer.material = playerMats[result.playerID];
            podium.username.SetText(result.playerName);
            podium.statsText.SetText($"Wins: {result.wins}" + 
            $"\nKills: {result.kills}");

            deadBodySpawnData.Add(new DeadBodySpawnData(result.kills, playerMats[result.playerID], numOfPlayers, podium.bodySpawner));
        }

        StartCoroutine(EndScreenRoutine());
    }

    private IEnumerator EndScreenRoutine()
    {
        PlayerManager.Instance.ChangePov(0);

        yield return new WaitForEndOfFrame();
        PlayerManager.Instance.TeleportPlayer(playerTP.position);
        yield return new WaitForSeconds(3);

        for (int i = 0; i < deadBodySpawnData.Count; i++)
        {
            DeadBodySpawnData d = deadBodySpawnData[i];
            yield return StartCoroutine(SpawnDeadBodies(d.kills, d.culledMaterial, d.numOfPlayers, d.bodySpawner));
            yield return new WaitForSeconds(2f);
        }


        yield return new WaitForSeconds(10f);

        Debug.Log("Game Ends");
        // if (PlayerManager.IsHostClient)
        // {
        //     yield return new WaitForSeconds(2f);
        //     ExitMenu.Instance.ExitToMainMenu();
        // }
        // else
        // {
        //     yield return new WaitForSeconds(1f - (PlayerManager.LocalClientId * .25f));
        //     ExitMenu.Instance.ExitToMainMenu();
        // }
    }

    private IEnumerator SpawnDeadBodies(int kills, Material culledMaterial, int numOfPlayers, Transform bodySpawner)
    {
        OnHitData onHitData = new OnHitData()
        {
            explosionForce = 2.5f, sourceHit = bodySpawner.position, upwardsModifier = .5f
        };
        for (int i = 0; i < kills; i++)
        {
            Material randMat = playerMats[Random.Range(0, numOfPlayers)];

            if (randMat == culledMaterial)
            {
                i--; continue;
            }
            else
            {
                Ragdoll ragdoll = Instantiate(ragdollPrefab, bodySpawner.parent.parent.position, bodySpawner.parent.parent.rotation);
                ragdoll.Create(bodySpawner, randMat, onHitData);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}

[System.Serializable]
public class DeadBodySpawnData
{
    public int kills;
    public Material culledMaterial; 
    public int numOfPlayers;
    public Transform bodySpawner;

    public DeadBodySpawnData(int k, Material c, int n, Transform b)
    {
        kills = k;
        culledMaterial = c;
        numOfPlayers = n;
        bodySpawner = b;
    }
}


[System.Serializable]
public class Podium
{
    public Animator playerModel;
    public SkinnedMeshRenderer renderer;
    public TextMeshPro username;
    public TextMeshPro statsText;
    public TextMeshPro numText;
    public Light light;
    public Transform bodySpawner;
}


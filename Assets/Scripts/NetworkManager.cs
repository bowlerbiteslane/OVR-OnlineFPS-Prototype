using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace UnitySampleAssets.Characters.FirstPerson
{

    public class NetworkManager : MonoBehaviour
    {

        [SerializeField] Transform[] spawnPoints;
        [SerializeField] Camera lobbyCamera;
        [SerializeField] GameObject OVRLobbyCamera;
        [SerializeField] Canvas FPSCanvas;

        private bool usingHMD;

        GameObject player;
        // Use this for initialization
        void Start()
        {
            Screen.lockCursor = true;
            usingHMD = (OVRManager.display.isPresent) ? true : false;
            //usingHMD = true;
            Debug.Log("usingHMD?: " + usingHMD);
            PhotonNetwork.logLevel = PhotonLogLevel.Full;
            PhotonNetwork.ConnectUsingSettings("0.1");
            if (!usingHMD)
            {
                lobbyCamera.enabled = true;
                GameObject.Find("FPSReticle").GetComponent<Image>().enabled = false;
                StartCoroutine("UpdateConnectionString");
            }
            else
            {
                lobbyCamera.enabled = false;
                GameObject.Find("FPSReticle").GetComponent<Image>().enabled = true;
            }
            
        }

        // Update is called once per frame
        IEnumerator UpdateConnectionString()
        {
            while (true)
            {
                if(!usingHMD)
                    FPSCanvas.GetComponent<Text>().text = PhotonNetwork.connectionStateDetailed.ToString();
                yield return null;
            }
        }

        void OnJoinedLobby()
        {

            // PhotonNetwork.JoinRandomRoom(); //Will cause error if one does not exist
            RoomOptions ro = new RoomOptions() { isVisible = true, maxPlayers = 10 };
            PhotonNetwork.JoinOrCreateRoom("DefaultRoom", ro, TypedLobby.Default);
        }

        void OnJoinedRoom()
        {
            if (!usingHMD)
            {
                StopCoroutine("UpdateConnectionString");
                FPSCanvas.GetComponent<Text>().text = "";
                lobbyCamera.enabled = false;
                GameObject.Find("FPSReticle").GetComponent<Image>().enabled = true;
            }
            else
            {

            }
            StartSpawnProcess(0f);
        }

        void StartSpawnProcess(float respawnTime)
        {
            if (!usingHMD)
            {
                lobbyCamera.enabled = true;
                GameObject.Find("FPSReticle").GetComponent<Image>().enabled = true;
                StartCoroutine("SpawnPlayer", respawnTime);
            }
            else
            {
                StartCoroutine("SpawnOVRPlayer", respawnTime);
            }
            //lobbyCamera.GetComponent<OVRCameraRig>().enabled = true;
            //lobbyCamera.GetComponent<OVRManager>().enabled = true;
            //lobbyCamera.GetComponentInChildren<AudioListener>().enabled = true;
            //foreach (Camera cam in lobbyCamera.GetComponentsInChildren<Camera>())
            //    cam.enabled = true;
        }

        IEnumerator SpawnPlayer(float respawnTime)
        {
            yield return new WaitForSeconds(respawnTime);

            lobbyCamera.enabled = false;
            lobbyCamera.gameObject.GetComponent<AudioListener>().enabled = false;
            GameObject.Find("FPSReticle").GetComponent<Image>().enabled = true;

            int index = Random.Range(0, spawnPoints.Length);
            player = PhotonNetwork.Instantiate("FPSPlayer", spawnPoints[index].position, spawnPoints[index].rotation, 0);
            player.GetComponent<PlayerNetworkMover>().RespawnMe += StartSpawnProcess;
        }

        IEnumerator SpawnOVRPlayer(float respawnTime)
        {
            yield return new WaitForSeconds(respawnTime);
            //lobbyCamera.GetComponent<OVRCameraRig>().enabled = false;
            //lobbyCamera.GetComponent<OVRManager>().enabled = false;
            //lobbyCamera.GetComponentInChildren<AudioListener>().enabled = false;
            //foreach (Camera cam in lobbyCamera.GetComponentsInChildren<Camera>())
            //    cam.enabled = false;
            int index = Random.Range(0, spawnPoints.Length);
            player = PhotonNetwork.Instantiate("OVRFPSPlayer", spawnPoints[index].position, spawnPoints[index].rotation, 0);
            player.GetComponent<PlayerNetworkMoverVR>().RespawnMe += StartSpawnProcess;

        }

    }
}

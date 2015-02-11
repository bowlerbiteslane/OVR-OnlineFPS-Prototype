using UnityEngine;
using System.Collections;

namespace UnitySampleAssets.Characters.FirstPerson
{

    public class PlayerNetworkMover : Photon.MonoBehaviour
    {

        public delegate void Respawn(float time);
        public event Respawn RespawnMe;

        Vector3 position;
        Quaternion rotation;
        float smoothing = 10f;
        float health = 100f;

        void Start()
        {
            if (photonView.isMine)
            {
                gameObject.tag = "Player";
                Debug.Log("Player with Tag: " + "'Player'" + " just entered the game.");
                foreach (Camera cam in GetComponentsInChildren<Camera>())
                    cam.enabled = true;
                rigidbody.useGravity = true;
                GetComponent<FirstPersonController>().enabled = true;
                GetComponentInChildren<PlayerShooting>().enabled = true;
                GetComponentInChildren<AudioListener>().enabled = true;
                transform.Find("FirstPersonCharacter/GunCamera/Candy-Cane").gameObject.layer = 11;
            }
            else
            {
                gameObject.tag = "OtherPlayer";
                Debug.Log("Player with Tag: " + "'OtherPlayer'" + " just entered the game.");
                StartCoroutine("UpdateData");
            }
        }

        IEnumerator UpdateData()
        {
            while (true)
            {
                transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * smoothing);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * smoothing);
                yield return null;
            }
        }


        void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
                stream.SendNext(health);
            }
            else
            {
                position = (Vector3)stream.ReceiveNext();
                rotation = (Quaternion)stream.ReceiveNext();
                health = (float)stream.ReceiveNext();
            }
        }

        [RPC]  // Remote Procedural Call
        public void GetShot(float damage)
        {
            health -= damage;
            if (health <= 0 && photonView.isMine)
            {
                if (RespawnMe != null)
                    RespawnMe(3f);

                PhotonNetwork.Destroy(gameObject);
            }
        }

    }
}

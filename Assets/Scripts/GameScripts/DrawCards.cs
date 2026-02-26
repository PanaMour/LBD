using UnityEngine;
using Mirror;

public class DrawCards : NetworkBehaviour
{
    public void OnClick()
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        if (networkIdentity != null)
        {
            PlayerManager pm = networkIdentity.GetComponent<PlayerManager>();
            pm.OnDrawPhaseClicked();
        }
    }
}
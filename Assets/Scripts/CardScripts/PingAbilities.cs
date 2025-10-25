using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PingAbilities : CardAbilities
{
    public override void OnCompile()
    {
        PlayerManager.CmdGMChangeVariables(1);
    }

    public override void OnExecute()
    {
        PlayerManager.CmdGMChangeLP(0, gameObject.GetComponent<CardStats>().ATK);
    }
}

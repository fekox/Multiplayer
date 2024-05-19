using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Player
{
    public string tagName;
    public int ID;

    public Player(int id, string name)
    {
        this.ID = id;
        this.tagName = name;
    }

    public string GetPlayerName()
    {
        return tagName;
    }

    public int GetPlayerID()
    {
        return ID;
    }
}

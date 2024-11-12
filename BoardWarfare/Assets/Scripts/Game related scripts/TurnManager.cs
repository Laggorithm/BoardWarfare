using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    enum Turn {Player, Ai};
    
    // Start is called before the first frame update
    void Start()
    {
        Turn currentturn = Turn.Ai;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

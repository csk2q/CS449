using System.Collections.Generic;

namespace SOS_Game;

public class GameRecord
{
    public bool BluePlayerIsComputer;
    public bool RedPlayerIsComputer;
    
    public GameType GameType;
    public int BoardSize;

    public TurnResult[] TurnResults = [];

}
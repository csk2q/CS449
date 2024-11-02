using System.Collections.Generic;

namespace SOS_Game.Logic;

public class IntMaxCompare : IComparer<int>
{
    public int Compare(int x, int y)
    {
        return x - y;
    }
}
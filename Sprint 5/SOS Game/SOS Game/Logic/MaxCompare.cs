using System.Collections.Generic;

namespace SOS_Game.Logic;


public class IntPickMaxCompare : IComparer<int>
{
    public int Compare(int x, int y)
    {
        return y - x;
    }
}
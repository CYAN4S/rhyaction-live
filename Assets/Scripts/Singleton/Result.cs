using Core;

namespace CYAN4S
{
    public class Result : Singleton<Result>
    {
        public int score;
        public double accuracy;
        public int[,] judgeCount;
    }

}
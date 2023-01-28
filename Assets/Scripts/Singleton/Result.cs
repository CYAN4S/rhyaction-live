using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;

namespace CYAN4S
{
    public class Result : Singleton<Result>
    {
        public Situation situation;

        public int score;
        public int[,] judgeCount;
    }

}
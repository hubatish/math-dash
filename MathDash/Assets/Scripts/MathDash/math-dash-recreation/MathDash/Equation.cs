using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;

namespace MathDash
{
    public class Equation
    {
        public int? leftNum;
        public int? rightNum;
        public int? solution;
        public Op operation;

        public static string OpToSymbol(Op op)
        {
            switch (op)
            {
                case Op.Add:
                    return "+";
                case Op.Subtract:
                    return "-";
                case Op.Divide:
                    return "/";
                case Op.Multiply:
                    return "x";
            }
            return "|";
        }

        public static int ComputeResult(int a, int b, Op op)
        {
            switch (op)
            {
                case Op.Add:
                    return a + b;
                case Op.Subtract:
                    return a - b;
                case Op.Divide:
                    return a / b;
                case Op.Multiply:
                    return a * b;
            }
            //nothing else worked
            return -1;
        }

        public override string ToString()
        {
            return "solution" + (solution ?? -1).ToString() + "\n" + "operation" + operation.ToString();
        }
    }
}

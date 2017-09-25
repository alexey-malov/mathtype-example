using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleApp1
{
    class Program
    {
        private const string MATH_ML_2_TRANSLATOR = "MathML2 (no namespace).tdl";

        static void Main(string[] args)
        {
            var srcFilePath = "ManyCharacters.mtef";
            var equationBytes = File.ReadAllBytes(srcFilePath);
            var mathType = new MathTypeSdkFacade();
            var result = mathType.ConvertEquation(equationBytes, MATH_ML_2_TRANSLATOR);
            Console.Out.Write(result);
        }
    }
}

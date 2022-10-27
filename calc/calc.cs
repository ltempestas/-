using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace calc
{
    class Lab
    {
        static private bool IsDelimeter(char c) 
        { 
            return " =".IndexOf(c) != -1; 
        }
        static private bool IsOperator(char с) 
        {
            return "+-/*()".IndexOf(с) != -1; 
        }
        static private byte GetOrder(char s)
        {
            switch (s)
            {
                case '(': return 0;
                case ')': return 1;
                case '+': return 2;
                case '-': return 2;
                case '*': return 3;
                case '/': return 3;
                default: return 4;
            }
        }
        static public double Calculate(string input)
        {
            string output = GetExpression(input);
            double result = Counting(output);
            return result;
        }


        static private string GetExpression(string input)
        {
            string output = string.Empty;
            Stack<char> operStack = new Stack<char>();

            for (int i = 0; i < input.Length; i++)
            {

                if (Char.IsDigit(input[i]))
                {

                    while (!(IsDelimeter(input[i]) || IsOperator(input[i])))
                    {
                        output += input[i];
                        i++;

                        if (i == input.Length) break;

                        
                    }

                    output += " ";
                    i--;
                }

                if (IsOperator(input[i]))
                {
                    if (input[i] == '(')
                        operStack.Push(input[i]);
                    else if (input[i] == ')')
                    {
                        char s = operStack.Pop();

                        while (s != '(')
                        {
                            output += s.ToString() + ' ';
                            s = operStack.Pop();
                        }
                    }
                    else
                    {
                        if (operStack.Count > 0)
                            if (GetOrder(input[i]) <= GetOrder(operStack.Peek()))
                                output += operStack.Pop().ToString() + " ";

                        operStack.Push(char.Parse(input[i].ToString()));
                    }
                }
            }


            while (operStack.Count > 0)
                output += operStack.Pop() + " ";

            return output;
        }




        static private double Counting(string input)
        {
            double result = 0;
            Stack<double> temp = new Stack<double>();

            for (int i = 0; i < input.Length; i++)
            {
                if (Char.IsDigit(input[i]))
                {
                    string a = string.Empty;

                    while (!(IsDelimeter(input[i]) || IsOperator(input[i])))
                    {
                        a += input[i];
                        i++;
                        if (i == input.Length) break;
                    }
                    temp.Push(double.Parse(a));
                    i--;
                }
                else if (IsOperator(input[i]))
                {
                    double a = temp.Pop();
                    double b = temp.Pop();

                    switch (input[i])
                    {
                        case '+': result = b + a; break;
                        case '-': result = b - a; break;
                        case '*': result = b * a; break;
                        case '/': result = b / a; break;
                    }
                    temp.Push(result);
                }
            }
            return temp.Peek();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("Введите выражение: ");
                Console.WriteLine(Lab.Calculate(Console.ReadLine()));
            }
        }
    }
}

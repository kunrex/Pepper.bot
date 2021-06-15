using System;
using System.Collections.Generic;

using KunalsDiscordBot.Services;

namespace KunalsDiscordBot.Services.Math
{
    public class LinearEquationSolver : BotService
    {
        public LinearEquationSolver(string equation)
        {
            VarName = FindVariable(equation); 
        }
        private char VarName { get; set; }

        private char FindVariable(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                bool isNumber = Char.IsDigit(s[i]);
                if (!isNumber)//not a number
                {
                    bool isLetter = Char.IsLetter(s[i]);
                    if (isLetter)
                        return s[i];
                }
            }
            return '.';//some value
        }

        public List<string> GetPolynomials(string s)
        {
            List<string> polynomials = new List<string>();
            bool foundVal = false;
            string val = "";
            string sign = "";
            bool foundEqualTo = false;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] != ' ' && s[i] != '-' && s[i] != ' ' && s[i] != '=')
                {
                    if (foundVal == false)
                    {
                        foundVal = true;
                        val += s[i];
                    }
                    else
                        val += s[i];
                }
                else if (s[i] == '-')
                {
                    sign = "-";
                }
                else if (s[i] == '=')
                {
                    foundEqualTo = true;
                    foundVal = false;
                    val = "";
                    sign = "";
                }
                else
                {
                    if (val == "")
                        continue;
                    if (Contains(val) && foundEqualTo)
                    {
                        if (sign == "-")
                            sign = "";
                        else
                            sign = "-";
                    }
                    else if (!Contains(val) && !foundEqualTo)
                    {
                        if (sign == "-")
                            sign = "";
                        else
                            sign = "-";
                    }


                    if (val != "+" && val != "-")
                    {
                        polynomials.Add(sign + val);
                    }
                    else
                        polynomials.Add(val);

                    foundVal = false;
                    val = "";
                    sign = "";
                }
            }

            return polynomials;
        }

        public string Solve(List<string> polynomials)
        {
            double numericVal = 0, varNumericVal = 0;
            for (int i = 0; i < polynomials.Count; i++)
            {
                if (Contains(polynomials[i]))
                {
                    varNumericVal = DealWithVariables(polynomials[i], varNumericVal);
                }
                else
                {
                    numericVal += Double.Parse(polynomials[i]);
                }
            }
            double answer = numericVal / varNumericVal;
            string answerToString = VarName + " is equal to " + answer;
            return answerToString;
        }

        private double DealWithVariables(string s, double numericVal)
        {
            if (s.Length == 0)
                return numericVal;
            else if (s.Length == 1 && s[0] == VarName)
                return numericVal + 1;
            else if (s.Length == 2 && s[0] == '-')
            {
                return numericVal - 1;
            }

            String number = "";
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] != VarName && s[i] != ' ' && s[i] != '+')
                    number += s[i];
            }

            if (number != "")
                numericVal += Double.Parse(number);

            return numericVal;
        }

        private bool Contains(string s)
        {
            if (s.Length == 0)
                return true;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == VarName || s[i] == '+' || s[i] == ' ')
                {
                    return true;
                }
            }

            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KunalsDiscordBot.Core.Modules.MathCommands
{
    //This was 1 year old programmer me smashing his keyboard, so if this code makes you cry im sorry
    public class LinearEquationSolver 
    {
        public LinearEquationSolver(string _equation)
        {
            equation = _equation;
            varName = FindVariable(equation); 
        }

        private readonly string equation;
        private readonly char varName;
        private List<string> polynomials { get; set; }

        private char FindVariable(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                bool isNumber = char.IsDigit(s[i]);
                if (!isNumber)//not a number
                {
                    bool isLetter = char.IsLetter(s[i]);
                    if (isLetter)
                        return s[i];
                }
            }
            return '.';//some value
        }

        private void GetPolynomials()
        {
            polynomials = new List<string>();
            bool foundVal = false;
            string val = "";
            string sign = "";
            bool foundEqualTo = false;

            for (int i = 0; i < equation.Length; i++)
            {
                if (equation[i] != ' ' && equation[i] != '-' && equation[i] != ' ' && equation[i] != '=')
                {
                    if (foundVal == false)
                    {
                        foundVal = true;
                        val += equation[i];
                    }
                    else
                        val += equation[i];
                }
                else if (equation[i] == '-')
                {
                    sign = "-";
                }
                else if (equation[i] == '=')
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
        }

        public Task<string> Solve()
        {
            GetPolynomials();

            double numericVal = 0, varNumericVal = 0;
            for (int i = 0; i < polynomials.Count; i++)
            {
                if (Contains(polynomials[i]))
                {
                    varNumericVal = DealWithVariables(polynomials[i], varNumericVal);
                }
                else
                {
                    numericVal += double.Parse(polynomials[i]);
                }
            }
            double answer = numericVal / varNumericVal;
            string answerToString = varName + " is equal to " + answer;

            return Task.FromResult(answerToString);
        }

        private double DealWithVariables(string s, double numericVal)
        {
            if (s.Length == 0)
                return numericVal;
            else if (s.Length == 1 && s[0] == varName)
                return numericVal + 1;
            else if (s.Length == 2 && s[0] == '-')
            {
                return numericVal - 1;
            }

            string number = "";
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] != varName && s[i] != ' ' && s[i] != '+')
                    number += s[i];
            }

            if (number != "")
                numericVal += double.Parse(number);

            return numericVal;
        }

        private bool Contains(string s)
        {
            if (s.Length == 0)
                return true;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == varName || s[i] == '+' || s[i] == ' ')
                {
                    return true;
                }
            }

            return false;
        }
    }
}

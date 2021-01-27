//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Vertex
//{
//    class Calculator
//    {
//        // Here implements Calculator tag.
//        // It contains ordered list of calculations. Calculations
//        // are structs that define a single calculation step of
//        // the validation. Here the validation is actually what
//        // the calculator does. The calculator will reply the 
//        // result to the evaluate() function.

//        public List<Calculation> Calculations = new List<Calculation>();
//        public string Result = "";

//        public void Calculate(List<ParameterItem> Parameters)
//        {
//            if (Parameters is null)
//            {
//                throw new ArgumentNullException(nameof(Parameters));
//            }

//            // Step 1: Get the parameters
//            // Step 2: Read and execute calculations one by one
//            // Step 3: Return the result to result property

//            // HERE IS STEP 2
//            foreach (Calculation calculation in Calculations)
//            {
//                foreach(var item in Parameters)
//                {
//                    // Copy oprs from parameters to calculation structure
//                    calculation.Oprs.Add(item.Value.ToString());
//                }

//                // perform the calculation
//                // The declaration of tmp vars.
//                int IntA, IntB;
//                bool BoolA, BoolB;

//                // The 'great' ugly repetitive switch
//                switch (calculation.ToPerform)
//                {
//                    case CalculationType.ADD:
//                        // Not implements 'ADD' calculation
//                        // Steps: to int -> perform -> to string
//                        //int IntA, IntB;
//                        IntA = int.Parse(calculation.Oprs[0]);
//                        IntB = int.Parse(calculation.Oprs[1]);
//                        IntA += IntB;
//                        Result = IntA.ToString();
//                        break;

//                    case CalculationType.SUB:
//                        // Not implements 'SUB' calculation
//                        // Steps: to int -> perform -> to string
//                        //int IntA, IntB;
//                        IntA = int.Parse(calculation.Oprs[0]);
//                        IntB = int.Parse(calculation.Oprs[1]);
//                        IntA -= IntB;
//                        Result = IntA.ToString();
//                        break;

//                    case CalculationType.MUL:
//                        // Not implements 'MUL' calculation
//                        // Steps: to int -> perform -> to string
//                        //int IntA, IntB;
//                        IntA = int.Parse(calculation.Oprs[0]);
//                        IntB = int.Parse(calculation.Oprs[1]);
//                        IntA *= IntB;
//                        Result = IntA.ToString();
//                        break;

//                    case CalculationType.DIV:
//                        // Not implements 'DIV' calculation
//                        // Steps: to int -> perform -> to string
//                        //int IntA, IntB;
//                        try
//                        {
//                            IntA = int.Parse(calculation.Oprs[0]);
//                            IntB = int.Parse(calculation.Oprs[1]);
//                            IntA /= IntB;
//                            Result = IntA.ToString();
//                        }
//                        catch(DivideByZeroException)
//                        {
//                            Console.Error.WriteLine("[ERROR] DivideByZeroException caught!");
//                            Result = "";
//                        }
//                        break;

//                    case CalculationType.NOT:
//                        // Usage: NOT the first opr.
//                        BoolA = bool.Parse(calculation.Oprs[0]);
//                        BoolA = !BoolA;
//                        Result = BoolA.ToString();
//                        break;

//                    case CalculationType.AND:
//                        BoolA = bool.Parse(calculation.Oprs[0]);
//                        BoolB = bool.Parse(calculation.Oprs[1]);
//                        BoolA = BoolA && BoolB;
//                        Result = BoolA.ToString();
//                        break;

//                    case CalculationType.OR:
//                        BoolA = bool.Parse(calculation.Oprs[0]);
//                        BoolB = bool.Parse(calculation.Oprs[1]);
//                        BoolA = BoolA || BoolB;
//                        Result = BoolA.ToString();
//                        break;

//                    case CalculationType.XOR:
//                        BoolA = bool.Parse(calculation.Oprs[0]);
//                        BoolB = bool.Parse(calculation.Oprs[1]);
//                        BoolA ^= BoolB;
//                        Result = BoolA.ToString();
//                        break;

//                    case CalculationType.NAND:
//                        BoolA = bool.Parse(calculation.Oprs[0]);
//                        BoolB = bool.Parse(calculation.Oprs[1]);
//                        BoolA = BoolA && BoolB;
//                        BoolA = !BoolA;
//                        Result = BoolA.ToString();
//                        break;

//                    default:
//                        Console.Error.WriteLine("[WARNING] You are using an unsupported calculation.");
//                        Result = "";
//                        break;
//                }
//            }
//        }
//    }

//    struct Calculation
//    {
//        // Here implements a single calculation's structure
//        public CalculationType ToPerform;
//        public List<string> Oprs;
//    }

//    enum CalculationType
//    {
//        ADD, SUB, MUL, DIV,
//        AND, OR, XOR, NOT, NAND
//    }
//}
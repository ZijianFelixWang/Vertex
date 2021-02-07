﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading;
using System.Threading.Tasks;
//using NLog;
using ResourceHelper = Vertex.IOSupport.ResourceHelper;

namespace Vertex.Kernel
{
    class Evaluator
    {
        //private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public List<ParameterList> ParameterLists = new List<ParameterList>();
        //public ParameterList AnswerList = new ParameterList();
        public List<ParameterList> AnswerLists = new List<ParameterList>();
        public RIBinder RIBinder;   // RI: Ready Indicator (cell)

        // Top point: How to implement <Calculator> tag...
        //public Calculator Calculator;

        private bool success = true;
        private short currentRanking = 0;
        private short maxRanking = 0;
        public List<short> RankingHistory = new List<short>();

        public void ConfigCurrentRanking(short CurrentRanking)
        {
            currentRanking = CurrentRanking;
            if (currentRanking > maxRanking)
            {
                throw new ArgumentOutOfRangeException(nameof(CurrentRanking), "CurrentRanking cannot be larger than MaxRanking!");
            }
        }

        public void ConfigMaxRanking(short MaxRanking)
        {
            maxRanking = MaxRanking;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public void Execute(ref IO IODefinition, ref Matrix MatrixDefinition, ref RulePool RulePoolDefinition)
        {
            if (IODefinition is null)
            {
                throw new ArgumentNullException(nameof(IODefinition));
            }

            if (MatrixDefinition is null)
            {
                throw new ArgumentNullException(nameof(MatrixDefinition));
            }

            if (RulePoolDefinition is null)
            {
                throw new ArgumentNullException(nameof(RulePoolDefinition));
            }

            // Here executes the automata for one time...
            #region
            // To execute, one need:
            //  - The automata's CM
            //  - The rule to perform

            // Algorithm to get the rule to perform
            // a. a rule is a sequence of bits generated by RuleGenerator.
            // b. to get a rule for the matrix to perform, the function needs access
            //      to the rule generator via the rule pool of the environment.

            // Definition of each bits of a rule
            /*
             * A rule defines what to do on what situation.
             * 
             * Total situations count (TSC) calculated by human:
             * TSC = 2^9 = 512 situations
             * 
             * Then for each situation one need to specify the result of the cell 
             * itself so a rule contains 512 bits.
             * 
             * Yet then the executor needs to know what each bit represents so that
             * we name situation ALL 0 as s[0], ALL 1 as s[511]. For example, situation
             * 000000110 means when (-1,-1)(0,-1) are 1 while others are 0 and this is
             * the [6] situation.
             * 
             * Respective coordinate system definition:
             * (-1,1)  (0,1)  (1,1) Where (0,0) represents the current cell to execute.
             * (-1,0)  (0,0)  (1,0)
             * (-1,-1) (0,-1) (1,-1)
             */
            #endregion

            // STEP I: Get rule from GA (RulePool) for Matrix
            bool[] rule = RulePoolDefinition.Next();

            // STEP II: EXECUTE THE RULE ON THE MATRIX.
            // - Stage A. Backup the cell list
            List<Cell> CellsBackup = MatrixDefinition.Cells;

            // - Stage B. Execute the cell list
            if (MatrixDefinition.UseMultiThreading == false)
            {
                //Console.WriteLine("Traditional single-thread mode (Slow but saves memory if CM large).");
                Console.Write("0%... ");
                for (uint x = 0, y = 0; y < MatrixDefinition.Rows; x++)
                {
                    if (x >= MatrixDefinition.Columns)
                    {
                        x = 0;
                        y++;
                        Console.Write(y / (double)MatrixDefinition.Rows * 100.00 + "%... ");
                    }

                    ExecuteCell(CellsBackup, ref MatrixDefinition, IODefinition, rule, x, y);
                }
                Console.WriteLine();
            }
            else
            {
                //Console.WriteLine("Modern multi-threading mode (Quick but costs much memory if CM large).");
                List<Task<Matrix>> tasks = new List<Task<Matrix>>(CellsBackup.Count);

                // Delegate definition
                Func<TECParam, Matrix> exec = TaskExecuteCell;

                // Construct tasks
                for (uint x = 0, y = 0, t = 0; y < MatrixDefinition.Rows; x++, t++)
                {
                    if (x >= MatrixDefinition.Columns)
                    {
                        x = 0;
                        y++;
                    }

                    if (t >= CellsBackup.Count)
                    {
                        break;
                    }

                    TECParam pm = new TECParam
                    {
                        CellsBackup = CellsBackup,
                        MatrixDefinition = MatrixDefinition,
                        IODefinition = IODefinition,
                        rule = rule,
                        x = x,
                        y = y,
                        t = t
                    };

                    tasks.Add(new Task<Matrix>(() => TaskExecuteCell(pm)));
                    //Console.WriteLine("Constructed task[" + t + "]");
                }
                //Logger.Info("Tasks construction ok.");
                ResourceHelper.Log(IOSupport.VxLogLevel.Verbose, "TasksConstructedInfo");

                // Start tasks
                foreach (var t in tasks)
                {
                    t.Start();
                    //t.RunSynchronously();
                }

                // Wait for the tasks
                Task.WaitAll(tasks.ToArray());
                //Logger.Info("All tasks OK.");
                ResourceHelper.Log(IOSupport.VxLogLevel.Verbose, "TasksDoneInfo");

                // Update from t.Result matrix
                for (int ti = 0; ti < tasks.Count; ti++)
                {
                    //Console.WriteLine("ti= " + ti);
                    Matrix modified = tasks.ToArray()[ti].Result;
                    MatrixDefinition.Cells[ti] = modified.Cells[ti];
                }
                //Logger.Info("All cells updated.");
                ResourceHelper.Log(IOSupport.VxLogLevel.Verbose, "CellsUpdatedInfo");
            }

            // Print resulted matrix
            if (MatrixDefinition.ShowRealTimeMatrix == true)
            {
                #region
                ConsoleColor fgBak = Console.ForegroundColor;
                ConsoleColor bgBak = Console.BackgroundColor;

                for (int idx = 0, loc = 0; idx < CellsBackup.Count; idx++, loc++)
                {
                    if (loc >= MatrixDefinition.Columns)
                    {
                        // Next row
                        Console.BackgroundColor = bgBak;
                        Console.WriteLine();
                        Console.BackgroundColor = bgBak;
                        loc = 0;
                    }

                    Console.BackgroundColor = MatrixDefinition.Cells[idx].Value == true ? ConsoleColor.Green : ConsoleColor.Blue;

                    #region
                    //if (MatrixDefinition.Cells[idx].Value != CellsBackup[idx].Value)
                    //{
                    //    // different
                    //    Console.ForegroundColor = ConsoleColor.Red;
                    //    Console.Write("*");
                    //    Console.ForegroundColor = fgBak;
                    //}
                    //else
                    //{
                    //    Console.Write(".");
                    //}

                    char ch = '.';
                    // Figure out if [idx] is a VCIOCell
                    // Inputs
                    uint defornot = 0;
                    try
                    {
                        defornot = IODefinition.Inputs.FirstOrDefault(p => p.Value.ID == idx).Value.ID;
                    }
                    catch (NullReferenceException)
                    {
                        defornot = 0;
                    }

                    if (defornot != 0)
                    {
                        // in inputs
                        ch = 'I';
                    }

                    // Outputs
                    try
                    {
                        defornot = IODefinition.Outputs.FirstOrDefault(p => p.Value.ID == idx).Value.ID;
                    }
                    catch (NullReferenceException)
                    {
                        defornot = 0;
                    }

                    if (defornot != 0)
                    {
                        ch = 'O';
                    }

                    // ReadyIndicators
                    try
                    {
                        Dictionary<string, VCIOCell> ReadyIndicators = new Dictionary<string, VCIOCell>
                        {
                            { IODefinition.ReadyIndicator.name, IODefinition.ReadyIndicator.content }
                        };
                        defornot = ReadyIndicators.FirstOrDefault(p => p.Value.ID == idx).Value.ID;
                    }
                    catch (NullReferenceException)
                    {
                        defornot = 0;
                    }
                    if (defornot != 0)
                    {
                        // in ris
                        ch = 'R';
                    }
                    #endregion
                    Console.Write(ch);
                }

                Console.ForegroundColor = fgBak;
                Console.BackgroundColor = bgBak;
                Console.WriteLine();
                #endregion
            }
        }

        private static Matrix TaskExecuteCell(TECParam p)
        {
            ExecuteCell(p.CellsBackup, ref p.MatrixDefinition, p.IODefinition, p.rule, p.x, p.y);
            //Console.WriteLine($"Task {Task.CurrentId} has finished. (CID= {p.MatrixDefinition.FindCellIndexByLocation((int)p.x, (int)p.y)})");
            return p.MatrixDefinition;
        }

        private static void ExecuteCell(in List<Cell> CellsBackup, ref Matrix MatrixDefinition, in IO IODefinition, bool[] rule, uint x, uint y)
        {
            //Console.Write($"({x},{y}) ");
            #region
            // Here programs the logic for a particular cell of (x,y) in the matrix
            // the logic will use rule loaded from rule[] array; and will use the
            // original matrix loaded from the List<Cell> called CellsBackup. What's
            // more, the altered value will be written into the List<Cell> Cells. The
            // both cell lists are members of the parameter MatrixDefinition. Remember,
            // it is a reference parameter, so the delta to the parameter will affect
            // the data stored in the entity matrix.

            // For cell (x,y)
            /* 
             * (-1,1)   =   (x-1, y+1)      | FORMULA FOR THIS TRANSITION
             * (0,1)    =   (x, y+1)        |
             * (1,1)    =   (x+1, y+1)      | The key point is,
             * (-1,0)   =   (x-1, y)        | RealLoc(xr, yr) = (x,y) + RelativeLoc(deltax, deltay)
             * (0,0)    =   (x, y)
             * (1,0)    =   (x+1, y)
             * (-1,-1)  =   (x-1, y-1)
             * (0,-1)   =   (x, y-1)
             * (1,-1)   =   (x+1, y-1)
             */
            #endregion

            bool[] situation = new bool[9];

            // To get situation array:
            // situation[0] = (-1,1)
            // situation[1] = (0,1)
            // situation[(1-rerlativeY)*3+relativeX+1] = (relativeX, relativeY)
            for (short relativeY = 1; relativeY >= -1; relativeY--)
            {
                for (short relativeX = -1; relativeX <= 1; relativeX++)
                {
                    int realX = (int)(x + relativeX);
                    int realY = (int)(y + relativeY);

                    //Console.WriteLine("---> rx= " + realX + " ry= " + realY);

                    uint? index = MatrixDefinition.FindCellIndexByLocation(realX, realY);
                    if (index == null)
                    {
                        continue;
                    }

                    uint sindex = (uint)((1 - relativeY) * 3 + relativeX + 1);  // index of situation

                    if (index == null)
                    {
                        situation[sindex] = false;
                    }
                    else
                    {
                        // not null fetch cell.Value;
                        situation[sindex] = CellsBackup[(int)index].Value;
                    }
                }
            }

            // To find solution from the rule:
            /*
             * SITUATION    #
             * 000000000    0
             * 000000001    1
             * 000000010    10
             * 000000011    11
             * .........    ..
             */

            int situNo = 0;
            string bin = new string('0', 9);
            StringBuilder binBuilder = new StringBuilder(bin);

            for (int idx = 0; idx < 9; idx++)
            {
                if (situation[idx])
                {
                    binBuilder[idx] = '1';
                }
            }
            bin = binBuilder.ToString();
            situNo = Convert.ToInt32(bin, 2);
            // so that the target value is rule[situNo];

            // Now is the core part -- 'execute'
            // PART I. Check if its a VCIOCell which shouldn't be executed?
            #region
            // Cell: MatrixDefinition.Cells[(int)MatrixDefinition.FindCellIndexByLocation((int)x, (int)y)];
            // Algorithm: Check IOD.Inputs... IOD.Outputs... IOD.RIs...
            bool execute = true;
            int? cellID = (int?)MatrixDefinition.FindCellIndexByLocation((int)x, (int)y);

            if (cellID == null)
            {
                return;
            }

            // Inputs
            uint defornot = 0;
            try
            {
                defornot = IODefinition.Inputs.FirstOrDefault(p => p.Value.ID == cellID).Value.ID;
            }
            catch (NullReferenceException)
            {
                defornot = 0;
            }

            if (defornot != 0)
            {
                // in inputs
                execute = IODefinition.Inputs.FirstOrDefault(p => p.Value.ID == cellID).Value.Execute;
            }

            // Outputs
            try
            {
                defornot = IODefinition.Outputs.FirstOrDefault(p => p.Value.ID == cellID).Value.ID;
            }
            catch (NullReferenceException)
            {
                defornot = 0;
            }

            if (defornot != 0)
            {
                // in inputs
                execute = IODefinition.Outputs.FirstOrDefault(p => p.Value.ID == cellID).Value.Execute;
            }

            // ReadyIndicators
            try
            {
                //defornot = IODefinition.ReadyIndicators.FirstOrDefault(p => p.Value.ID == cellID).Value.ID;
                Dictionary<string, VCIOCell> ReadyIndicators = new Dictionary<string, VCIOCell>
                        {
                            { IODefinition.ReadyIndicator.name, IODefinition.ReadyIndicator.content }
                        };
                defornot = ReadyIndicators.FirstOrDefault(p => p.Value.ID == cellID).Value.ID;
            }
            catch (NullReferenceException)
            {
                defornot = 0;
            }
            if (defornot != 0)
            {
                // in inputs
                execute = IODefinition.ReadyIndicator.content.Execute;
            }
            #endregion

            // PART II. Execute the cell.
            if (execute == true)
            {
                MatrixDefinition.Cells[(int)cellID].Value = rule[situNo];
            }
            //else
            //{
            //    Console.WriteLine("(Skip)");
            //}
        }

        public bool Evaluate(ref IO IODefinition, ref Matrix MatrixDefinition, ref RulePool RulePoolDefinition)
        {
            if (MatrixDefinition is null)
            {
                throw new ArgumentNullException(nameof(MatrixDefinition));
            }

            if (RulePoolDefinition is null)
            {
                throw new ArgumentNullException(nameof(RulePoolDefinition));
            }

            if (IODefinition is null)
            {
                throw new ArgumentNullException(nameof(IODefinition));
            }

            //_ = RulePoolDefinition.Produce(RankingHistory);
            //Logger.Info("Evaluating rule: ");
            ResourceHelper.Log("EvaluatingHint");
            for (int i = 0; i < RulePoolDefinition.RuleLength; i++)
            {
                Console.Write(RulePoolDefinition.GetLatest()[i] ? "1" : "0");
            }
            Console.WriteLine();

            success = true;
            for (ushort Index = 0; Index < ParameterLists[0].Parameters.Count; Index++)
            {
                RIBinder.ResetTimes();
                // New workspace
                MatrixDefinition.ResetCells();

                //ParameterLists.Count
                // Here implements what an evaluator does for ONE try
                // STEP 1: Fetch oprs in order and give them to the automata
                // STEP 2: Fetch the answer from the automata
                // STEP 3: Use the calculator to get the result
                // STEP 4: Check the result with the answer

                // STEP 1
                #region
                // TO: List<ParameterItem> Parameters
                foreach (ParameterList pl in ParameterLists)
                {
                    //IODefinition.Inputs.Find(x => x.Name == pl.For).Value = pl.Parameters[Index].Value;
                    var id = IODefinition.Inputs.FirstOrDefault(p => p.Value.Name == pl.For).Key;
                    //Console.WriteLine("TEMP_DATA: " + IODefinition.Inputs[id].Value);
                    //IODefinition.Inputs[id].Value = pl.Parameters[Index].Value;
                    var temp = IODefinition.Inputs[id];
                    temp.Value = pl.Parameters[Index].Value;
                    IODefinition.Inputs[id] = temp;
                }
                #endregion

                IODefinition.UpdateCellsFromInput(ref MatrixDefinition.Cells);

                // STEP 2
                // Detailed stages of STEP II:
                // a.   the following operations should be done in a loop
                //      the loop should be terminated if RIBinder.TryAnotherTime()
                //      returns the FALSE value. (BREAK)
                // b.   If true, does the loop contains the following things:
                // c.   Check the RIBinder's value.
                // d.   If TRUE, does RIBinder.On; if FALSE, does RIBinder.Off

                // RIBAction definitions:
                //  - ToShiftNext: skip this time of foreach, don't alt success
                //  - Break: kill the automata, success = false
                //  - Continue: let the automata does another execution

                do
                {
                    // Stage c. and Stage d.
                    // RIBinder: IODefinition.ReadyIndicators[RIBinder.Indicator]
                    //switch (IODefinition.ReadyIndicators[RIBinder.Indicator].Value)
                    IODefinition.Sync(MatrixDefinition.Cells);
                    //switch (IODefinition.ReadyIndicators.FirstOrDefault(ri => ri.Value.Name == RIBinder.Indicator).Value.Value)
                    switch (IODefinition.ReadyIndicator.content.Value)
                    {
                        case true:
                            // To Execute: RIBinder.On
                            switch (RIBinder.On)
                            {
                                case RIBAction.Break:
                                    success = false;
                                    return success;
                                // break;   -- unreachable

                                case RIBAction.Continue:
                                    var times = RIBinder.GetTimes();
                                    //Console.Clear();
                                    //ConsoleColor fgBak = Console.ForegroundColor;
                                    //ConsoleColor bgBak = Console.BackgroundColor;
                                    //Console.BackgroundColor = ConsoleColor.Red;
                                    //Console.ForegroundColor = ConsoleColor.Blue;
                                    //Logger.Info("Execute[" + Index + "," + times + "]");
                                    ResourceHelper.Log("ExecutionHint", Index + "," + times + "]");

                                    //Console.ForegroundColor = fgBak;
                                    //Console.BackgroundColor = bgBak;
                                    Console.WriteLine();

                                    Execute(ref IODefinition, ref MatrixDefinition, ref RulePoolDefinition);
                                    break;

                                case RIBAction.ToShiftNext:
                                    //Index++ included in loop definition;
                                    continue;

                                case RIBAction.OK:
                                    //Console.WriteLine("RI_TRUE: OK");
                                    RIBinder.HackTime();
                                    break;
                            }
                            break;

                        case false:
                            // To Execute: RIBinder.Off
                            switch (RIBinder.Off)
                            {
                                case RIBAction.Break:
                                    success = false;
                                    return success;
                                // break;   -- unreachable

                                case RIBAction.Continue:
                                    var times = RIBinder.GetTimes();
                                    //Console.Clear();
                                    //ConsoleColor fgBak = Console.ForegroundColor;
                                    //ConsoleColor bgBak = Console.BackgroundColor;
                                    //Console.BackgroundColor = ConsoleColor.Red;
                                    //Console.ForegroundColor = ConsoleColor.Blue;
                                    //Logger.Info("Execute[" + Index + "," + times + "]");
                                    ResourceHelper.Log("ExecutionHint", Index + "," + times + "]");

                                    //Console.ForegroundColor = fgBak;
                                    //Console.BackgroundColor = bgBak;
                                    Console.WriteLine();
                                    //Console.BackgroundColor = bgBak;
                                    Execute(ref IODefinition, ref MatrixDefinition, ref RulePoolDefinition);
                                    break;

                                case RIBAction.ToShiftNext:
                                    //Index++ included in loop definition;
                                    continue;

                                case RIBAction.OK:
                                    //Console.WriteLine("RI_TRUE: OK");
                                    RIBinder.HackTime();
                                    break;
                            }
                            break;
                    }
                }
                while (RIBinder.TryAnotherTime() == true);

                // STEP 3
                //Calculator.Calculate(AnswerList.Parameters);
                //bool result = bool.Parse(Calculator.Result);

                // Fetch answer from answerlist
                //bool[] results = AnswerList.Parameters[Index].Value;
                List<bool> results = new List<bool>(AnswerLists.Count);
                for (int m = 0; m < AnswerLists.Count; m++)
                {
                    // Note. here AnswerLists.Count means how many VCIO Output cells are there.
                    results.Add(AnswerLists[m].Parameters[Index].Value);
                }

                // STEP 4
                for (int resi = 0; resi < AnswerLists.Count; resi++)
                {
                    // Now checking the RESIth VCIO output cell's answer...
                    // More work to do Feb.3.2021:
                    /*
                     * The code below can only work well in the following conditions:
                     * 
                     * EnvParser can read AnswerLists correctly... including 'For' attributes, of AnsLst, PLst collections...
                     */

                    if (IODefinition.Outputs[AnswerLists[resi].For].Value == results[resi])
                    {
                        // Match!
                        //Logger.Info($"For Index = {Index}, success = true, output = {(IODefinition.Outputs.First().Value.Value ? 1 : 0)}, ans = {(result ? 1 : 0)}");
                        ResourceHelper.Log(IOSupport.VxLogLevel.Verbose,"TestSuccessHint", Index.ToString());
                        currentRanking++;   // Improve ranking
                                            //Logger.Info("+1 " + currentRanking);
                        ResourceHelper.Log("RankingIncHint", currentRanking.ToString());
                    }
                    else
                    {
                        // Not matches ;'(
                        //Logger.Info($"For Index = {Index}, success = false, output = {(IODefinition.Outputs.First().Value.Value ? 1 : 0)}, ans = {(result ? 1 : 0)}");
                        ResourceHelper.Log(IOSupport.VxLogLevel.Verbose, "TestFailureHint", Index.ToString());
                        success = false;
                        currentRanking--;
                        //Logger.Info("-1 " + currentRanking);
                        ResourceHelper.Log("RankingDecHint", currentRanking.ToString());
                    }
                    //RankingHistory.Add(currentRanking);
                    //Logger.Info("CR= " + currentRanking);
                }
                ResourceHelper.Log("CurrentRankingHint", currentRanking.ToString());

                //RulePoolDefinition.ruleHistory.Add(RulePoolDefinition.GetLatest());
                //Console.WriteLine("Rule registered to rule history.");
            }

            RankingHistory.Add(currentRanking);
            RulePoolDefinition.ruleHistory.Add(RulePoolDefinition.GetLatest());

            // Evaluate() done. Returns success or not to caller.
            return success;
        }
    }

    struct TECParam // TEC: TaskExecuteCell
    {
        public uint t;
        public List<Cell> CellsBackup;
        public Matrix MatrixDefinition;
        public IO IODefinition;
        public bool[] rule;
        public uint x, y;
    }
}

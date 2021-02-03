using System;
using System.Collections.Generic;
//using NLog;

namespace Vertex
{
    class Matrix
    {
        //private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // The Cellular Matrix should be designed here...
        public List<Cell> Cells;

        public bool ShowRealTimeMatrix = false;
        public bool UseMultiThreading = true;

        public readonly uint Columns;
        public readonly uint Rows;

        public string rawSize = "0x0";

        public Matrix(uint Columns, uint Rows)
        {
            if(Columns == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Columns), "Columns cannot be zero");
            }
            if(Rows == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Rows), "Rows cannot be zero");
            }

            this.Columns = Columns;
            this.Rows = Rows;

            Cells = new List<Cell>(Convert.ToInt32((this.Columns * this.Rows)));
            //        Console.WriteLine("[Debug] env.Matrix.Cells.Count = " + Cells.Count)

            // Initialize the cell list in order to get a count = capacity.
            for (int index = 0; index < Cells.Capacity; index++)
            {
                Cells.Add(new Cell());
            }
        }

        public void ResetCells()
        {
            for (int i = 0; i < Cells.Count; i++)
            {
                Cells[i].Value = false;
            }
            //Logger.Info("Automata status reseted successfully.");
            ResourceHelper.Log("CCAResetedInfo");
        }

        public uint? FindCellIndexByLocation(int LocationX, int LocationY)
        {
            // Algorithm:
            // Loc(0,0) -> C[0]
            // Loc(LocationX, LocationY) -> C[LocationY*Columns + LocationX]

            // What should be paid attention to:
            // - Columns:   a new readonly private var which is set during construction function,
            //              its type is uint while it is >0. LocationX + 1 cannot exceed this.
            // - Rows:      LocationY + 1 cannnot exceed this.

            // For example, if a CM has the structure below,
            /*
             * -|- - - Columns - - -|
             * | 0 0 1 1 0 1 0 1 1 0    The correct value for Columns is 10;
             * R 0 0 1 0 0 0 0 0 1 1    The correct value for Rows is 6;
             * o 1 1 0 1 1 0 1 0 1 0    The max loc(x,y) is (9,5);
             * w 1 0 1 0 0 1 0 1 0 1    The max C[index] is 59.
             * s 0 0 1 0 1 0 1 0 0 1
             * | 1 1 1 0 0 0 1 0 1 1
             * -
            */

            // TODO: Add <0 logic
            if ((LocationX < 0) || (LocationY < 0))
            {
                return null;
            }

            if(LocationX + 1 > Columns)
            {
                // exceeds Columns
                //throw new ArgumentOutOfRangeException("LocationX");                
                return null;
            }
            if(LocationY + 1 > Rows)
            {
                // exceeds Rows
                //throw new ArgumentOutOfRangeException("LocationY");
                return null;
            }

            uint index = (uint)((uint)(LocationY * Columns) + LocationX);
            return index;
        }
    }
}

﻿//using System;
using System.Collections.Generic;
//using System.Collections;
using System.Linq;
//using NLog;

namespace Vertex
{
    class IO
    {
        //private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // IO Definition
        public Dictionary<string, VCIOCell> Inputs = new Dictionary<string, VCIOCell>();
        public Dictionary<string, VCIOCell> Outputs = new Dictionary<string, VCIOCell>();
        //public Dictionary<string, VCIOCell> ReadyIndicators = new Dictionary<string, VCIOCell>();
        public (string name, VCIOCell content) ReadyIndicator;

        public IO()
        {
            ReadyIndicator.name = "";
            ReadyIndicator.content = new VCIOCell();
        }

        public void UpdateCellsFromInput(ref List<Cell> Cells)   // Sync from VD[Input] to Cells
        {
            //Logger.Info("Syncing Cells data... ");
            ResourceHelper.Log("CellsSyncInfo");
            // Will only sync Inputs list
            for (int i = 0; i < Inputs.Count; i++)
            {
                Cells[(int)Inputs.Values.ToArray()[i].ID].Value = Inputs.Values.ToArray()[i].Value;
            }
        }

        //public int GetVCIOCount() => Inputs.Count + Outputs.Count + ReadyIndicators.Count;
        public int GetVCIOCount() => Inputs.Count + Outputs.Count + 1;

        public void Sync(in List<Cell> Cells)  // Sync from Cells to VCIO dicts
        {
            #region
            //foreach (KeyValuePair<string, VCIOCell> vcio in Inputs)
            //{
            //    vcio.Value.Value = Cells[(int)vcio.Value.ID].Value;
            //}
            //foreach (KeyValuePair<string, VCIOCell> vcio in Outputs)
            //{
            //    vcio.Value.Value = Cells[(int)vcio.Value.ID].Value;
            //}
            //foreach (KeyValuePair<string, VCIOCell> vcio in ReadyIndicators)
            //{
            //    vcio.Value.Value = Cells[(int)vcio.Value.ID].Value;
            //}
            #endregion

            //Console.Write("Syncing IO data... 0%... ");
            for (int i = 0; i < Inputs.Count; i++)
            {
                if (Inputs.Values.ToArray()[i].Execute == true)
                {
                    Inputs.Values.ToArray()[i].Value = Cells[(int)Inputs.Values.ToArray()[i].ID].Value;
                }
            }
            //Console.Write("33%... ");
            for (int i = 0; i < Outputs.Count; i++)
            {
                if (Outputs.Values.ToArray()[i].Execute == true)
                {
                    Outputs.Values.ToArray()[i].Value = Cells[(int)Outputs.Values.ToArray()[i].ID].Value;
                }
            }
            //Console.Write("66%... ");
            //for (int i = 0; i < ReadyIndicators.Count; i++)
            //{
            //    if (ReadyIndicators.Values.ToArray()[i].Execute == true)
            //    {
            //        ReadyIndicators.Values.ToArray()[i].Value = Cells[(int)ReadyIndicators.Values.ToArray()[i].ID].Value;
            //    }
            //}

            if (ReadyIndicator.content.Execute == true)
            {
                ReadyIndicator.content.Value = Cells[(int)ReadyIndicator.content.ID].Value;
            }

            //Console.WriteLine("100%");
        }
    }

    class VCIOCell : Cell
    {
        public VCIOType Type;
        public uint ID;   // ID is the location (index) for the VCIO Cell in the list of Cells defined in the Matrix.
        public string Name; // Name for the VCIOCell, used by Evaluator to validate.
        public bool Execute;    // If yes, execute this VCIO cell.
    }

    enum VCIOType
    {
        Input, Output, ReadyIndicator
    }
}

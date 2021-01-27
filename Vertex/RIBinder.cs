//using System;
//using System.Collections.Generic;
//using System.Text;

namespace Vertex
{
    class RIBinder
    {
        public string Indicator;    // name of the VCIO cell
        public uint Timeout;    // executions limit for timeouting (rule->terminate)
        public RIBAction On;
        public RIBAction Off;

        private uint times = 0;
        private bool hackTime = false;

        public bool TryAnotherTime()
        {
            if (hackTime == true)
            {
                hackTime = false;
                return false;
            }

            times++;
            if(times >= Timeout)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void HackTime()
        {
            hackTime = true;
        }

        public uint GetTimes() => times;

        public void ResetTimes()
        {
            times = 0;
        }
    }

    enum RIBAction
    {
        ToShiftNext,    // Switch parameter pointer to next
        Break,  // Terminate the automata immediately
        Continue,   // Let the automata continue until timeouts
        OK  // Means the automata is ready now
    }
}

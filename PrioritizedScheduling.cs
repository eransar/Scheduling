using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Scheduling
{
    class PrioritizedScheduling : SchedulingPolicy
    {
        private int quantom;
        HashSet<int> pids = new HashSet<int>();
        public PrioritizedScheduling(int iQuantum)
        {
            this.quantom = iQuantum;
        }

        public override int NextProcess(Dictionary<int, ProcessTableEntry> dProcessTable)
        {
            int max = 0;
            int maxid = -5;
            foreach (ProcessTableEntry p in dProcessTable.Values)
            {
                if (!p.Blocked && !p.Done && p.Priority > max)
                {
                    max = p.Priority;
                    maxid = p.ProcessId;
                    
                }


                    dProcessTable[maxid].Quantum = quantom;
                    return maxid;
                }
               
          

            return -1;
        }

        public override void AddProcess(int iProcessId)
        {
            pids.Add(iProcessId);
        }

        public override bool RescheduleAfterInterrupt()
        {
            return true;
        }
    }
}

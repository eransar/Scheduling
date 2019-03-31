using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scheduling
{
    class RoundRobin : SchedulingPolicy
    {
        private int quantom;
        private Queue<int> rr_q = new Queue<int>();
        public RoundRobin(int iQuantum)
        {
            this.quantom = iQuantum;
        }

        public override int NextProcess(Dictionary<int, ProcessTableEntry> dProcessTable)
        {
            if (rr_q.Count == 0) // nothing in queue
            {
                return -1;
            }
            else
            {
                int processid = rr_q.Dequeue();
                if (dProcessTable.ContainsKey(processid))
                {
                    dProcessTable[processid].Quantum = quantom;
                    return processid;  
                }
                
            }

            return -1;

        }
        

        public override void AddProcess(int iProcessId)
        {
          rr_q.Enqueue(iProcessId);
        }

        public override bool RescheduleAfterInterrupt()
        {
            return true;
        }

      
    }
}

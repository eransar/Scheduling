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
            //set all quantoms to given quantom 
            if (rr_q.Count > 0)
            {
                int process_id = rr_q.Dequeue();
                dProcessTable[process_id].Quantum = quantom;
                return process_id;
            }

            else
            {
                dProcessTable[0].Quantum = -1;
                
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

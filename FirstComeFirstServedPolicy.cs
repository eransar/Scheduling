using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace Scheduling
{
    class FirstComeFirstServedPolicy : SchedulingPolicy
    {
        private Queue<int> q = new Queue<int>();
        public override int NextProcess(Dictionary<int, ProcessTableEntry> dProcessTable)
        {
            foreach (ProcessTableEntry p in dProcessTable.Values)
            {
                if (!p.Done && !p.Blocked && p.ProcessId !=0)
                {
                    return p.ProcessId;
                }
            }

            if (dProcessTable.Count > 1)
            {
                return 0;
            }

            return -1;
        }


        public override void AddProcess(int iProcessId)
        {
         q.Enqueue(iProcessId);   
        }

        public override bool RescheduleAfterInterrupt()
        {
            return false;
        }
    }
}

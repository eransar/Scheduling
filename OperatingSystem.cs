using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scheduling
{
    class OperatingSystem
    {
        public Disk Disk { get; private set; }
        public CPU CPU { get; private set; }
        private Dictionary<int, ProcessTableEntry> m_dProcessTable;
        private List<ReadTokenRequest> m_lReadRequests;
        private int m_cProcesses;
        private SchedulingPolicy m_spPolicy;
        private static int IDLE_PROCESS_ID = 0;

        public OperatingSystem(CPU cpu, Disk disk, SchedulingPolicy sp)
        {
            CPU = cpu;
            Disk = disk;
            m_dProcessTable = new Dictionary<int, ProcessTableEntry>();
            m_lReadRequests = new List<ReadTokenRequest>();
            cpu.OperatingSystem = this;
            disk.OperatingSystem = this;
            m_spPolicy = sp;
            IdleCreator();

            //create an "idle" process here
        }
        
        private void IdleCreator()
        {
            bool existsIdle = false;

                IdleCode idle = new IdleCode();
                m_dProcessTable[m_cProcesses] = new ProcessTableEntry(m_cProcesses, "idle", idle);
                m_dProcessTable[m_cProcesses].StartTime = CPU.TickCount;
                m_spPolicy.AddProcess(m_cProcesses);
                newProcessProcedure(m_cProcesses);
                m_cProcesses++;
               
            
            
        }

        private void newProcessProcedure(int id)
        {
            CPU.ActiveConsole = m_dProcessTable[id].Console;
            CPU.ProgramCounter = m_dProcessTable[id].ProgramCounter;
            CPU.ActiveAddressSpace = m_dProcessTable[id].AddressSpace;
            CPU.ActiveProcess = id;
        }


        public void CreateProcess(string sCodeFileName)
        {
            Code code = new Code(sCodeFileName);
            m_dProcessTable[m_cProcesses] = new ProcessTableEntry(m_cProcesses, sCodeFileName, code);
            m_dProcessTable[m_cProcesses].StartTime = CPU.TickCount;
            m_spPolicy.AddProcess(m_cProcesses);
            m_cProcesses++;
        }
        public void CreateProcess(string sCodeFileName, int iPriority)
        {
            Code code = new Code(sCodeFileName);
            m_dProcessTable[m_cProcesses] = new ProcessTableEntry(m_cProcesses, sCodeFileName, code);
            m_dProcessTable[m_cProcesses].Priority = iPriority;
            m_dProcessTable[m_cProcesses].StartTime = CPU.TickCount;
            m_spPolicy.AddProcess(m_cProcesses);
            m_cProcesses++;
        }

        public void ProcessTerminated(Exception e)
        {
            if (e != null)
                Console.WriteLine("Process " + CPU.ActiveProcess + " terminated unexpectedly. " + e);
            m_dProcessTable[CPU.ActiveProcess].Done = true;
            m_dProcessTable[CPU.ActiveProcess].Console.Close();
            m_dProcessTable[CPU.ActiveProcess].EndTime = CPU.TickCount;
            ActivateScheduler();
        }

        public void TimeoutReached()
        {
            ActivateScheduler();
        }

        public void ReadToken(string sFileName, int iTokenNumber, int iProcessId, string sParameterName)
        {
            ReadTokenRequest request = new ReadTokenRequest();
            request.ProcessId = iProcessId;
            request.TokenNumber = iTokenNumber;
            request.TargetVariable = sParameterName;
            request.Token = null;
            request.FileName = sFileName;
            m_dProcessTable[iProcessId].Blocked = true;
            if (Disk.ActiveRequest == null)
                Disk.ActiveRequest = request;
            else
                m_lReadRequests.Add(request);
            CPU.ProgramCounter = CPU.ProgramCounter + 1;
            ActivateScheduler();
        }

        public void Interrupt(ReadTokenRequest rFinishedRequest)
        {
            //implement an "end read request" interrupt handler.
            //translate the returned token into a value (double). 
            //when the token is null, EOF has been reached.
            //write the value to the appropriate address space of the calling process.
            //activate the next request in queue on the disk.
            double result;
            if (rFinishedRequest.Token == null)
            {
                result = double.NaN;
            }
            else
            {
                result = double.Parse(rFinishedRequest.Token);
                
            }

            if (rFinishedRequest == null)
            {
                m_dProcessTable[rFinishedRequest.ProcessId].AddressSpace[rFinishedRequest.TargetVariable] = double.NaN;
            }
            else
            {
                m_dProcessTable[rFinishedRequest.ProcessId].AddressSpace[rFinishedRequest.TargetVariable] = result; // data from buffer into target
                m_dProcessTable[rFinishedRequest.ProcessId].Blocked = false;    
            }
            
            

            if (this.m_lReadRequests.Count > 0)
            {
                ReadTokenRequest firstRequest= m_lReadRequests.ElementAt(0);
                Disk.ActiveRequest = firstRequest;
                m_lReadRequests.RemoveAt(0);
                
            }
           
            //implement an "end read request" interrupt handler.
            //translate the returned token into a value (double). 
            //when the token is null, EOF has been reached.
            //write the value to the appropriate address space of the calling process.
            //activate the next request in queue on the disk.

            if (m_spPolicy.RescheduleAfterInterrupt())
                ActivateScheduler();
        }

        private ProcessTableEntry ContextSwitch(int iEnteringProcessId)
        {
            //your code here
            //implement a context switch, switching between the currently active process on the CPU to the process with pid iEnteringProcessId
            //returns the process table information of the outgoing process
            ProcessTableEntry newprocess = m_dProcessTable[iEnteringProcessId];
            if (CPU.ActiveProcess != -1)
            {
               
                //there is proccess using the cpu
                ProcessTableEntry oldprocess = m_dProcessTable[CPU.ActiveProcess];
                oldprocess.AddressSpace = CPU.ActiveAddressSpace;
                oldprocess.ProgramCounter = CPU.ProgramCounter;
                oldprocess.Console = CPU.ActiveConsole;
                oldprocess.LastCPUTime = CPU.TickCount;
                CPU.RemainingTime = newprocess.Quantum;
                CPU.ActiveProcess = newprocess.ProcessId;
                CPU.ActiveConsole = newprocess.Console;
                CPU.ActiveAddressSpace = newprocess.AddressSpace;
                CPU.ProgramCounter = newprocess.ProgramCounter;

                return oldprocess;

            }
            else
            {
                CPU.ActiveProcess = newprocess.ProcessId;
                CPU.ActiveConsole = newprocess.Console;
                CPU.ActiveAddressSpace = newprocess.AddressSpace;
                CPU.ProgramCounter = newprocess.ProgramCounter;
               
                return null;
            }


        }

        public void ActivateScheduler()
        {
            int iNextProcessId = m_spPolicy.NextProcess(m_dProcessTable);
            if (iNextProcessId == -1)
            {
                Console.WriteLine("All processes terminated or blocked.");
                CPU.Done = true;
            }
            else
            {
                bool bOnlyIdleRemains = false;
                if (iNextProcessId == IDLE_PROCESS_ID)
                {
                    bOnlyIdleRemains = true;
                    foreach (ProcessTableEntry e in m_dProcessTable.Values)
                    {
                        if (e.Name != "idle" && e.Done != true)
                        {
                            bOnlyIdleRemains = false;
                        }
                    }
                }
                if(bOnlyIdleRemains)
                {
                    Console.WriteLine("Only idle remains.");
                    CPU.Done = true;
                }
                else
                    ContextSwitch(iNextProcessId);
            }
        }

        public double AverageTurnaround()
        {
            //Compute the average time from the moment that a process enters the system until it terminates.
            throw new NotImplementedException();
        }
        public int MaximalStarvation()
        {
            //Compute the maximal time that some project has waited in a ready stage without receiving CPU time.
            throw new NotImplementedException();
        }
    }
}

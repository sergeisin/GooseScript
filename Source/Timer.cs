using System.Runtime.InteropServices;

namespace GooseScript
{
    public static class Timer
    {
        [DllImport("ntdll.dll")]
        private static extern int NtQueryTimerResolution(ref uint MinimumResolution, ref uint MaximumResolution, ref uint CurrentResolution);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);

        [DllImport("ntdll.dll")]
        private static extern int NtDelayExecution(bool Alertable, ref long DelayInterval);

        static Timer()
        {
            uint min = 0;
            uint max = 0;
            uint cur = 0;

            NtQueryTimerResolution(ref min, ref max, ref cur);

            if (cur != max)
            {
               NtSetTimerResolution(max, true, ref cur);
            }
        }

        public static void Sleep(int milliseconds)
        {
            if (milliseconds <= 0)
                milliseconds = 1;

            long interval = milliseconds * -10_000L;

            NtDelayExecution(false, ref interval);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRIE.Etc
{
    internal class Diagnostics
    {
        public static void MeasureExecutionTime(string MethodName, Action callback, Output Output)
        {
            Stopwatch stopwatch = new Stopwatch();

            // Start measuring time
            stopwatch.Start();

            // Execute the callback function
            callback();

            // Stop measuring time
            stopwatch.Stop();

            // Output the elapsed time
            Output.WriteLine($"{MethodName} Execution time: {stopwatch.ElapsedMilliseconds} milliseconds");
        }
    }
}

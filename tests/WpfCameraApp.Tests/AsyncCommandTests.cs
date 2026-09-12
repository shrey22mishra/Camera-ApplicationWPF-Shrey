using System.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using WpfCameraApp.Commands;

namespace WpfCameraApp.Tests
{
    public class AsyncCommandTests
    {
        public async Task AsyncCommand_ExecuteAsync_Runs()
        {
            var executed = false;
            var cmd = new AsyncCommand(async () =>
            {
                await Task.Yield();
                executed = true;
            });

            SimpleAssert.IsTrue(cmd.CanExecute(null));
            await cmd.ExecuteAsync();
            SimpleAssert.IsTrue(executed);
        }

        public async Task AsyncCommand_Prevents_Reentry()
        {
            var tcs = new TaskCompletionSource<bool>();
            var started = 0;
            var cmd = new AsyncCommand(async () =>
            {
                Interlocked.Increment(ref started);
                await tcs.Task; // wait until test signals
            });

            // start the command but don't await completion yet
            var run = cmd.ExecuteAsync();

            // while running, CanExecute should be false
            SimpleAssert.IsFalse(cmd.CanExecute(null));

            // Attempt to start again; it should return immediately (no extra start)
            var run2 = cmd.ExecuteAsync();

            // Allow the command to finish
            tcs.SetResult(true);
            await Task.WhenAll(run, run2);

            SimpleAssert.AreEqual(1, started);
        }
    }
}

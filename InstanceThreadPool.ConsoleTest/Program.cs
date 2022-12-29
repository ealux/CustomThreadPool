namespace InstanceThreadPool.ConsoleTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Stuff to work with
            var messages = Enumerable.Range(1, 1000).Select(i => $"Message-{i}");

            // Create custom pool (disposable)
            using var pool = new InstanceThreadPool(25, ThreadPriority.Normal, "MyThreadPool");
            {
                foreach (var message in messages)
                    // Create job
                    pool.Run(message, p =>
                    {
                        var msg = (string)p!;   // Take message
                        Console.WriteLine($">> Job with message {msg} started...");   // Inform start
                        Thread.Sleep(5000);  // Wait a bit
                        Console.WriteLine($">> Job with message {msg} completed!");   // Inform complete
                    });

                Console.ReadLine();
            }

            Console.ReadLine();
        }
    }
}
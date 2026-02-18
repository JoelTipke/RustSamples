using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

class ConsoleEcho
{
    const string Address = "127.0.0.1";
    const int Port = 7878;

    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--receive")
            ReceiveMode();
        else
            SendMode();
    }

    static void ReceiveMode()
    {
        var listener = new TcpListener(IPAddress.Parse(Address), Port);
        listener.Start();
        Console.WriteLine("=== Shared Console (Mirror) ===\n");

        using var client = listener.AcceptTcpClient();
        listener.Stop();
        using var reader = new StreamReader(client.GetStream());

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            Console.WriteLine(line);
        }

        Console.WriteLine("\nConnection closed. Press Enter to exit.");
        Console.ReadLine();
    }

    static void SendMode()
    {
        var exe = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule!.FileName!;

        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd",
            Arguments = $"/c start \"\" \"{exe}\" --receive",
            UseShellExecute = false
        });

        Thread.Sleep(500);

        using var client = new TcpClient(Address, Port);
        using var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };

        Console.WriteLine("=== Shared Console (Main) ===");
        Console.WriteLine("Enter commands to run. Output appears on both screens.");
        Console.WriteLine("Type 'exit' to quit.\n");

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
                continue;

            if (input == "exit")
                break;

            // Show the command on both screens
            Console.WriteLine($"$ {input}");
            writer.WriteLine($"$ {input}");

            // Run the command and capture output
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c {input}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                })!;

                var stdout = process.StandardOutput.ReadToEnd();
                var stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(stdout))
                {
                    Console.Write(stdout);
                    foreach (var line in stdout.TrimEnd().Split('\n'))
                        writer.WriteLine(line.TrimEnd('\r'));
                }

                if (!string.IsNullOrEmpty(stderr))
                {
                    Console.Error.Write(stderr);
                    foreach (var line in stderr.TrimEnd().Split('\n'))
                        writer.WriteLine($"[ERR] {line.TrimEnd('\r')}");
                }
            }
            catch (Exception e)
            {
                var msg = $"Failed to run command: {e.Message}";
                Console.Error.WriteLine(msg);
                writer.WriteLine(msg);
            }

            // Blank line between commands on both screens
            Console.WriteLine();
            writer.WriteLine();
        }
    }
}

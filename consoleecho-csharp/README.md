# ConsoleEcho (C#)

A C# port of the ConsoleEcho tool. Mirrors console output to a second terminal window over a local TCP connection. Run commands in the main console and see the output echoed in real time on a separate window.

## How It Works

The program operates in two modes connected via a local TCP socket (`127.0.0.1:7878`):

- **Send mode** (default) — Opens a receiver window, then accepts commands from the user. Output is displayed locally and streamed to the receiver.
- **Receive mode** (`--receive`) — Listens for incoming output and displays it. Launched automatically by send mode.

## Usage

```sh
# Run with .NET CLI
dotnet run

# Or build and run the exe
dotnet build
./bin/Debug/net8.0/ConsoleEcho.exe
```

Type commands at the `>` prompt. Output appears in both windows. Type `exit` to quit.

## Requirements

- .NET 8.0 SDK

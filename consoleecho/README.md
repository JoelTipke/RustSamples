# ConsoleEcho

A Rust command-line tool that mirrors console output to a second terminal window. Run commands in the main console and see the output echoed in real time on a separate window.

## How It Works

The program operates in two modes connected via a local TCP socket (`127.0.0.1:7878`):

- **Send mode** (default) — Opens a receiver window, then accepts commands from the user. Output is displayed locally and streamed to the receiver.
- **Receive mode** (`--receive`) — Listens for incoming output and displays it. Launched automatically by send mode.

## Usage

```sh
# Compile
rustc hello.rs

# Run (opens a second console window automatically)
./hello
```

Type commands at the `>` prompt. Output appears in both windows. Type `exit` to quit.

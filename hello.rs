use std::env;
use std::io::{self, BufRead, BufReader, Write};
use std::net::{TcpListener, TcpStream};
use std::process::Command;

const ADDR: &str = "127.0.0.1:7878";

fn main() {
    let args: Vec<String> = env::args().collect();

    if args.len() > 1 && args[1] == "--receive" {
        receive_mode();
    } else {
        send_mode();
    }
}

fn receive_mode() {
    let listener = TcpListener::bind(ADDR).expect("Failed to bind");
    println!("=== Shared Console (Mirror) ===\n");

    let (stream, _) = listener.accept().expect("Failed to accept");
    let reader = BufReader::new(stream);

    for line in reader.lines() {
        match line {
            Ok(text) => println!("{}", text),
            Err(_) => break,
        }
    }

    println!("\nConnection closed. Press Enter to exit.");
    let _ = io::stdin().read_line(&mut String::new());
}

fn send_mode() {
    let exe = env::current_exe().expect("Failed to get exe path");

    Command::new("cmd")
        .args(["/c", "start", "", &exe.to_string_lossy(), "--receive"])
        .spawn()
        .expect("Failed to spawn receiver console");

    std::thread::sleep(std::time::Duration::from_millis(500));

    let mut stream = TcpStream::connect(ADDR).expect("Failed to connect to receiver");

    println!("=== Shared Console (Main) ===");
    println!("Enter commands to run. Output appears on both screens.");
    println!("Type 'exit' to quit.\n");

    let stdin = io::stdin();
    loop {
        print!("> ");
        io::stdout().flush().ok();

        let mut input = String::new();
        if stdin.lock().read_line(&mut input).is_err() || input.trim().is_empty() {
            continue;
        }

        let input = input.trim();
        if input == "exit" {
            break;
        }

        // Show the command on both screens
        println!("$ {}", input);
        let _ = writeln!(stream, "$ {}", input);

        // Run the command and capture output
        let output = Command::new("cmd")
            .args(["/c", &input])
            .output();

        match output {
            Ok(result) => {
                let stdout = String::from_utf8_lossy(&result.stdout);
                let stderr = String::from_utf8_lossy(&result.stderr);

                if !stdout.is_empty() {
                    print!("{}", stdout);
                    for line in stdout.lines() {
                        let _ = writeln!(stream, "{}", line);
                    }
                }
                if !stderr.is_empty() {
                    eprint!("{}", stderr);
                    for line in stderr.lines() {
                        let _ = writeln!(stream, "[ERR] {}", line);
                    }
                }
            }
            Err(e) => {
                let msg = format!("Failed to run command: {}", e);
                eprintln!("{}", msg);
                let _ = writeln!(stream, "{}", msg);
            }
        }

        // Blank line between commands on both screens
        println!();
        let _ = writeln!(stream);
    }
}

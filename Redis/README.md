# Redis Implementation in Go

This is a lightweight Redis implementation written in Go. It supports basic Redis commands and uses an Append-Only File (AOF) for data persistence.

## Features

- TCP server listening on the default Redis port (6379)
- Support for basic Redis commands like GET, SET, etc.
- Data persistence through Append-Only File (AOF)
- RESP (Redis Serialization Protocol) implementation

## Prerequisites

- Go 1.13 or higher

## Getting Started

### Running the Server

1. Clone this repository:
   ```
   git clone <repository-url>
   cd Redis
   ```

2. Build and run the server:
   ```
   go build
   ./Redis
   ```

   On Windows:
   ```
   go build
   .\Redis.exe
   ```

The server will start listening on port 6379.

### Connecting to the Server

You can connect to the server using the `redis-cli` tool or any Redis client:

```
redis-cli -p 6379
```

Or you can use a TCP client like telnet:

```
telnet localhost 6379
```

## Supported Commands

The server supports the following Redis commands:
- SET - Set a key to a value
- GET - Get the value of a key
- DEL - Delete a key
- HSET - Set a field in a hash stored at key to value
- HGET - Get the value of a field in a hash
- (Additional commands as implemented in the handler.go file)

## Project Structure

- `main.go` - Server initialization and main loop
- `resp.go` - RESP (Redis Serialization Protocol) implementation
- `handler.go` - Command handlers
- `aof.go` - Append-Only File implementation for persistence

## Data Persistence

The server uses an Append-Only File (AOF) for data persistence. All write operations (SET, HSET, etc.) are appended to the database.aof file. When the server starts, it reads this file to restore the previous state.

## License

[Specify your license here]

## Contributing

[Specify contribution guidelines]

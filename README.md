# GGGG

A Go-playing AI using Monte Carlo Tree Search, written in C#. Communicates via the Go Text Protocol (GTP), compatible with any GTP-supporting UI (e.g., SmartGo).

## Architecture

```
GTP GUI → GGGGGoText.exe → GGGG.dll (core) → GGGG.Interface.dll (abstractions)
                                         ↓
                              GGGG.NeuralEstimator.dll (optional, neural winrates)
```

### Projects

| Project | Type | Purpose |
|---------|------|---------|
| **GGGGGoText** | Console app | GTP engine executable. Engine name: "Matty Go" |
| **GGGG** | Class library | Core MCTS/UCT search, board representations, move generation, caching |
| **GGGG.Interface** | Class library | Shared interfaces (`IFastBoard`, `IGoEngine`) and data types |
| **GGGG.NeuralEstimator** | Class library | Neural network winrate estimation via Encog |
| **GGGG.ExperimentalBoard** | Class library | Experimental board using Yeppp! SIMD math library |
| **GGGG.Accelerators** | C++/CLI DLL | Native code acceleration (placeholder) |
| **PatternCreator** | Console app | Extracts 5x5 board patterns from SGF game records |
| **UnitTests** | Test library | MSTest unit/integration tests |

### Search Algorithm

The primary search is **UCT (Upper Confidence Bounds applied to Trees)** with:

- **RAVE / AMAF** — Rapid Action Value Estimation for move ordering
- **Parallel simulations** — `System.Threading.Tasks.Parallel.For`
- **Neural network guidance** — Encog-based winrate priors (optional, loaded from `.nn` files)
- **Caching** — LRU caches for Monte Carlo evaluations and board hashes
- **Dead stone detection** — Final position scoring

### Board Implementations

| Class | Description |
|-------|-------------|
| `StringBasedBoard` | Default. Chain-tracking with string pooling for memory efficiency |
| `FastBoard` | Optimized with `unsafe` code, mark-and-sweep capture, Zobrist hashing |
| `Board` | Original immutable reference implementation using `GoString` |

## Build

- **Visual Studio 2019+** with .NET desktop development workload
- .NET Framework 4.5 / 4.6 Developer Pack
- `GGGGGoText` is the startup project
- Unsafe blocks enabled for optimized board code
- NuGet package restore pulls protobuf-net and Combinatorics

### Command-Line Arguments

| Argument | Description |
|----------|-------------|
| `think` | Think during opponent's time (stronger, impolite) |
| `dontpass` | Disable early passing |
| `dontresign` | Disable early resignation |
| `movetime N` | Set time per move to N seconds |
| `selftest` | Run self-test in console mode |
| `train` | Load and train neural networks from accumulated data |

## Dependencies

| Library | Purpose |
|---------|---------|
| **log4net** | Logging |
| **MoreLinq** | Enhanced LINQ operators |
| **encog-core-cs** | Neural network training/ evaluation |
| **protobuf-net** | Training data serialization |
| **Combinatorics** | Move permutations (RAVE) |
| **Yeppp!** | SIMD vector operations (experimental) |

## Training

Self-play generates training data (`*.gtrain` files) in `%LOCALAPPDATA%\MattyGo\`. Neural networks are saved as `*.nn` files with naming convention `{COLOR}-{BOARDSIZE}-{KOMI}.nn`.

## License

MIT — see [LICENSE](LICENSE).

Copyright (c) 2021 MatthewRobertDunn
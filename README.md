# Stopwatch Application

A professional Windows Forms stopwatch application built with C# following Test-Driven Development principles.

## Features

- **Start**: Begins timing from 00:00:00
- **Pause**: Temporarily stops the timer while preserving elapsed time
- **Resume**: Continues timing from the paused point
- **Reset**: Returns the timer to 00:00:00
- **Stop**: Completely stops the timer and displays final time

## Technical Specifications

- **Time Format**: 00:00:00 (hours:minutes:seconds)
- **Update Frequency**: 100ms for smooth display
- **Framework**: .NET 7.0 Windows Forms
- **Testing**: comprehensive unit tests

## How to Run
 copy the github link : 
 then 
 dotnet run 
 

### Prerequisites
- .NET 7.0 SDK or later
- Windows operating system

### Running the Application
1. Open Command Prompt or PowerShell
2. Navigate to the project directory:
   ```
   cd c:\Users\user\stopwatch_GC1\Stopwatch
   ```
3. Build and run the application:
   ```
   dotnet run
   ```

### Running Tests
To execute the unit tests:
```
dotnet test
```

## Application Architecture

### Core Components

1. **StopwatchEngine**: Core timing logic with precise time tracking
2. **MainForm**: Windows Forms UI with intuitive button controls
3. **Program**: Application entry point

### Key Features Implementation

- **Precise Timing**: Uses System.Diagnostics.Stopwatch for accuracy
- **State Management**: Proper handling of running, paused, and stopped states
- **UI Updates**: Real-time display updates every 100ms
- **Button States**: Smart enabling/disabling based on current state

## User Interface

The application features a **stunning modern interface** with:
- **🎨 Dark gradient theme** with animated glow effects
- **⏱️ Large digital display** (36pt Segoe UI) with rounded panel
- **🌟 Animated border glow** that pulses when running
- **🎯 Modern rounded buttons** with gradient styling and emojis
- **✨ Smooth animations** and visual feedback
- **🖼️ Borderless design** with custom close button
- **🎭 Dynamic status indicators** with contextual colors and icons

## Code Quality

- **XML Documentation**: Complete documentation for all public methods
- **Test Coverage**: Comprehensive unit tests covering all functionality
- **Clean Architecture**: Separation of concerns between UI and business logic
- **Error Handling**: Robust state management preventing invalid operations
- **Modern UI**: Custom controls with gradient rendering and smooth animations
- **Visual Polish**: Professional design with attention to detail

## Button Functionality

| Button | Function | Enabled When |
|--------|----------|--------------|
| Start | Begins timing from 00:00:00 | Stopwatch is stopped |
| Pause | Temporarily stops timer | Stopwatch is running |
| Resume | Continues from paused time | Stopwatch is paused |
| Reset | Returns to 00:00:00 | Always available |
| Stop | Completely stops timer | Stopwatch is running or paused |

## Testing Approach

The project follows Test-Driven Development (TDD) with tests covering:
- Initial state verification
- Start/stop functionality
- Pause/resume operations
- Reset functionality
- Time format validation
- State transitions

## Project Structure

```
Stopwatch/
├── Program.cs              # Application entry point
├── MainForm.cs             # Windows Forms UI
├── StopwatchEngine.cs      # Core timing logic
├── StopwatchEngineTests.cs # Unit tests
├── Stopwatch.csproj        # Project configuration
└── README.md               # This documentation
```

## Development Notes

- Built with clean, maintainable code following C# best practices
- Comprehensive XML documentation for all public methods
- Responsive UI that updates smoothly during operation
- Proper resource management and disposal
- Thread-safe operations for UI updates

## Screenshot

<img width="2290" height="1792" alt="paused 2025-11-04 031752" src="https://github.com/user-attachments/assets/f2865262-fb14-4e84-b290-9faad48c5c7f" />
<img width="2512" height="1758" alt="stopped 2025-11-04 031709" src="https://github.com/user-attachments/assets/56e67e37-c366-4755-b21b-f3d5ec3b5a8e" />
<img width="2642" height="1826" alt="reset 2025-11-04 031626" src="https://github.com/user-attachments/assets/eee969e4-143e-4f4e-82d9-c9c87711646c" />
<img width="2866" height="1830" alt="stopwatch  2025-11-04 031536" src="https://github.com/user-attachments/assets/373626ee-0490-4ed5-8cba-0a62baece057" />
## Demo video 
(https://drive.google.com/file/d/19ZHsJV0qt17Ws0kFnqZNGv_6FZDaeciC/view?usp=sharing)



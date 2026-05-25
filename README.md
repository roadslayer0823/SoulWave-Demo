# SoulWave Demo

SoulWave Demo is a Unity-based application that leverages the ElevenLabs API to provide advanced Text-to-Speech (TTS) and Voice Cloning capabilities. The application supports various input methods, including custom text, pre-recorded audio files, live microphone recordings, and PDF documents. It also features a real-time audio visualizer for an immersive playback experience.

## Features

- **Text-to-Speech (TTS)**: Generate high-quality, natural-sounding speech from text using the ElevenLabs API.
- **Voice Cloning**: Clone voices by recording audio directly from your microphone or uploading existing audio files (supported formats: MP3, WAV, OGG, M4A).
- **PDF Text Extraction**: Extract text content from PDF documents and convert it directly into speech using the integrated `PdfPig` library.
- **Audio Visualization**: Integrated `RhythmVisualizatorPro` to visually react to generated and playback audio.
- **Voice Isolation & Filtering**: Automatically processes uploaded audio to isolate and filter voices before uploading them for cloning.
- **Native Device Integrations**: Utilizes native file pickers and sharing modules for a seamless user experience on mobile platforms.

## Prerequisites

- **Unity Editor** (Recommended version supporting standard C# 8.0+ features)
- **ElevenLabs API Key** (Required for TTS and Voice Cloning functionalities)
- **FFmpeg** (Optional: Required on macOS if you need to convert `.ogg` files to `.wav` for voice processing)

## Setup Instructions

1. **Open the Project**: Launch Unity Hub and open the `SoulWave Demo` project directory.
2. **Configure Environment Variables**:
   The project loads the ElevenLabs API key from an environment variable. Create a `.env` file in the root directory of the project (alongside the `Assets` folder) and add your API key:
   ```env
   ELEVEN_LABS_API_KEY=your_elevenlabs_api_key_here
   ```
3. **Verify Dependencies**:
   Ensure the following plugins/packages are present and properly configured in the project:
   - `UglyToad.PdfPig` (PDF Extraction)
   - `NativeFilePicker` & `NativeShare` (Mobile native integrations)
   - `RhythmVisualizatorPro` (Audio visualization)
4. **Play**: Open the main scene and press the Play button in the Unity Editor to test the application.

## Project Structure (Key Managers)

All core logic is neatly structured using a Manager-based architecture located under `Assets/Scripts/Manager/`:
- **`VoiceCloningManager.cs`**: The heart of the application. Handles ElevenLabs API integration, audio recording, TTS requests, PDF extraction, and file management.
- **`UIManager.cs`**: Controls UI transitions, screen states, and interactive elements.
- **`LoginManager.cs` & `SessionManager.cs`**: Manages user authentication and active user sessions.
- **`SoundEffectManager.cs`**: Controls UI feedback and ambient audio.
- **`SettingManager.cs`**: Handles user preferences and configuration.

## Additional Notes

- If testing `.ogg` audio uploads on macOS, the application attempts to use FFmpeg located at `/opt/homebrew/bin/ffmpeg`. Ensure FFmpeg is installed via Homebrew if you require this feature.
- File sizes for PDF extraction are bounded to prevent API limitations (currently truncated gracefully at 5000 characters).

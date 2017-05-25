using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace NoiseDetector
{
    public interface IAudioRecorder
    {
        void BeginMonitoring(int recordingDevice);
      
        void Stop();
        double MicrophoneLevel { get; set; }
        RecordingState RecordingState { get; }
        SampleAggregator SampleAggregator { get; }
        event EventHandler Stopped;
        WaveFormat RecordingFormat { get; set; }
        
    }
}

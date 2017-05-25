using System.Net.Http;
using System.Threading;
using NoiseDetector;
using System;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace NoiseDetectionApp
{
    class Program
    {
        private static Timer t;
        static string deviceID;
        static int noisecounts=0;
        static int interval = 5000;
        static HttpClient client = new HttpClient();
        private static int fastSamplingCount =0;

        static void Main(string[] args)
        {

            RunAsync().Wait();
        }

        static async Task RunAsync()
        {
            Console.WriteLine("Please provide the device ID you obtained from the NoiseDetection Bot");
            deviceID = Console.ReadLine();

            client.BaseAddress = new Uri("https://noisedetectionfunctions.azurewebsites.net/api/AddEventHttpTrigger?code=eCFnaCPKSLtLwFhuLNdEpchsuJXZGosUzdw0AqTCedBXBa3Nh5Iw3Q==");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //var status = client.GetAsync(client.BaseAddress + $"&deviceID={deviceID}&noiseLevel=noise").Result.Content.ReadAsStringAsync().Result;

            t = new Timer(new TimerCallback(timerEvent));
            t.Change(5000, Timeout.Infinite);

            //try
            //{

            //    var url = await SendNoiseNotification("7676");




            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //}
            Console.WriteLine("NoiseDetection is up and running. Press any key to exit.");
            Console.ReadLine();
        }

        static void timerEvent(object target)
        {
            string status;
            var volume = AudioRecorderContainer.GetVolume()*(-100);
            if (volume * 1000 > 3)//if it's noisy 
            {
                noisecounts++;
                fastSamplingCount++;
                interval = 1000; // increase sample rate
                if(noisecounts==1)
                Console.WriteLine("Detected noise. Increasing sample rate.");
                if (noisecounts > 5)
                {
                    status = client.GetAsync(client.BaseAddress + $"&deviceID={deviceID}&noiseLevel={volume*1000}").Result.Content.ReadAsStringAsync().Result;
                    noisecounts = 0;
                    interval = 5000; //back to initial sample rate
                    Console.WriteLine("Noise for more than 5 seconds. Sending notification.");
                    Console.WriteLine("Sample rate back to normal.");
                }
                if (fastSamplingCount > 20)
                {
                    fastSamplingCount = 0;
                    interval = 5000;
                    Console.WriteLine("Sound levels reduced. Sample rate back to normal.");
                }
            }

            t.Change(interval, Timeout.Infinite);

        }
        static async Task<Uri> SendNoiseNotification(string deviceId)
        {
            var stringContent = new StringContent("noise" + deviceId);
            HttpResponseMessage response = await client.PostAsync("posts/", stringContent);
            response.EnsureSuccessStatusCode();


            return response.Headers.Location;
        }
        public static class AudioRecorderContainer
        {
            private static AudioRecorder _recorder;
            public static AudioRecorder Recorder
            {
                get { if (_recorder == null) Init(); return _recorder; }
            }

            private static void Init()
            {
                _recorder = new AudioRecorder();
                _recorder.BeginMonitoring(0);
            }
            public static float GetVolume()
            {
                AudioRecorderContainer.Recorder.Stop();
                return AudioRecorderContainer.Recorder.SampleAggregator.Value;
            }
        }
    }


}

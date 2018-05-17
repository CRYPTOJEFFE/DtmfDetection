using DtmfDetection;

namespace DtmfDetector
{
	using System;

	using NAudio.CoreAudioApi;
	using NAudio.Wave;

	using DtmfDetection.NAudio;

	public static class Program
	{
		public static int Main(string[] args)
		{
			try
			{
				if (args.Length > 0)
					AnalyzeWaveFile(args[0]);
				else
					CaptureAndAnalyzeLiveAudio();

				return 0;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return 1;
			}
		}

		private static void AnalyzeWaveFile(string path)
		{
			using (var log = new Log("waveFile.log"))
			{
				PureTones.ClearTones();
				//PureTones.AddLowTone(0);
				//PureTones.AddHighTone(645);

				//PureTones.AddTonePair(794, 524); // Station 2
				//PureTones.AddTonePair(1084, 794); // Station 3
				//PureTones.AddTonePair(716, 645); // Station 6

				PureTones.TryAddHighTone(794);
				PureTones.TryAddHighTone(881);
				PureTones.TryAddHighTone(524);
				PureTones.TryAddHighTone(1084);
				//PureTones.AddHighTone(881);
				PureTones.TryAddHighTone(645);
				PureTones.TryAddHighTone(716);
				PureTones.TryAddHighTone(384);
				PureTones.TryAddHighTone(547);
				PureTones.TryAddHighTone(802);
				PureTones.TryAddHighTone(346);
				PureTones.TryAddHighTone(426);
				PureTones.TryAddHighTone(582);
				PureTones.TryAddHighTone(757);
				PureTones.TryAddHighTone(592);
				PureTones.TryAddHighTone(473);

				DtmfClassification.ClearAllTones();


				DtmfTone custom1 = new DtmfTone(794, 0, PhoneKey.Custom1);
				DtmfTone custom2 = new DtmfTone(881, 0, PhoneKey.Custom2);
				DtmfTone custom3 = new DtmfTone(524, 0, PhoneKey.Custom3);
				DtmfTone custom4 = new DtmfTone(1084, 0, PhoneKey.Custom4);
				//DtmfTone custom5 = new DtmfTone(881, 0, PhoneKey.Custom5);
				DtmfTone custom6 = new DtmfTone(645, 0, PhoneKey.Custom6);
				DtmfTone custom7 = new DtmfTone(716, 0, PhoneKey.Custom7);
				DtmfTone custom8 = new DtmfTone(384, 0, PhoneKey.Custom8);
				DtmfTone custom9 = new DtmfTone(547, 0, PhoneKey.Custom9);
				DtmfTone custom10 = new DtmfTone(802, 0, PhoneKey.Custom10);
				DtmfTone custom11 = new DtmfTone(346, 0, PhoneKey.Custom11);
				DtmfTone custom12 = new DtmfTone(426, 0, PhoneKey.Custom12);
				DtmfTone custom13 = new DtmfTone(582, 0, PhoneKey.Custom13);
				DtmfTone custom14 = new DtmfTone(757, 0, PhoneKey.Custom14);
				DtmfTone custom15 = new DtmfTone(592, 0, PhoneKey.Custom15);
				DtmfTone custom16 = new DtmfTone(473, 0, PhoneKey.Custom16);

				DtmfClassification.AddCustomTone(custom1);
				DtmfClassification.AddCustomTone(custom2);
				DtmfClassification.AddCustomTone(custom3);
				DtmfClassification.AddCustomTone(custom4);
				//DtmfClassification.AddCustomTone(custom5);
				DtmfClassification.AddCustomTone(custom6);
				DtmfClassification.AddCustomTone(custom7);
				DtmfClassification.AddCustomTone(custom8);
				DtmfClassification.AddCustomTone(custom9);
				DtmfClassification.AddCustomTone(custom10);
				DtmfClassification.AddCustomTone(custom11);
				DtmfClassification.AddCustomTone(custom12);
				DtmfClassification.AddCustomTone(custom13);
				DtmfClassification.AddCustomTone(custom14);
				DtmfClassification.AddCustomTone(custom15);
				DtmfClassification.AddCustomTone(custom16);


				using (var audioFile = new AudioFileReader(path))
				{
					foreach (var occurence in audioFile.DtmfTones())
					{
						if (occurence.Duration.TotalSeconds >= .5)
						{
							log.Add(
								$"{occurence.Position.TotalSeconds:00.000} s "
								+ $"({occurence.Channel}): "
								+ $"{occurence.DtmfTone.Key} key "
								+ $"(duration: {occurence.Duration.TotalSeconds:00.000} s)");
						}
					}
				}
			}
		}

		private static void CaptureAndAnalyzeLiveAudio()
		{
			using (var log = new Log("audioOut.log"))
			{
				LiveAudioDtmfAnalyzer analyzer = null;
				IWaveIn audioSource = null;

				while (true)
				{
					Console.WriteLine("====================================\n"
														+ "[O]   Capture current audio output\n"
														+ "[M]   Capture mic in\n"
														+ "[Esc] Stop capturing/quit");
					var key = Console.ReadKey(true);

					switch (key.Key)
					{
						case ConsoleKey.O:
							analyzer?.StopCapturing();
							audioSource?.Dispose();

							audioSource = new WasapiLoopbackCapture { ShareMode = AudioClientShareMode.Shared };
							analyzer = InitLiveAudioAnalyzer(log, audioSource);
							analyzer.StartCapturing();

							break;

						case ConsoleKey.M:
							analyzer?.StopCapturing();
							audioSource?.Dispose();

							audioSource = new WaveInEvent { WaveFormat = new WaveFormat(8000, 32, 1) };
							analyzer = InitLiveAudioAnalyzer(log, audioSource);
							analyzer.StartCapturing();

							break;

						case ConsoleKey.Escape:
							if (analyzer == null || !analyzer.IsCapturing)
								return;

							analyzer.StopCapturing();
							audioSource.Dispose();

							break;
					}
				}
			}
		}

		private static LiveAudioDtmfAnalyzer InitLiveAudioAnalyzer(Log log, IWaveIn waveIn)
		{
			var analyzer = new LiveAudioDtmfAnalyzer(waveIn);
			analyzer.DtmfToneStarted += start => log.Add($"{start.DtmfTone.Key} key started on {start.Position.TimeOfDay} (channel {start.Channel})");
			analyzer.DtmfToneStopped += end => log.Add($"{end.DtmfTone.Key} key stopped after {end.Duration.TotalSeconds} s (channel {end.Channel})");
			return analyzer;
		}
	}
}

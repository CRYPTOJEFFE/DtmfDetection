namespace DtmfDetection
{
	using System.Collections.Generic;
	using System.Linq;

	public class PureTones
	{
		private static readonly List<int> LowPureTones = new List<int> { 697, 770, 852, 941 };

		private static readonly List<int> HighPureTones = new List<int> { 1209, 1336, 1477, 1633 };

		private readonly Dictionary<int, AmplitudeEstimator> estimators;

		public PureTones(AmplitudeEstimatorFactory estimatorFactory)
		{
			estimators = LowPureTones
					.Concat(HighPureTones)
					.ToDictionary(tone => tone, estimatorFactory.CreateFor);
		}

		public double this[int tone] => estimators[tone].AmplitudeSquared;

		public void ResetAmplitudes()
		{
			foreach (var estimator in estimators.Values)
				estimator.Reset();
		}

		public static void ClearTones()
		{
			LowPureTones.Clear();
			HighPureTones.Clear();
		}

		public static void AddTonePair(int highTone, int lowTone)
		{
			LowPureTones.Add(lowTone);
			HighPureTones.Add(highTone);
		}

		public static bool TryAddLowTone(int lowTone)
		{
			if (LowPureTones.Contains(lowTone) || HighPureTones.Contains(lowTone))
				return false;

			LowPureTones.Add(lowTone);

			return true;
		}

		public static bool TryAddHighTone(int highTone)
		{
			if (LowPureTones.Contains(highTone) || HighPureTones.Contains(highTone))
				return false;

			HighPureTones.Add(highTone);

			return true;
		}

		public static bool OnlyHighTones()
		{
			if (HighPureTones.Any() && !LowPureTones.Any())
				return true;

			return false;
		}

		public void AddSample(float sample)
		{
			foreach (var estimator in estimators.Values)
				estimator.Add(sample);
		}

		public int FindStrongestHighTone() => StrongestOf(HighPureTones);

		public int FindStrongestLowTone() => StrongestOf(LowPureTones);

		private int StrongestOf(IEnumerable<int> pureTones)
		{
			if (pureTones != null && pureTones.Any())
			{
				return pureTones
					.Select(
						tone => new
						{
							Tone = tone,
							Power = estimators[tone].AmplitudeSquared
						})
					.OrderBy(result => result.Power)
					.Select(result => result.Tone)
					.Last();
			}

			return 0;
		}
	}
}
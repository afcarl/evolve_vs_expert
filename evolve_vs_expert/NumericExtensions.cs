using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace evolve_vs_expert
{
	public static class NumericExtensions
	{
		public static int SampleFromDistribution (this Random _rand, double[] dist)
		{
			double val = 0;
			double sample = _rand.NextDouble ();
			int result = -1;
			for (int i = 0; i < dist.Length; i++) 
			{
				val += dist[i];
				if (sample < val)
				{
					result = i;
					break;
				}
			}
			return result;
		}

		public static void Normalize(this double[] arr)
		{
			var sum = arr.Sum();
			for(int i = 0 ; i < arr.Length; i++)
				arr[i] = arr[i] / sum;
		}

		public static string Flatten<T>(this IEnumerable<T> list)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("[");
			for(int i = 0; i < list.Count(); i++)
			{
				sb.Append(list.ElementAt(i).ToString());
				if(i < list.Count() - 1)
					sb.Append(",");
			}
			sb.Append("]");
			return sb.ToString();
		}
	}
}


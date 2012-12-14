using System;
using holdem_engine;
using HoldemFeatures;

namespace evolve_vs_expert
{
	public class NeuralNetworkPlayer : IPlayer
	{
		FeedForwardNeuralNetwork _network;
		LimitFeatureGenerator _featureGen;
		Random _rand = new Random();

		public NeuralNetworkPlayer (FeedForwardNeuralNetwork network)
		{
			_network = network;
			_featureGen = new LimitFeatureGenerator();
		}

		public void GetAction(HandHistory history, out holdem_engine.Action.ActionTypes action, out double amount)
		{
			var xml = history.ToXmlHand();
			int rIdx = (int)history.CurrentRound - 1;
			int aIdx = xml.Rounds[rIdx].Actions.Length;
			var features = _featureGen.GenerateMonolithicNeuralNetworkFeatures(xml, rIdx, aIdx, false);

			for(int i = 0; i < features.Length; i++)
			{
				var feature = features[i];
				if(feature < 0 || feature > 1)
				{
					_featureGen.PrintFeatureList();
					throw new Exception(string.Format("Feature {0}: {1}", i, feature));
				}
			}

//			Console.WriteLine("{0} features", features.Length);
//			Console.WriteLine("Features: {0}", features.Flatten());
			var probs = _network.Activate(features);
			//Console.WriteLine("Raw Probs: {0}", probs.Flatten());
			probs.Normalize();
			//Console.WriteLine("Normalized: {0}", probs.Flatten());
			int result = _rand.SampleFromDistribution(probs);

			if (result == 0)
				action = holdem_engine.Action.ActionTypes.Fold;
			else if (result == 1)
				action = holdem_engine.Action.ActionTypes.Call;
			else if (result == 2)
				action = holdem_engine.Action.ActionTypes.Raise;
			else
				throw new Exception("Unknown class: " + result);
			amount = 0;
		}
	}
}


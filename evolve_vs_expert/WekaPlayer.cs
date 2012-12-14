using System;
using holdem_engine;
using weka.classifiers;
using HoldemFeatures;
using weka.core;

namespace evolve_vs_expert
{
	public class WekaPlayer : IPlayer
	{
		static Classifier[] _models;
		static LimitFeatureGenerator _featureGen;
		static Instances[] _instances;
		Random _rand = new Random();
		
		public bool MixedPolicy { get; set; }


		
		public WekaPlayer(string preflopModelFile, string flopModelFile,
		                  string turnModelFile, string riverModelFile)
		{
			if (_featureGen == null)
			_featureGen = new LimitFeatureGenerator() { SkipMissingFeatures = true };
			
			if (_models == null)
			{
				_models = new Classifier[4];
				_models[0] = (Classifier)weka.core.SerializationHelper.read(preflopModelFile);
				_models[1] = (Classifier)weka.core.SerializationHelper.read(flopModelFile);
				_models[2] = (Classifier)weka.core.SerializationHelper.read(turnModelFile);
				_models[3] = (Classifier)weka.core.SerializationHelper.read(riverModelFile);
			}
			
			if (_instances == null)
			{
				_instances = new Instances[4];
				for (int i = 0; i < 4; i++)
					_instances[i] = _featureGen.GenerateClassifierInstances(i);
			}
			
		}
		
		#region IPlayer implementation
		
		public void GetAction(HandHistory history, out holdem_engine.Action.ActionTypes action, out double amount)
		{
			
			var xml = history.ToXmlHand();
			int rIdx = (int)history.CurrentRound - 1;
			int aIdx = xml.Rounds[rIdx].Actions.Length;
			var features = _featureGen.GenerateFeatures(xml, rIdx, aIdx, _instances[rIdx], false);
			
			var classifier = _models[rIdx];
			
			int result;
			// Mixed policies take a randomized action based on a probability distribution.
			if (MixedPolicy)
			{
				double[] dist = classifier.distributionForInstance(features);
				result = _rand.SampleFromDistribution(dist);
			}
			else
			{
				// Otherwise, the policy is a pure strategy taking deterministic actions.
				result = (int)classifier.classifyInstance(features);
			}
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
		
#endregion
	}
}


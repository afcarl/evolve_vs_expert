using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace evolve_vs_expert
{
	public class FeedForwardNeuralNetwork
	{
		public int[] Nodes { get; set; }
		public double[][,] Weights { get; set; }
		
		double[][] _activations;
		int _totalWeights;
		
		public FeedForwardNeuralNetwork (int inputs, int outputs, params int[] hidden)
		{
			Nodes = new int[2 + (hidden == null? 0 : hidden.Length)];
			Nodes[0] = inputs + 1; // add 1 input for the bias node
			Nodes[Nodes.Length - 1] = outputs;
			if(hidden != null && hidden.Length > 0)
				hidden.CopyTo(Nodes, 1);
			
			Weights = new double[Nodes.Length - 1][,];
			_totalWeights = 0;
			for(int i = 0 ; i < Weights.Length; i++)
			{
				Weights[i] = new double[Nodes[i], Nodes[i+1]];
				_totalWeights += Nodes[i] * Nodes[i+1];
			}
			
			
			_activations = new double[Nodes.Length][];
			for(int i = 0; i < Nodes.Length; i++)
				_activations[i] = new double[Nodes[i]];
		}
		
		public void SetWeights(IEnumerable<double> newWeights)
		{
			Debug.Assert(newWeights.Count() == _totalWeights, string.Format("Need {0} weights", _totalWeights));
			int curIdx = 0;
			for(int i = 0 ; i < Weights.Length; i++)
				for(int srcIdx = 0; srcIdx < Nodes[i]; srcIdx++)
					for(int destIdx = 0; destIdx < Nodes[i+1]; destIdx++)
				{
					Weights[i][srcIdx, destIdx] = newWeights.ElementAt(curIdx);
					curIdx++;
				}
		}
		
		public double[] Activate(double[] inputs)
		{
			Debug.Assert (inputs.Length == Nodes[0] - 1, string.Format("Need {0} inputs", Nodes[0] - 1));
			
			// Feed the inputs into the network
			inputs.CopyTo(_activations[0], 0);
			
			// Set the bias node
			_activations[0][_activations[0].Length - 1] = 1;
			
			// Activate each layer one-by-one
			for(int i = 1; i < _activations.Length; i++)
			{
				// Zero out the array
				for(int destIdx = 0; destIdx < Nodes[i]; destIdx++)
					_activations[i][destIdx] = 0;
				
				// Activate the layer
				for(int srcIdx = 0; srcIdx < Nodes[i-1]; srcIdx++)
					for(int destIdx = 0; destIdx < Nodes[i]; destIdx++)
						_activations[i][destIdx] += Weights[i-1][srcIdx,destIdx] * _activations[i-1][srcIdx];

				// Apply sigmoid activation function
				for(int destIdx = 0; destIdx < Nodes[i]; destIdx++)
					_activations[i][destIdx] = 1.0 / (1.0 + Math.Exp (-_activations[i][destIdx]));
			}

			// Return the output layer
			return _activations.Last();
		}
	}
}


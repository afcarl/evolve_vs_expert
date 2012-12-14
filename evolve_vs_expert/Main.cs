using System;
using holdem_engine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace evolve_vs_expert
{
	class MainClass
	{
		public static void Main (string[] args)
		{
//			Random rand = new Random();
//			for(int i = 0; i < 42*25 + 25*10 + 10*3; i++)
//				Console.Write("{0},", rand.NextDouble() * 10.0 - 5.0);
//			return;

			int argIdx = 0;
			int hands = 100;// int.Parse(args[argIdx++]);
			int numPlayers = 6; //int.Parse(args[argIdx++]);

			string neuralnetPath = args[argIdx++]; //Path.GetFullPath(@"./models/sample.network");
			string resultsPath = args[argIdx++];
			string finishedFlagsPath = args[argIdx++];

			IEnumerable<double> weights;
			FeedForwardNeuralNetwork net;
			using(TextReader reader = new StreamReader(neuralnetPath))
			{
				string line = reader.ReadLine();
				int inputs = int.Parse(line);
				int outputs = 3;

				line = reader.ReadLine();
				int[] hidden = line.Split(',').Select(t => int.Parse(t)).ToArray();

				net = new FeedForwardNeuralNetwork(inputs, outputs, hidden);

				line = reader.ReadLine();
				weights = line.Split(',').Select(t => double.Parse(t));
				net.SetWeights(weights);
			}
			Console.WriteLine("Loaded neural network now. Now loading weka model...");
			var Bot = new WekaPlayer(Path.GetFullPath(@"./models/preflop.model"),
			                         Path.GetFullPath(@"./models/flop.model"),
			                         Path.GetFullPath(@"./models/turn.model"),
			                         Path.GetFullPath(@"./models/river.model"));

			var neuralNetPlayer = new NeuralNetworkPlayer(net);


			HandEngine engine = new HandEngine();
			double[] blinds = new double[]{ 10, 20 };
			uint button = 1;

			int humanSeat = 3;
			Seat[] players = new Seat[numPlayers];
			for(int i = 1; i <= numPlayers; i++)
			{
				if(i == humanSeat)
					players[i-1] = new Seat(i, "NeuralNetwork", 1000, neuralNetPlayer);
				else
					players[i-1] = new Seat(i, "Bot" + i, 1000, Bot);
			}

			HandHistory[] histories = new HandHistory[hands];
			double score = 0;
			var start = DateTime.UtcNow;
			for(int curHand = 0; curHand < hands; curHand++)
			{
				if(curHand % 100 == 0)
					Console.WriteLine(curHand);
				HandHistory history = new HandHistory(players, (ulong)curHand, button, blinds, 0, BettingStructure.Limit);

				engine.PlayHand(history);

				histories[curHand] = history;

				button++;
				if(button > players.Length)
					button = 1;

				// record winnings and reset chips
				foreach(var player in players)
				{
					if(player.SeatNumber == humanSeat)
						score += player.Chips - 1000;
					player.Chips = 1000;
				}
			}
			var end = DateTime.UtcNow;

			Console.WriteLine("Total winnings: {0}", score);
			Console.WriteLine("Time: {0}", end - start);

			using(TextWriter writer = new StreamWriter(resultsPath))
				writer.WriteLine(-score);

			File.Create(finishedFlagsPath);
		}
	}
}

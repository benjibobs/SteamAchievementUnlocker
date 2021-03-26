using System;
using System.Threading;
using System.Net;
using System.IO;
using Steamworks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Achievables
{
	class MainClass
	{

		private static void initSteamApi() {

			if(!SteamAPI.Init()) {
				Console.WriteLine("Could not initialize Steam API.");
				exitWithError();
				return;
			}
			
			Console.WriteLine("Requesting current stats...");

			if(!SteamUserStats.RequestCurrentStats()) {
				Console.WriteLine ("Failed to fetch stats.");
				exitWithError();
			}
		}

		private static List<Achievement> populateAchievementsLists(string apiKey, string gameId) {

			List<Achievement> achievements = new List<Achievement>();

			Console.Clear ();
			Console.WriteLine ("Finding achievements...");

			string response = "";

			try {
				WebRequest schemaReq = WebRequest.Create(string.Format("http://api.steampowered.com/ISteamUserStats/GetSchemaForGame/v0002/?key={0}&appid={1}&l=english&format=json", apiKey, gameId));
				Stream objStream = schemaReq.GetResponse().GetResponseStream();

				StreamReader objReader = new StreamReader(objStream);

				response = objReader.ReadToEnd();
			} catch(Exception) {
				Console.WriteLine ("No achievements found for AppID {0}", gameId);
				exitWithError();
            }

			Console.WriteLine ("Parsing achievements...");

			try {
				JObject schema = JObject.Parse (response);

				JEnumerable<JToken> results = schema["game"]["availableGameStats"]["achievements"].Children();

				foreach (JToken result in results) {
					achievements.Add(new Achievement(result.SelectToken("displayName").ToString(), result.SelectToken("name").ToString()));
				}
			} catch(Exception) {
				Console.WriteLine ("Failed to parse json - there could be no achievements for this game.");
				exitWithError();
			}

			Console.WriteLine ("Parsed.");

			return achievements;
		}

		public static void Main () {

			Console.Write ("Please enter the game's AppID (found in the store url): ");

			string gameId = Console.ReadLine();

			Console.Write("Please enter your API key: ");

			string apiKey = Console.ReadLine();

			File.WriteAllText(string.Format("{0}\\steam_appid.txt", AppDomain.CurrentDomain.BaseDirectory), gameId);

			initSteamApi ();

			List<Achievement> achievements = populateAchievementsLists(apiKey, gameId);

			Console.WriteLine ("Press spacebar to unlock achievements, or escape to exit.");

			while (true) {
				if (Console.KeyAvailable) {

					ConsoleKeyInfo info = Console.ReadKey(true);

					if (info.Key == ConsoleKey.Escape) {
						break;
					} else if (info.Key == ConsoleKey.Spacebar) {

						foreach(Achievement achievement in achievements) {
							SteamUserStats.SetAchievement(achievement.getId());
							Console.WriteLine("Unlocking '{0}'...", achievement.getDisplayName());
						}

						if (!SteamUserStats.StoreStats()) {
							Console.WriteLine("Failed to save user stats, achievements not unlocked.");
						} else {
							Console.WriteLine("Saved user stats, achievements successfully unlocked.");
						}

						break;
					}

				}

				Thread.Sleep(50);
			}
			
			Console.WriteLine ("Press <ENTER> to exit");
			Console.ReadLine();
			SteamAPI.Shutdown ();
		}

		private static void exitWithError() {
			Console.ReadLine();
			Environment.Exit(1);
		}
	}
}

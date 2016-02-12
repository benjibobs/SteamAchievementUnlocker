using System;
using System.Threading;
using System.Net;
using System.IO;
using Steamworks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Achievables
{
	class MainClass
	{
		
		private static string m_GameID;

		private static List<string> names = new List<string>();
		private static List<string> ufNames = new List<string>();

		private static void init()
		{
			if(!SteamAPI.Init())
			{
				return;
			}

			Console.WriteLine("Reqesting current stats...");

			if(!SteamUserStats.RequestCurrentStats())
			{
				Console.WriteLine ("Failed to fetch stats.");
				Console.ReadLine ();
				Environment.Exit (1);
			}
				
		}

		private static void getSchema()
		{
			Console.Clear ();
			Console.WriteLine ("Finding achievements...");

			string response = "";

			try{
				WebRequest schemaReq = WebRequest.Create("http://api.steampowered.com/ISteamUserStats/GetSchemaForGame/v0002/?key=E05FFFE70321142E3D01F2BC9E3026E2&appid="+m_GameID+"&l=english&format=json");
				Stream objStream = schemaReq.GetResponse().GetResponseStream();

				StreamReader objReader = new StreamReader(objStream);

				response = objReader.ReadToEnd();
			}catch(Exception){
				Console.WriteLine ("No achievements found for AppID " + m_GameID);
				Console.ReadLine ();
				Environment.Exit (1);
			}

			Console.WriteLine ("Parsing achievements...");

			try{
				JObject schema = JObject.Parse (response);

				JEnumerable<JToken> results = schema["game"]["availableGameStats"]["achievements"].Children();

				foreach (JToken result in results)
				{
					names.Add (result.SelectToken ("name").ToString());
					ufNames.Add (result.SelectToken ("displayName").ToString());
				}
			}catch(Exception){
				Console.WriteLine ("Failed to parse json - there are likely no achievements for this game.");
				Console.ReadLine ();
				Environment.Exit (1);
			}

			Console.WriteLine ("Parsed.");

		}

		public static void Main (string[] args)
		{

			Console.Write ("Please enter the game's AppID (found in the store url): ");

			m_GameID = Console.ReadLine();

			File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"\steam_appid.txt", m_GameID);

			init ();
			SteamAPI.RunCallbacks();
			getSchema ();

			Console.WriteLine ("Press spacebar to unlock achievements, or escape to exit.");

			while (true) {
				// Must be called from the primary thread.
				SteamAPI.RunCallbacks();

				if (Console.KeyAvailable) {
					ConsoleKeyInfo info = Console.ReadKey(true);

					if (info.Key == ConsoleKey.Escape) {
						break;
					}
					else if (info.Key == ConsoleKey.Spacebar) {
						for(int i = 0; i < names.Count;i++)
						{
							SteamUserStats.SetAchievement (names[i]);
							Console.WriteLine ("Unlocking \'" + ufNames[i] + "\'...");
						}
						if (!SteamUserStats.StoreStats ()) {
							Console.WriteLine ("Achievements failed to unlock...");
						} else {
							Console.WriteLine ("Achievements successfully unlocked.");
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
	}
}

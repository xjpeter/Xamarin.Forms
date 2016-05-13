using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms.Controls.Issues;

namespace Xamarin.Forms.Controls
{
	public class App : Application
	{
		public const string AppName = "XamarinFormsControls";
		static string s_insightsKey;

		// ReSharper disable once InconsistentNaming
		public static int IOSVersion = -1;

		public static List<string> AppearingMessages = new List<string>();

		static Dictionary<string, string> s_config;
		readonly ITestCloudService _testCloudService;

		public App()
		{
			_testCloudService = DependencyService.Get<ITestCloudService>();
			InitInsights();

			MainPage = new MasterDetailPage
			{
				Master = new ContentPage { Title = "Master", BackgroundColor = Color.Red },
				Detail = CoreGallery.GetMainPage()
			};
		}

		protected override void OnAppLinkRequestReceived(Uri uri)
		{

			var appDomain = "http://" + AppName.ToLowerInvariant() + "/";

			if (!uri.ToString().ToLowerInvariant().StartsWith(appDomain))
				return;

			var url = uri.ToString().Replace(appDomain, "");

			var parts = url.Split('/');
			if (parts.Length == 2)
			{
				var isPage = parts[0].Trim().ToLower() == "gallery";
				if (isPage)
				{
					string page = parts[1].Trim();
					var pageForms = Activator.CreateInstance(Type.GetType(page));

					var appLinkPageGallery = pageForms as AppLinkPageGallery;
					if (appLinkPageGallery != null)
					{
						appLinkPageGallery.ShowLabel = true;
						(MainPage as MasterDetailPage)?.Detail.Navigation.PushAsync((pageForms as Page));
					}
				}
			}

			base.OnAppLinkRequestReceived(uri);
		}

		public static Dictionary<string, string> Config
		{
			get
			{
				if (s_config == null)
					LoadConfig();

				return s_config;
			}
		}

		public static string InsightsApiKey
		{
			get
			{
				if (s_insightsKey == null)
				{
					string key = Config["InsightsApiKey"];
					s_insightsKey = string.IsNullOrEmpty(key) ? Insights.DebugModeKey : key;
				}

				return s_insightsKey;
			}
		}

		public static ContentPage MenuPage { get; set; }

		public void SetMainPage(Page rootPage)
		{
			MainPage = rootPage;
		}

		static Assembly GetAssembly(out string assemblystring)
		{
			assemblystring = typeof(App).AssemblyQualifiedName.Split(',')[1].Trim();
			var assemblyname = new AssemblyName(assemblystring);
			return Assembly.Load(assemblyname);
		}

		void InitInsights()
		{
			if (Insights.IsInitialized)
			{
				Insights.ForceDataTransmission = true;
				if (_testCloudService != null && _testCloudService.IsOnTestCloud())
					Insights.Identify(_testCloudService.GetTestCloudDevice(), "Name", _testCloudService.GetTestCloudDeviceName());
				else
					Insights.Identify("DemoUser", "Name", "Demo User");
			}
		}

		static void LoadConfig()
		{
			s_config = new Dictionary<string, string>();

			string keyData = LoadResource("controlgallery.config").Result;
			string[] entries = keyData.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			foreach (string entry in entries)
			{
				string[] parts = entry.Split(':');
				if (parts.Length < 2)
					continue;

				s_config.Add(parts[0].Trim(), parts[1].Trim());
			}
		}

		static async Task<string> LoadResource(string filename)
		{
			string assemblystring;
			Assembly assembly = GetAssembly(out assemblystring);

			Stream stream = assembly.GetManifestResourceStream($"{assemblystring}.{filename}");
			string text;
			using (var reader = new StreamReader(stream))
				text = await reader.ReadToEndAsync();
			return text;
		}
	}
}
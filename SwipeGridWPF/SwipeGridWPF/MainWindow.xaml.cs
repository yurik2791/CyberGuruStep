using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Animation;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Input;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Controls;

namespace SwipeGridWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public Storyboard StBoard = new Storyboard();
		const bool ToLeft = false;
		const bool ToRight = true;

		public MainWindow()
		{
			InitializeComponent();

			#region My region
			//

			//TcpListener listener = new TcpListener(IPAddress.Any, 80);
			//listener.Start();

			//while (true)
			//{
			//	var tcpStream = listener.AcceptTcpClient().GetStream();

			//	var response = @"<html>
			//	<HEAD>
			//	</HEAD>
			//	<BODY>Test</BODY>
			//	</html>";
			//	var buffer = Encoding.ASCII.GetBytes(response);

			//	tcpStream.Write(buffer, 0, buffer.Length);
			//	tcpStream.Flush();
			//	tcpStream.Close();
			//}
			#endregion
			List<string> listDestination = new List<string> { "Chisinau", "London", "Paris", "Geneva", "Stambul" };

			comboBoxDestination.ItemsSource = listDestination;
			comboBoxSource.ItemsSource = listDestination;
			comboBoxDestination.SelectedItem = listDestination[1];
			comboBoxSource.SelectedItem = listDestination[0];
			comboBoxClass.ItemsSource = new List<string> { "Buisness", "Econom", "Very Econom" };
			comboBoxClass.SelectedItem = comboBoxClass.Items[0];
			txtPrice.Text = "200";
			txtFirstName.Text = "Josan";
			txtSecondName.Text = "Iurie";
			dateDeparture.SelectedDate = DateTime.Now;
			dateArrival.SelectedDate = DateTime.Now;
			List<string> listPrice = new List<string> { "LEI", "RON", "$", "EUR" };
			listBoxPrice.ItemsSource = listPrice;
			var selectedItems = listBoxPrice.SelectedItems;
		}

		void ChangeGrid(FrameworkElement firstElement, FrameworkElement secondElement, bool changeSide)
		{
			StBoard?.Begin(this, true);

			var ticknessLeft = new Thickness(Width, 0, -Width, 0);
			var ticknessRight = new Thickness(-Width, 0, Width, 0);
			var ticknessClient = new Thickness(0, 0, 0, 0);

			var timeSpanStarting = TimeSpan.FromSeconds(0);
			var timeSpanStopping = TimeSpan.FromSeconds(1);

			var keyTimeStarting = KeyTime.FromTimeSpan(timeSpanStarting);
			var keyTimeStopping = KeyTime.FromTimeSpan(timeSpanStopping);

			secondElement.Margin = changeSide ? ticknessRight : ticknessLeft;
			secondElement.Visibility = Visibility.Visible;

			var storyboardTemp = new Storyboard();

			var currentThicknessAnimationUsingKeyFrames = new ThicknessAnimationUsingKeyFrames { BeginTime = timeSpanStarting };

			Storyboard.SetTargetName(currentThicknessAnimationUsingKeyFrames, firstElement.Name);
			Storyboard.SetTargetProperty(currentThicknessAnimationUsingKeyFrames, new PropertyPath("(FrameworkElement.Margin)"));
			currentThicknessAnimationUsingKeyFrames.KeyFrames.Add(new SplineThicknessKeyFrame(ticknessClient, keyTimeStarting));
			currentThicknessAnimationUsingKeyFrames.KeyFrames.Add(new SplineThicknessKeyFrame(changeSide ? ticknessLeft : ticknessRight, keyTimeStopping));

			storyboardTemp.Children.Add(currentThicknessAnimationUsingKeyFrames);

			var nextThicknessAnimationUsingKeyFrames = new ThicknessAnimationUsingKeyFrames { BeginTime = timeSpanStarting };

			Storyboard.SetTargetName(nextThicknessAnimationUsingKeyFrames, secondElement.Name);
			Storyboard.SetTargetProperty(nextThicknessAnimationUsingKeyFrames, new PropertyPath("(FrameworkElement.Margin)"));
			nextThicknessAnimationUsingKeyFrames.KeyFrames.Add(new SplineThicknessKeyFrame(changeSide ? ticknessRight : ticknessLeft, keyTimeStarting));
			nextThicknessAnimationUsingKeyFrames.KeyFrames.Add(new SplineThicknessKeyFrame(ticknessClient, keyTimeStopping));

			storyboardTemp.Children.Add(nextThicknessAnimationUsingKeyFrames);

			storyboardTemp.Completed += delegate

			{
				firstElement.Visibility = Visibility.Hidden;
				StBoard = null;
			};

			StBoard = storyboardTemp;
			BeginStoryboard(storyboardTemp);
		}

		private void btnNext_Click(object sender, RoutedEventArgs e)
		{
			groupBoxInfoBilet.Content = $"{comboBoxSource.SelectedItem} to {comboBoxDestination.SelectedItem}\nDate departure: {dateDeparture.Text}";
			if (rbToAndFrom.IsChecked == true)
			{
				groupBoxInfoBilet.Content += $"\nDate arrival: {dateArrival.Text}";
			}
			ChangeGrid(gridFirst, gridSecond, ToLeft);
		}

		private void btnPrev_Click(object sender, RoutedEventArgs e)
		{
			ChangeGrid(gridSecond, gridFirst, ToRight);
		}

		private void rbToAndFrom_Checked(object sender, RoutedEventArgs e)
		{
			dateArrival.Visibility = Visibility.Visible;
		}

		private void rbTo_Checked(object sender, RoutedEventArgs e)
		{
			dateArrival.Visibility = Visibility.Hidden;
		}

		private void btnInsert_Click(object sender, RoutedEventArgs e)
		{
			var constr = ConfigurationManager.ConnectionStrings["constr"].ToString();
			var dataContext = new TicketDataContext(constr);
			new Thread(() =>
			{
				var res = (from c in dataContext.Tickets
						   select new
						   {
							   c.FirstName,
							   c.SecondName,
							   Source = c.City,
							   Destination = c.City1,
							   c.Date1,
							   c.Date2,
							   c.Class,
							   c.Price,
						   }).ToList();

				dataGrid.Dispatcher.Invoke(() => { dataGrid.ItemsSource = res; });
			}).Start();

		}


		private void btnSale_Click(object sender, RoutedEventArgs e)
		{
			FirstGrid.Dispatcher.Invoke(() =>
			{
				var constr = ConfigurationManager.ConnectionStrings["constr"].ToString();
				var dataContext = new TicketDataContext(constr);
				var newTicket = new Bilet()
				{
					Id = Guid.NewGuid(),
					FirstName = txtFirstName.Text,
					LastName = txtSecondName.Text,
					Source = (from c in dataContext.Gorods where c.CityName == comboBoxSource.Text select c.Id).First(),
					Destination = (from c in dataContext.Gorods where c.CityName == comboBoxDestination.Text select c.Id).First(),
					Class = comboBoxClass.Text,
					DateOne = (DateTime) dateDeparture.SelectedDate,
					DateTwo = (DateTime) dateArrival.SelectedDate,
					Way = rbTo.IsChecked == true ? (byte)0 : (byte)1,
					Price = decimal.Parse(txtPrice.Text, CultureInfo.InvariantCulture)
				};
				dataContext.Bilets.InsertOnSubmit(newTicket);
				dataContext.SubmitChanges();
			});

			#region comment

			//var connection = new SqlConnection(constr);
			//connection.Open();
			//var comand = connection.CreateCommand();
			//comand.CommandText =
			//	$"INSERT INTO Bilet(FirstName, LastName, Source, Destination, Class, Way, DateOne, DateTwo, Price) " +
			//	$"VALUES('{txtFirstName.Text}'," +
			//	$"'{txtSecondName.Text}'," +
			//	$" (SELECT Id FROM Gorod WHERE CityName = '{comboBoxSource.Text}'), " +
			//	$"(SELECT Id FROM Gorod WHERE CityName = '{comboBoxDestination.Text}')," +
			//	$" '{comboBoxClass.Text}'," +
			//	$" {(rbTo.IsChecked == true ? 0 : 1)}," +
			//	$"'{(dateDeparture.DisplayDate.ToString(@"yyyy-MM-dd HH:mm:ss"))}','{(rbTo.IsChecked == true ? null : (dateArrival.DisplayDate.ToString(@"yyyy-MM-dd HH:mm:ss")))}'," +
			//	$"{(Int16.Parse(txtPrice.Text))})";
			//comand.ExecuteNonQuery();
			//comand.CommandText =
			//	$"SELECT b.FirstName, b.LastName, G.CityName, G.CityName,b.Class, b.Way, b.DateOne, b.DateTwo, b.Price " +
			//	$"FROM Bilet b, Gorod G, Gorod Gd WHERE b.Source=G.Id AND  b.Destination = Gd.Id";
			//var reader = comand.ExecuteReader();
			//string str = String.Empty;
			//int i = 0;

			//while (reader.Read())
			//{
			//	for (int i = 0; i < 9; i++)
			//	{
			//		str += reader.GetName(i) + ": " + reader[i] + Environment.NewLine;
			//		i++;
			//	}
			//}
			//MessageBox.Show(str, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
			//connection.Close();

			#endregion


		}
	}
}

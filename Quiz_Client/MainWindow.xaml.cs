using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;

namespace Quiz_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly GameClient game = null;
        private int selection;
        DispatcherTimer timer;
        private static int time;
        public static string Time { get; set; }
        public bool? toggleAnswers;
        public MainWindow()
        {
            InitializeComponent();

            TriviaGame.WindowState = WindowState.Maximized;

            game = new GameClient();
            txtQuestion.Text = "ENTER A USERNAME:";
            txtInput.Focus();
            selection = 8;
            toggleAnswers = null;

            // Set up initial UI
            btnArea1.Visibility = Visibility.Hidden;
            btnArea2.Visibility = Visibility.Hidden;
            btnArea3.Visibility = Visibility.Hidden;
            btnArea4.Visibility = Visibility.Hidden;
            btnArea5.Visibility = Visibility.Hidden;
            txtCountdown.Visibility = Visibility.Hidden;
            btnAnswers.Visibility = Visibility.Hidden;
            txtShowAnswers.Visibility = Visibility.Hidden;

            // Set up timer for counting down
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += timer_Tick;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Time = time--.ToString();
            txtCountdown.Text = Time;
            if (time <= 0)
            {
                Send("0");
            }
        }

        private void btnNameSubmit_Click(object sender, RoutedEventArgs e)
        {
            string status = "";

            // Input validation: any username less than 16 chars in length
            if (txtInput.Text.Length < 16 && txtInput.Text.Trim() != "")
            {
                // Validation: no , ; = '
                if (txtInput.Text.Contains(' ') || txtInput.Text.Contains(',') || txtInput.Text.Contains(';') || txtInput.Text.Contains('=') || txtInput.Text.Contains(':') || txtInput.Text.Contains('\'') || txtInput.Text.Contains('\"'))
                {
                    status = "Enter new username WITHOUT , ; : = ' \" characters".ToUpper();
                }
                else
                {
                    // Get the input and send to server
                    game.UserName = txtInput.Text.Trim();
                    status = game.NewGame();

                    // Make sure the send was ok
                    if (status == "OK")
                    {
                        // Confirm player ready to start
                        MessageBox.Show("You will have 20 seconds to answer each question. \nYou get 1 point for each second remaining" +
                            " on the countdown!", $"Get ready {game.UserName}!!".ToUpper(), MessageBoxButton.OK);

                        // Hide user input UI
                        txtInput.Visibility = Visibility.Hidden;
                        btnNameSubmit.Visibility = Visibility.Hidden;

                        // Display Question UI
                        DisplayQuestion();

                        return;
                    }
                }

            }
            else
            {
                // Ask for name again
                txtQuestion.Text = "Enter another username, between 1-20 characters".ToUpper();
            }

            // Ask for name again
            txtQuestion.Text = status.ToUpper();
        }


        private void DisplayQuestion()
        {
            // Start the countdown.
            time = 20;
            timer.Start();

            // Display UI
            btnArea1.Visibility = Visibility.Visible;
            btnArea2.Visibility = Visibility.Visible;
            btnArea3.Visibility = Visibility.Visible;
            btnArea4.Visibility = Visibility.Visible;
            txtCountdown.Visibility = Visibility.Visible;

            // Fill in text
            txtUsername.Text = game.UserName;
            txtQuestion.Text = game.QuestionText.ToUpper();
            btnArea1.Content = game.Answers[game.Selection1].ToUpper();
            btnArea2.Content = game.Answers[game.Selection2].ToUpper();
            btnArea3.Content = game.Answers[game.Selection3].ToUpper();
            btnArea4.Content = game.Answers[game.Selection4].ToUpper();

        }


        private void DisplayGameOver()
        {
            // Stop timer
            timer.Stop();

            // Display UI
            btnArea1.Visibility = Visibility.Visible;
            btnArea2.Visibility = Visibility.Visible;
            btnArea3.Visibility = Visibility.Visible;
            btnArea4.Visibility = Visibility.Visible;
            btnArea5.Visibility = Visibility.Visible;
            btnAnswers.Visibility = Visibility.Visible;
            txtCountdown.Visibility = Visibility.Hidden;

            // Disable buttons
            btnArea1.IsEnabled = false;
            btnArea2.IsEnabled = false;
            btnArea3.IsEnabled = false;
            btnArea4.IsEnabled = false;

            // Fill in text
            txtUsername.Text = "YOUR SCORE: " + game.Score;
            txtQuestion.Text = "LEADERBOARD";
            this.Title = "GAME OVER " + game.UserName.ToUpper() + ", THANKS FOR PLAYING!";

            // Display the leaderboard;
            btnArea1.Content = $"{game.Leaderboard.ElementAt(0).Key.ToUpper()}: {game.Leaderboard.ElementAt(0).Value.ToUpper()}";
            btnArea2.Content = $"{game.Leaderboard.ElementAt(1).Key.ToUpper()}: {game.Leaderboard.ElementAt(1).Value.ToUpper()}";
            btnArea3.Content = $"{game.Leaderboard.ElementAt(2).Key.ToUpper()}: {game.Leaderboard.ElementAt(2).Value.ToUpper()}";
            btnArea4.Content = $"{game.Leaderboard.ElementAt(3).Key.ToUpper()}: {game.Leaderboard.ElementAt(3).Value.ToUpper()}";
            btnArea5.Content = $"{game.Leaderboard.ElementAt(4).Key.ToUpper()}: {game.Leaderboard.ElementAt(4).Value.ToUpper()}";
        }


        private void btnArea1_Click(object sender, RoutedEventArgs e)
        {

            selection = 1;
            Send(txtCountdown.Text);
        }

        private void btnArea2_Click(object sender, RoutedEventArgs e)
        {
            selection = 2;
            Send(txtCountdown.Text);
        }

        private void btnArea3_Click(object sender, RoutedEventArgs e)
        {
            selection = 3;
            Send(txtCountdown.Text);
        }

        private void btnArea4_Click(object sender, RoutedEventArgs e)
        {
            selection = 4;
            Send(txtCountdown.Text);
        }

        private void Send(string score)
        {
            string status = "";

            // Send answer if an answer was checked
            if (selection != 0)
            {
                status = game.SendAnswer(selection, score);
            }

            // Check result
            if (status == "OK")
            {
                // Display next question
                DisplayQuestion();
            }
            else if (status == "GAMEOVER")
            {
                // Display leaderboard
                DisplayGameOver();
            }
            else
            {
                // Display error
                txtQuestion.Text = status.ToUpper();
            }
        }



        private void btnAnswers_Click(object sender, RoutedEventArgs e)
        {
            game.GetAnswers();
            DisplayAnswers();
        }

        private void DisplayAnswers()
        {
            // Set Toggle
            if (toggleAnswers == null)
            {
                // Display UI elements
                txtShowAnswers.Visibility = Visibility.Visible;
                btnAnswers.Content = "VIEW LEADERS";
                toggleAnswers = true;

                // Fill in text from dictionary
                txtShowAnswers.Text = $"\nHere are the correct answers, {game.UserName}!:\n\n" +
                $"1) {game.QuestionAnswerTexts.ElementAt(0).Key.ToUpper()}\n" +
                $"{game.QuestionAnswerTexts.ElementAt(0).Value}\n\n" +
                $"2) {game.QuestionAnswerTexts.ElementAt(1).Key.ToUpper()}\n" +
                $"{game.QuestionAnswerTexts.ElementAt(1).Value}\n\n" +
                $"3) {game.QuestionAnswerTexts.ElementAt(2).Key.ToUpper()}\n" +
                $"{game.QuestionAnswerTexts.ElementAt(2).Value}\n\n" +
                $"4) {game.QuestionAnswerTexts.ElementAt(3).Key.ToUpper()}\n" +
                $"{game.QuestionAnswerTexts.ElementAt(3).Value}\n\n" +
                $"5) {game.QuestionAnswerTexts.ElementAt(4).Key.ToUpper()}\n" +
                $"{game.QuestionAnswerTexts.ElementAt(4).Value}\n\n" +
                $"6) {game.QuestionAnswerTexts.ElementAt(5).Key.ToUpper()}\n" +
                $"{game.QuestionAnswerTexts.ElementAt(5).Value}\n\n" +
                $"7) {game.QuestionAnswerTexts.ElementAt(6).Key.ToUpper()}\n" +
                $"{game.QuestionAnswerTexts.ElementAt(6).Value}\n\n" +
                $"8) {game.QuestionAnswerTexts.ElementAt(7).Key.ToUpper()}\n" +
                $"{game.QuestionAnswerTexts.ElementAt(7).Value}\n\n" +
                $"9) {game.QuestionAnswerTexts.ElementAt(8).Key.ToUpper()}\n" +
                $"{game.QuestionAnswerTexts.ElementAt(8).Value}\n\n" +
                $"10) {game.QuestionAnswerTexts.ElementAt(9).Key.ToUpper()}\n" +
                $"{game.QuestionAnswerTexts.ElementAt(9).Value}\n\n";
            }
            else if (toggleAnswers == false)
            {
                // Display UI elements
                txtShowAnswers.Visibility = Visibility.Visible;
                btnAnswers.Content = "VIEW LEADERS";
                toggleAnswers = true;
            }
            else if (toggleAnswers == true)
            {
                // Hide 
                txtShowAnswers.Visibility = Visibility.Hidden;
                btnAnswers.Content = "VIEW ANSWERS";
                toggleAnswers = false;
            }



        }







        /*
        *	NAME	:	btnEnd_Click
        *	PURPOSE	:	This method will check if the user is connected to the server, and if yes,
        *	            call method to disconnect from server. If no, it will close the application.
        *	INPUTS	:	object sender       Reference to the object that triggered the event
        *	            RoutedEventArgs e   The data identifying the event that was raised
        *	RETURNS	:	None
        */
        private void btnEnd_Click(object sender, RoutedEventArgs e)
        {
            // Confirm user wants to exit
            MessageBoxResult result = MessageBox.Show("Are you sure you want to end the game?", "Confirm Exit".ToUpper(), MessageBoxButton.YesNo);

            // Take action based on user response
            if (result == MessageBoxResult.Yes)
            {
                // Close the window
                Close();
            }
        }








        /*
        * DESCRIPTION : The following code will remove the close button from window
        * AUTHOR : Givens, Matthew
        * DATE : 2016-11-01
        * AVAILABIILTY : codeproject.com/Tips/1155345/How-to-Remove-the-Close-Button-from-a-WPF-ToolWind 
        */
        // Prep stuff needed to remove close button on window
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);


        /*
        * TITLE     :   Window_Loaded
        * AUTHOR    :   Givens, Matthew
        * PURPOSE	:	This borrowed method will remove the close button from the window, so that the user must use
        *               the exit button provided.
        * INPUTS	:	object sender       Reference to the object that triggered the event
        *	            RoutedEventArgs e   The data identifying the event that was raised
        * RETURNS	:	None
        * DATE : 2016-11-01
        * AVAILABIILTY : codeproject.com/Tips/1155345/How-to-Remove-the-Close-Button-from-a-WPF-ToolWind 
        */
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Code to remove close box from window
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }




    }


}
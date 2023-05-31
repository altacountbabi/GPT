using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GPT
{
    public partial class MainWindow : Window
    {
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private GlobalHotkey hotkey;
        bool minimized = false;
        string username;
        string api_key;
        bool systemPromptAdded = false;
        List<AI_Message> chat_logs = new List<AI_Message>()
        {
            new AI_Message { role = "system", content = "More context will be given in the next message" },
        };
        AIClient gpt;

        private IEasingFunction Smooth
        {
            get;
            set;
        } = new QuarticEase
        {
            EasingMode = EasingMode.EaseInOut
        };

        public async void typewrite(TextBlock element, string text, int speed)
        {
            foreach (char c in text)
            {
                element.Text += c;
                await Task.Delay(speed);
            }
        }

        public void animateOpacity(UIElement element, double opacity, double duration)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                EasingFunction = Smooth,
                From = element.Opacity,
                To = opacity,
                Duration = TimeSpan.FromSeconds(duration)
            };

            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        public void animateBorderMargin(Border element, Thickness thickness, double duration)
        {
            ThicknessAnimation animation = new ThicknessAnimation
            {
                EasingFunction = Smooth,
                From = element.Margin,
                To = thickness,
                Duration = TimeSpan.FromSeconds(duration)
            };

            element.BeginAnimation(Border.MarginProperty, animation);
        }

        public void animateWindowHeight(double height, double duration)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                EasingFunction = Smooth,
                From = base.Height,
                To = height,
                Duration = TimeSpan.FromSeconds(duration)
            };

            base.BeginAnimation(Window.HeightProperty, animation);
        }

        public void animateBorderColor(Border element, string hex, double duration)
        {
            var brush = element.Background as SolidColorBrush;
            if (brush != null)
            {
                ColorAnimation animation = new ColorAnimation
                {
                    EasingFunction = Smooth,
                    From = brush.Color,
                    To = (Color)ColorConverter.ConvertFromString(hex),
                    Duration = TimeSpan.FromSeconds(duration)
                };

                element.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
            }
            else
            {
                throw new Exception("pee pee poo poo");
            }
        }

        public void animateTextBoxColor(TextBox element, string hex, double duration)
        {
            var brush = element.Foreground as SolidColorBrush;
            if (brush != null)
            {
                ColorAnimation animation = new ColorAnimation
                {
                    EasingFunction = Smooth,
                    From = brush.Color,
                    To = (Color)ColorConverter.ConvertFromString(hex),
                    Duration = TimeSpan.FromSeconds(duration)
                };

                element.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, animation);
            }
            else
            {
                throw new Exception("pee pee poo poo");
            }
        }

        public async Task<string> promptAi(string prompt)
        {
            string username = File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "username.txt"));
            if (systemPromptAdded == false)
            {
              chat_logs.Add(new AI_Message { role = "system", content = $"You are an helpful assistant The user's name is: \"{username}\". Lastly, NEVER use markdown formatting or code blocks, simply say it as plain text." });
              systemPromptAdded = true;
            }
            chat_logs.Add(new AI_Message { role = "user", content = prompt });

            AI_ChatCompletionResponse response = await gpt.ChatCompletion(chat_logs.ToArray());

            if (response.choices != null)
            {
                chat_logs.Add(new AI_Message { role = "assistant", content = response.choices[0].message.content });
                return response.choices[0].message.content;
            } else 
            {   
                chat_logs.Add(new AI_Message { role = "assistant", content = "There was an error getting a response.." });
                return "There was an error getting a response..";
            }

        }

        public MainWindow()
        {
            InitializeComponent();
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

            Loaded += (s, e) =>
            {
                hotkey = new GlobalHotkey(ModifierKeys.Windows, Key.F12, () =>
                {
                    if (!minimized)
                    {
                        minimized = true;
                        animateBorderMargin(Main, new Thickness(378, 25, 378, 25), 1.0);
                        animateBorderMargin(Response, new Thickness(378, 0, 378, 0), 1.0);
                    }
                    else if (minimized == true)
                    {
                        minimized = false;
                        animateBorderMargin(Main, new Thickness(50, 25, 50, 25), 1.0);
                        animateBorderMargin(Response, new Thickness(50, 0, 50, 0), 1.0);
                    }
                }, this);
            };


            Loaded += (sender, e) =>
            {
                var hwnd = new WindowInteropHelper(this).Handle;

                int extendedStyle = GetWindowLong(hwnd, -20);
                SetWindowLong(hwnd, -20, extendedStyle | WS_EX_TOOLWINDOW);
            };

            Loaded += (sender, e) =>
            {
                username = File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "username.txt"));
                api_key = File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "api_key.txt"));

                if (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(username))
                {
                    MessageBox.Show("The username you provided is empty, please check the username.txt file again!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                gpt = new AIClient(api_key);

                string[] initial_messages = {
                    $"Hi {username}, welcome back!",
                    $"Hi there, {username}! How can I assist you today?",
                    $"Hello {username}, how are you doing?",
                    $"Hi {username}, what brings you here?",
                    $"Welcome {username}, how may I help you today?",
                    $"Hi {username}, it's good to see you again!",
                    $"Hi {username}, what can I help you with today?",
                    $"Hello {username}, how can I be of assistance?",
                    $"Hi {username}, what questions do you have for me?",
                    $"Welcome back, {username}! What can I do for you today?",
                };

                string initial_message = initial_messages.OrderBy(x => Guid.NewGuid()).FirstOrDefault();

                typewrite(Response_Text, initial_message, 5);

                Input.KeyDown += async (s, e_) =>
                {
                    if (string.IsNullOrEmpty(Input.Text) || string.IsNullOrWhiteSpace(Input.Text)) { return; }
                    if (e_.Key == Key.Enter || e_.Key == Key.Return)
                    {
                        string prompt = Input.Text;
                        Response_Text.Text = "";
                        string res = await promptAi(prompt);
                        typewrite(Response_Text, res, 5);
                    }
                };
            };
        }

        private void Main_MouseEnter(object sender, MouseEventArgs e) => animateBorderColor(Main, "#1b1b1b", 0.5);

        private void Main_MouseLeave(object sender, MouseEventArgs e) => animateBorderColor(Main, "#181818", 0.5);

        private void Response_MouseEnter(object sender, MouseEventArgs e) => animateBorderColor(Response, "#1b1b1b", 0.5);

        private void Response_MouseLeave(object sender, MouseEventArgs e) => animateBorderColor(Response, "#181818", 0.5);

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string username = File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "username.txt"));
            Response_Text.Text = "";
            typewrite(Response_Text, "Chat cleared", 5);
            chat_logs = new List<AI_Message>()
            {
                new AI_Message { role = "system", content = "More context will be given in the next message" },
            };
            chat_logs.Add(new AI_Message { role = "system", content = $"You are an helpful assistant The user's name is: \"{username}\". Lastly, NEVER use markdown formatting or code blocks, simply say it as plain text." });
        }
    }
}
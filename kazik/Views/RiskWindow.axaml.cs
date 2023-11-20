using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;

namespace kazik.Views;

public partial class RiskWindow : Window
{
     private ulong coins;

        private TextBlock[] cards = new TextBlock[4];

        private ulong[] nums = new ulong[4];

        private ulong mainnum;
        public RiskWindow(ulong coins)
        {
            InitializeComponent();

            this.coins = coins;

            coinsText.Text = coins.ToString();

            cards[0] = card0;
            cards[1] = card1;
            cards[2] = card2;
            cards[3] = card3;

            collectButton.Click += delegate
            {
                StaticData.wincoins = this.coins;
                Close(null);
            };

            cardborder0.DoubleTapped += delegate { ChooseCard(0); };
            cardborder1.DoubleTapped += delegate { ChooseCard(1); };
            cardborder2.DoubleTapped += delegate { ChooseCard(2); };
            cardborder3.DoubleTapped += delegate { ChooseCard(3); };

            SetGame();
        }

        public void SetGame()
        {
            LockCards(true);
            maincardborder.Background = Brushes.Gray;
            Random rand = new Random();
            foreach (var card in cards)
            {
                card.Text = "";
            }

            mainnum = (ulong)rand.Next(1, 9);

            for (int i = 0; i < nums.Length; i++)
            {
                nums[i] = (ulong)rand.Next(0, 10);
                Debug.WriteLine(nums[i]);
            }

            maincard.Text = mainnum.ToString();
        }

        public async void ChooseCard(int i)
        {
            await Task.Run( async () =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    collectButton.IsEnabled = false;
                    LockCards(false);
                    for (int i = 0; i < nums.Length; i++)
                    {
                        cards[i].Text = nums[i].ToString();
                    }
                    
                    switch (i)
                    {
                        case 0:
                            cardborder0.Background = Brushes.DarkBlue;
                            break;
                        case 1:
                            cardborder1.Background = Brushes.DarkBlue;
                            break;
                        case 2:
                            cardborder2.Background = Brushes.DarkBlue;
                            break;
                        case 3:
                            cardborder3.Background = Brushes.DarkBlue;
                            break;
                    }
                });
                
                
                ulong coef = 0;

                Dispatcher.UIThread.Post(() =>
                {
                    if (mainnum <= nums[i])
                    {
                        maincardborder.Background = Brushes.Green;
                        coef = 2;
                    }
                    else
                    {
                        maincardborder.Background = Brushes.Red;
                        coef = 0;
                    }              
                });

                await Task.Delay(2000);

                Dispatcher.UIThread.Post(() =>
                {
                    coins *= coef;
                    coinsText.Text = coins.ToString();
                });

                await Task.Delay(1000);

                Dispatcher.UIThread.Post(() =>
                {
                    collectButton.IsEnabled = true; 
                    if (coef > 0)
                    {
                        SetGame();
                    }
                });
            });
        }

        public void LockCards(bool lok)
        {
            if (lok)
            {
                cardborder0.IsEnabled = true;
                cardborder1.IsEnabled = true;
                cardborder2.IsEnabled = true;
                cardborder3.IsEnabled = true;
                cardborder0.Background = Brushes.Gray;
                cardborder1.Background = Brushes.Gray;
                cardborder2.Background = Brushes.Gray;
                cardborder3.Background = Brushes.Gray;
            }
            else
            {
                cardborder0.IsEnabled = false;
                cardborder1.IsEnabled = false;
                cardborder2.IsEnabled = false;
                cardborder3.IsEnabled = false;
            }
        }
}
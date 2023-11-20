using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using kazik.ViewModels;
using ReactiveUI;

namespace kazik.Views;

public partial class MainWindow : Window
{
    static CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
    CancellationToken token = cancelTokenSource.Token;
    

    public ulong bet = 0;

    public float coef;
    
    public TextBlock[] slots = new TextBlock[5];
    public MainWindow()
    {
        InitializeComponent();
        zashecoinsText.Text = StaticData.zashecoins.ToString();
        riskButton.IsVisible = false;
        winCoinsText.IsVisible = false;
        
        this.KeyUp += (sender, args) =>
        {
            if (args.Key == Key.Enter && rollButton.IsEnabled)
            {
                R();
            }
        };

        rollButton.Click += delegate { R(); };
        riskButton.Click += delegate
        {
            cancelTokenSource.Cancel();
            
            RiskWindow rk = new RiskWindow(StaticData.wincoins);
            StaticData.wincoins = 0;
            rk.Closed += delegate
            {
                StaticData.zashecoins += StaticData.wincoins;
                StaticData.wincoins = 0;
                winCoinsText.Text = StaticData.wincoins.ToString();
                zashecoinsText.Text = StaticData.zashecoins.ToString();
            };
            rk.ShowDialog(this);
        };

        slots[0] = slot0;
        slots[1] = slot1;
        slots[2] = slot2;
        slots[3] = slot3;
        slots[4] = slot4;
    }

    public void R()
    {
        cancelTokenSource.Cancel();
        Roll();
    }

    public async void Roll()
    {
        ulong b;
        if (!UInt64.TryParse(betBox.Text, out b))
        {
            return;
        }

        if (b <= 0 || b > StaticData.zashecoins)
        {
            return;
        }
        
        Debug.WriteLine(b);

        StaticData.zashecoins -= b;
        
        StaticData.zashecoins += StaticData.wincoins;
        zashecoinsText.Text = StaticData.zashecoins.ToString();
        cancelTokenSource.Cancel();
        StaticData.wincoins = 0;
        winCoinsText.Text = StaticData.wincoins.ToString();
        winCoinsText.IsVisible = true;
        bet = b;
        betBox.IsEnabled = false;
        riskButton.IsVisible = false;
        rollButton.IsEnabled = false;
        foreach (var item in slots)
        {
            item.Background = Brushes.Transparent;
            item.Text = "";
        }
        
        int[] rols = new int[5];
        int sleepTime = 200;
        Random rand = new Random();
        cancelTokenSource = new CancellationTokenSource();
        token = cancelTokenSource.Token;
        await Task.Run(async () =>
            {
                for (int i = 0; i < rols.Length; i++)
                {
                    int l = 2;
                    while (l < 80)
                    {
                        int r = rand.Next(1, 6);
                        Dispatcher.UIThread.Post(() => { slots[i].Text = r.ToString(); });
                        await Task.Delay(l);
                        l += 5;
                    }

                    rols[i] = rand.Next(1, 6);
                    Dispatcher.UIThread.Post(() => { slots[i].Text = rols[i].ToString(); });
                    sleepTime += 50;
                    await Task.Delay(sleepTime);
                }

                Dispatcher.UIThread.Post(() =>
                {
                    CheckResult(rols);
                    StaticData.wincoins = Convert.ToUInt64(bet * coef);
                    winCoinsText.Text = StaticData.wincoins.ToString();
                    betBox.IsEnabled = true;
                    rollButton.IsEnabled = true;
                    if (StaticData.wincoins > 0)
                    {
                        riskButton.IsVisible = true;
                    }
                });
                
                    await Task.Delay(2000);

                    Dispatcher.UIThread.Post(() =>
                    {
                        StaticData.zashecoins += StaticData.wincoins;
                        StaticData.wincoins = 0;
                        riskButton.IsVisible = false;
                        zashecoinsText.Text = StaticData.zashecoins.ToString();
                    });
            }, token).ConfigureAwait(false);
    }

    public void CheckResult(int[] rols)
    {
        coef = 0;
        /*
        for (int i = 0; i < rols.Length; i++)
        {
            for (int j = i + 1; j < rols.Length - 1; j++)
            {
                if (rols[i] != rols[j - 1])
                {
                    break;
                }
                else
                {
                    slots[j].Background = Brushes.OrangeRed;
                }
            }
        }
        */

        /*
        if (rols[0] == rols[1])
        {
            coef += 0.6f;
            slots[0].Background = Brushes.OrangeRed;
            slots[1].Background = Brushes.OrangeRed;
            if (rols[1] == rols[2])
            {
                coef += 0.8f;
                slots[2].Background = Brushes.OrangeRed;
                if (rols[2] == rols[3])
                {
                    coef += 1f;
                    slots[3].Background = Brushes.OrangeRed;
                }
            }
        }
        
        if (rols[1] == rols[2])
        {
            coef += 0.6f;
            slots[1].Background = Brushes.Pink;
            slots[2].Background = Brushes.Pink;
            if (rols[2] == rols[3])
            {
                coef += 0.8f;
                slots[3].Background = Brushes.Pink;
            }
        }
        
        if (rols[2] == rols[3])
        {
            coef += 0.6f;
            slots[2].Background = Brushes.Gray;
            slots[3].Background = Brushes.Gray;
        }
        */
        
        List<HashSet<int>> ll = new List<HashSet<int>>();

        for (int i = 0; i < rols.Length - 1; i++)
        {
            int n = rols[i];

            if (rols[i + 1] == n)
            {
                float cef = 0.4f;
                HashSet<int> rp = new HashSet<int>();
                for (int j = i + 1; j < rols.Length; j++)
                {
                    rp.Add(i);
                    if (rols[j] == n)
                    {
                        cef *= 2.1f;
                        rp.Add(j);
                    }
                    else
                    {
                        i = j - 1;
                        break;
                    }
                }

                coef += cef;
                ll.Add(rp);
            }
        }

        Random r = new Random();

        Brush[] br = new Brush[5];

        for (int i = 0; i < br.Length; i++)
        {
            br[i] = new SolidColorBrush(Color.FromRgb((byte)r.Next(50, 200), 
                (byte)r.Next(50, 200), (byte)r.Next(50, 200)));
        }

        int bri = 0;
        
        foreach (var item in ll)
        {
            List<int> itms = item.ToList();
            foreach (var it in itms)
            {
                slots[it].Background = br[bri];
            }

            bri++;
        }
    }
}
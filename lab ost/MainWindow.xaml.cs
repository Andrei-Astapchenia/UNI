using FFS.Models;
using FFS.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FFS
{
    public partial class MainWindow : Window
    {
        private SimulationEngine engine;
        private DispatcherTimer animationTimer;
        private DispatcherTimer uiTimer;
        private List<TextBlock> cashierTexts = new List<TextBlock>();
        private List<TextBlock> cookTexts = new List<TextBlock>();
        private List<TextBlock> serverTexts = new List<TextBlock>();
        private TextBlock kitchenQueueText;
        private TextBlock servingQueueText;

        public MainWindow()
        {
            InitializeComponent();

            // Анимация прогрузки
            animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
            animationTimer.Tick += (s, e) => AnimateCustomers();
            // Обновление UI
            uiTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            uiTimer.Tick += (s, e) => UpdateTexts();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ReadInputs(out int customerInterval, out int cookInterval, out int cashierInterval, out int serveInterval,
                out int cashiers, out int cooks, out int servers))
                return;
            engine = new SimulationEngine();
            engine.StateChanged += OnEngineChanged;
            ClearCanvas();
            DrawZones();
            DrawEmployeeCards(cashiers, cooks, servers);
            engine.Start(customerInterval, cashierInterval, cookInterval, cashierInterval, cooks, servers, cashiers);
            animationTimer.Start();
            uiTimer.Start();

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            engine?.Stop();
            animationTimer.Stop();
            uiTimer.Stop();
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

        private bool ReadInputs(out int customerInterval, out int cookInterval, out int cashierInterval, out int serveInterval,
                        out int cashiers, out int cooks, out int servers)
        {
            customerInterval = cookInterval = cashierInterval = serveInterval = cashiers = cooks = servers = 0;

            if (!int.TryParse(CustomerIntervalBox.Text, out customerInterval) || customerInterval <= 0)
            { MessageBox.Show("Интервал клиентов должен быть положительным!"); return false; }
            if (!int.TryParse(CookIntervalBox.Text, out cookInterval) || cookInterval <= 0)
            { MessageBox.Show("Интервал готовки должен быть положительным!"); return false; }
            if (!int.TryParse(ServeIntervalBox.Text, out serveInterval) || serveInterval <= 0)
            { MessageBox.Show("Интервал выдачи должен быть положительным!"); return false; }
            if (!int.TryParse(CashierIntervalBox.Text, out cashierInterval) || cashierInterval <= 0)
            { MessageBox.Show("Интервал кассира должен быть положительным!"); return false; }

            if (!int.TryParse(CashierCountBox.Text, out cashiers) || cashiers < 1 || cashiers > 3)
            { MessageBox.Show("Кассиров: от 1 до 3"); return false; }

            if (!int.TryParse(CookCountBox.Text, out cooks) || cooks < 1 || cooks > 4)
            { MessageBox.Show("Поваров: от 1 до 4"); return false; }

            if (!int.TryParse(ServerCountBox.Text, out servers) || servers < 1 || servers > 2)
            { MessageBox.Show("Официантов: от 1 до 2"); return false; }

            return true;
        }


        private void ClearCanvas()
        {
            VisualizationCanvas.Children.Clear();
            cashierTexts.Clear();
            cookTexts.Clear();
            serverTexts.Clear();
            kitchenQueueText = null;
            servingQueueText = null;
        }
        private void DrawZones()
        {
            DrawZone(SimulationEngine.QueueX, SimulationEngine.QueueY,
                     SimulationEngine.QueueW, SimulationEngine.QueueH,
                     "#1976D2", "#E3F2FD", "ОЧЕРЕДЬ");

            DrawZone(SimulationEngine.CashierX, SimulationEngine.CashierY,
                     SimulationEngine.CashierW, SimulationEngine.CashierH,
                     "#F57C00", "#FFF3E0", "КАССЫ");

            DrawZone(SimulationEngine.WaitingX, SimulationEngine.WaitingY,
                     SimulationEngine.WaitingW, SimulationEngine.WaitingH,
                     "#388E3C", "#E8F5E9", "ЗОНА ОЖИДАНИЯ");

            DrawZone(SimulationEngine.ServingX, SimulationEngine.ServingY,
                     SimulationEngine.ServingW, SimulationEngine.ServingH,
                     "#1B5E20", "#C8E6C9", "ВЫДАЧА");

            DrawZone(SimulationEngine.KitchenX, SimulationEngine.KitchenY,
                     SimulationEngine.KitchenW, SimulationEngine.KitchenH,
                     "#B71C1C", "#FFEBEE", "КУХНЯ (закрытая зона)");
        }

        private void DrawZone(double x, double y, double w, double h,
                              string borderColor, string fillColor, string title)
        {
            Rectangle rect = new Rectangle
            {
                Width = w,
                Height = h,
                Stroke = (Brush)new BrushConverter().ConvertFromString(borderColor),
                StrokeThickness = 3,
                Fill = (Brush)new BrushConverter().ConvertFromString(fillColor),
                Opacity = 0.7,
                RadiusX = 10,
                RadiusY = 10
            };
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            VisualizationCanvas.Children.Add(rect);

            TextBlock titleBlock = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)new BrushConverter().ConvertFromString(borderColor),
                FontSize = 14
            };
            Canvas.SetLeft(titleBlock, x + 10);
            Canvas.SetTop(titleBlock, y - 25);
            VisualizationCanvas.Children.Add(titleBlock);
        }
        private void DrawEmployeeCards(int cashierCount, int cookCount, int serverCount)
        {
            // Кассиры нарисованы столбиком 
            for (int i = 0; i < cashierCount; i++)
            {
                double x = SimulationEngine.CashierX + 20;
                double y = SimulationEngine.CashierY + 30 + i * 150;
                TextBlock text = DrawEmployeeCard(x, y, 140, 120, "#F57C00",
                    "Кассир " + (i + 1) + "\nСвободен");
                cashierTexts.Add(text);
            }
            // Повара нарисованы в рядок
            double step = SimulationEngine.KitchenW / (cookCount + 1);
            for (int i = 0; i < cookCount; i++)
            {
                double x = SimulationEngine.KitchenX + step * (i + 1) - 90;
                double y = SimulationEngine.KitchenY + 30;
                TextBlock text = DrawEmployeeCard(x, y, 180, 110, "#B71C1C",
                    "Повар " + (i + 1) + "\nСвободен");
                cookTexts.Add(text);
            }
            // Официанты столбиком 
            for (int i = 0; i < serverCount; i++)
            {
                double x = SimulationEngine.ServingX + 20;
                double y = SimulationEngine.ServingY + 30 + i * 110;
                TextBlock text = DrawEmployeeCard(x, y, 160, 90, "#1B5E20",
                    "Официант " + (i + 1) + "\nСвободен");
                serverTexts.Add(text);
            }
            //"Очередь заказов" 
            kitchenQueueText = new TextBlock
            {
                Text = "Очередь: пусто",
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)new BrushConverter().ConvertFromString("#B71C1C"),
                Width = SimulationEngine.KitchenW - 20,
                TextAlignment = TextAlignment.Center
            };
            Canvas.SetLeft(kitchenQueueText, SimulationEngine.KitchenX + 10);
            Canvas.SetTop(kitchenQueueText, SimulationEngine.KitchenY + SimulationEngine.KitchenH - 30);
            VisualizationCanvas.Children.Add(kitchenQueueText);
            servingQueueText = new TextBlock
            {
                Text = "Очередь: пусто",
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)new BrushConverter().ConvertFromString("#1B5E20"),
                Width = SimulationEngine.ServingW - 20,
                TextAlignment = TextAlignment.Center
            };
            Canvas.SetLeft(servingQueueText, SimulationEngine.ServingX + 10);
            Canvas.SetTop(servingQueueText, SimulationEngine.ServingY + SimulationEngine.ServingH - 30);
            VisualizationCanvas.Children.Add(servingQueueText);
        }

        private TextBlock DrawEmployeeCard(double x, double y, double w, double h,
                                            string color, string text)
        {
            Border card = new Border
            {
                Width = w,
                Height = h,
                Background = Brushes.White,
                BorderBrush = (Brush)new BrushConverter().ConvertFromString(color),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(8)
            };

            TextBlock textBlock = new TextBlock
            {
                Text = text,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)new BrushConverter().ConvertFromString(color),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            card.Child = textBlock;

            Canvas.SetLeft(card, x);
            Canvas.SetTop(card, y);
            VisualizationCanvas.Children.Add(card);

            return textBlock;
        }
        private void OnEngineChanged()
        {
            Dispatcher.Invoke(() => AddNewCustomerVisuals());
        }

        private void AddNewCustomerVisuals()
        {
            if (engine == null) return;
            List<Customer> copy;
            lock (engine.CustomersLock)
            {
                copy = new List<Customer>(engine.Customers);
            }

            foreach (Customer customer in copy)
            {
                if (customer.Visual != null) continue;

                Grid group = new Grid { Width = 32, Height = 32 };

                Ellipse circle = new Ellipse
                {
                    Width = 32,
                    Height = 32,
                    Fill = Brushes.SteelBlue,
                    Stroke = Brushes.White,
                    StrokeThickness = 2
                };

                TextBlock idText = new TextBlock
                {
                    Text = customer.Id.ToString(),
                    FontSize = 12,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                group.Children.Add(circle);
                group.Children.Add(idText);

                customer.X = -50;
                customer.Y = 200;

                Canvas.SetLeft(group, customer.X);
                Canvas.SetTop(group, customer.Y);
                VisualizationCanvas.Children.Add(group);

                customer.Visual = group;
                customer.TargetX = SimulationEngine.QueueX + 40;
                customer.TargetY = SimulationEngine.QueueY + 50;
            }
        }
        private void AnimateCustomers()
        {
            if (engine == null) return;

            List<Customer> copy;
            lock (engine.CustomersLock)
            {
                copy = new List<Customer>(engine.Customers);
            }

            List<Customer> leaving = new List<Customer>();

            foreach (Customer customer in copy)
            {
                if (customer.Visual is not Grid group) continue;

                if (group.Children[0] is Ellipse circle)
                {
                    switch (customer.State)
                    {
                        case CustomerState.InQueue: circle.Fill = Brushes.DodgerBlue; break;
                        case CustomerState.WalkingToCashier: circle.Fill = Brushes.Orange; break;
                        case CustomerState.AtCashier: circle.Fill = Brushes.OrangeRed; break;
                        case CustomerState.WaitingForFood: circle.Fill = Brushes.Gold; break;
                        case CustomerState.WaitingForPickup: circle.Fill = Brushes.Violet; break;
                        case CustomerState.Leaving: circle.Fill = Brushes.LightGreen; break;
                    }
                }

                customer.X += (customer.TargetX - customer.X) * 0.08;
                customer.Y += (customer.TargetY - customer.Y) * 0.08;

                Canvas.SetLeft(group, customer.X);
                Canvas.SetTop(group, customer.Y);

                if (customer.State == CustomerState.Leaving && customer.X > 1250)
                    leaving.Add(customer);
            }
            foreach (Customer customer in leaving)
            {
                if (customer.Visual is Grid group)
                    VisualizationCanvas.Children.Remove(group);

                lock (engine.CustomersLock)
                {
                    engine.Customers.Remove(customer);
                }
            }
        }
        private void UpdateTexts()
        {
            if (engine == null) return;

            // Общая статистика
            lock (engine.CustomersLock)
            {
                TotalCustomersText.Text = engine.Customers.Count.ToString();
            }
            OrderQueueText.Text = engine.OrderQueue.Count.ToString();

            KitchenText.Text = engine.CookingTickets.Count == 0
                ? "—"
                : string.Join(", ", engine.CookingTickets.Select(t => t.ToString()));

            ServingText.Text = engine.ServingTickets.Count == 0
                ? "—"
                : string.Join(", ", engine.ServingTickets.Select(t => t.ToString()));

            // Очередь заказов на кухне
            if (kitchenQueueText != null)
            {
                if (engine.KitchenWaitingNumbers.Count == 0)
                    kitchenQueueText.Text = "Очередь: пусто";
                else
                    kitchenQueueText.Text = "Очередь: " + string.Join(", ", engine.KitchenWaitingNumbers);
            }

            // Очередь заказов на выдаче
            if (servingQueueText != null)
            {
                if (engine.ServingWaitingNumbers.Count == 0)
                    servingQueueText.Text = "Очередь: пусто";
                else
                    servingQueueText.Text = "Очередь: " + string.Join(", ", engine.ServingWaitingNumbers);
            }

            // Кассиры
            for (int i = 0; i < cashierTexts.Count; i++)
            {
                Cashier cashier = engine.Cashiers.ElementAtOrDefault(i);
                if (cashier == null) continue;

                if (cashier.IsBusy)
                    cashierTexts[i].Text = "Кассир " + (i + 1) + "\n\nПринимает\n" + cashier.CurrentTicket;
                else
                    cashierTexts[i].Text = "Кассир " + (i + 1) + "\n\nСвободен";
            }

            // Повара
            for (int i = 0; i < cookTexts.Count; i++)
            {
                if (i < engine.CookingTickets.Count)
                    cookTexts[i].Text = "Повар " + (i + 1) + "\n\nГотовит " + engine.CookingTickets[i];
                else
                    cookTexts[i].Text = "Повар " + (i + 1) + "\n\nСвободен";
            }

            // Официанты
            for (int i = 0; i < serverTexts.Count; i++)
            {
                if (i < engine.ServingTickets.Count)
                    serverTexts[i].Text = "Официант " + (i + 1) + "\n\nВыдаёт " + engine.ServingTickets[i];
                else
                    serverTexts[i].Text = "Официант " + (i + 1) + "\n\nСвободен";
            }
        }
    }
}
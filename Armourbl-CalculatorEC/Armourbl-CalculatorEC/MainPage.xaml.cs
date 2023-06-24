using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace Armourbl_CalculatorEC
{
    public partial class MainPage : ContentPage
    {
        List<string> calculationSequence = new List<string>();
        string currentNumber = "";

        public MainPage()
        {
            InitializeComponent();

            var requestedTheme = App.Current.RequestedTheme;

            if (requestedTheme == OSAppTheme.Light)
            {
                UpdateTheme(OSAppTheme.Light);
            }
            else if (requestedTheme == OSAppTheme.Dark)
            {
                UpdateTheme(OSAppTheme.Dark);
            }
        }

        public string GetResultText()
        {
            return result.Text;
        }

        public void ResetCalculator()
        {
            calculationSequence.Clear();
            currentNumber = "";
            result.Text = "";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var metrics = DeviceDisplay.MainDisplayInfo;

            var buttonSize = Math.Min(metrics.Width / metrics.Density / 5, 60);

            foreach (var child in grid.Children)
            {
                if (child is Button button)
                {
                    button.WidthRequest = buttonSize;
                    button.HeightRequest = buttonSize;
                    button.CornerRadius = (int)(buttonSize / 1);
                }
            }
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();

            if (Parent != null)
            {
                App.Current.RequestedThemeChanged += OnRequestedThemeChanged;
            }
            else
            {
                App.Current.RequestedThemeChanged -= OnRequestedThemeChanged;
            }
        }

        void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
        {
            UpdateTheme(e.RequestedTheme);
        }

        public void UpdateTheme(OSAppTheme theme)
        {
            if (theme == OSAppTheme.Dark)
            {
                BackgroundColor = Color.Black;
                result.TextColor = Color.White;

                foreach (var child in grid.Children)
                {
                    if (child is Button button)
                    {
                        switch (button.Text)
                        {
                            case "+":
                            case "-":
                            case "x":
                            case "÷":
                            case "=":
                                button.Style = (Style)Resources["DarkOperatorButtonStyle"];
                                break;
                            case "AC":
                            case "+/-":
                            case "%":
                                button.Style = (Style)Resources["DarkFunctionButtonStyle"];
                                break;
                            default:
                                button.Style = (Style)Resources["DarkNumberButtonStyle"];
                                break;
                        }
                    }
                }
            }
            else
            {
                BackgroundColor = Color.White;
                result.TextColor = Color.Black;

                foreach (var child in grid.Children)
                {
                    if (child is Button button)
                    {
                        switch (button.Text)
                        {
                            case "+":
                            case "-":
                            case "x":
                            case "÷":
                            case "=":
                                button.Style = (Style)Resources["LightOperatorButtonStyle"];
                                break;
                            case "AC":
                            case "+/-":
                            case "%":
                                button.Style = (Style)Resources["LightFunctionButtonStyle"];
                                break;
                            default:
                                button.Style = (Style)Resources["LightNumberButtonStyle"];
                                break;
                        }
                    }
                }
            }
        }

        void OnSelectNumber(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string pressed = button.Text;
            currentNumber += pressed;
            this.result.Text += pressed;
        }

        void OnSelectOperator(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string pressed = button.Text;

            if (string.IsNullOrEmpty(currentNumber))
            {
                if (calculationSequence.Count > 0)
                {
                    currentNumber = calculationSequence.Last();
                }
                else
                {
                    return;
                }
            }

            if (double.TryParse(currentNumber, out double number))
            {
                var oldLength = currentNumber.Length;
                var removeStartIndex = this.result.Text.Length - oldLength;

                switch (pressed)
                {
                    case "+/-":
                        number = -number;
                        break;
                    case "%":
                        number = number / 100;
                        break;
                }

                currentNumber = number.ToString();

                this.result.Text = this.result.Text.Remove(removeStartIndex) + currentNumber;

                calculationSequence.Add(currentNumber);
                currentNumber = "";

                switch (pressed)
                {
                    case "+":
                    case "-":
                    case "x":
                    case "÷":
                        calculationSequence.Add(pressed);
                        this.result.Text += " " + pressed + " ";
                        break;
                    case "=":
                        OnCalculate(sender, e);
                        break;
                }
            }
        }

        void OnClear(object sender, EventArgs e)
        {
            calculationSequence.Clear();
            currentNumber = "";
            result.Text = "";
        }

        void OnCalculate(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentNumber))
            {
                calculationSequence.Add(currentNumber);
                currentNumber = "";
            }

            var postfix = InfixToPostfix(calculationSequence);
            var resultNumber = EvaluatePostfix(postfix);
            currentNumber = resultNumber.ToString();
            result.Text = currentNumber;
            calculationSequence.Clear();
            calculationSequence.Add(resultNumber.ToString());
        }

        int Precedence(string operatorToken)
        {
            switch (operatorToken)
            {
                case "+":
                case "-":
                    return 1;
                case "x":
                case "÷":
                    return 2;
                case "%":
                    return 3;
                case "u-":
                    return 4;
                default:
                    throw new ArgumentException($"Unknown operator: {operatorToken}");
            }
        }

        List<string> InfixToPostfix(List<string> infix)
        {
            var postfix = new List<string>();
            var stack = new Stack<string>();

            for (int i = 0; i < infix.Count; i++)
            {
                var token = infix[i];

                if (Double.TryParse(token, out _))
                {
                    postfix.Add(token);
                }
                else if (token == "(")
                {
                    stack.Push(token);
                }
                else if (token == ")")
                {
                    while (stack.Peek() != "(")
                    {
                        postfix.Add(stack.Pop());
                    }
                    stack.Pop();
                }
                else if (token == "-" && (i == 0 || infix[i - 1] == "("))
                {
                    stack.Push("u-");
                }
                else
                {
                    while (stack.Any() && Precedence(stack.Peek()) >= Precedence(token))
                    {
                        postfix.Add(stack.Pop());
                    }
                    stack.Push(token);
                }
            }

            while (stack.Any())
            {
                postfix.Add(stack.Pop());
            }

            return postfix;
        }

        double EvaluatePostfix(List<string> postfix)
        {
            var stack = new Stack<double>();

            foreach (var token in postfix)
            {
                if (Double.TryParse(token, out double number))
                {
                    stack.Push(number);
                }
                else
                {
                    var rightOperand = stack.Pop();
                    var leftOperand = token == "u-" ? 0 : stack.Pop();

                    switch (token)
                    {
                        case "+":
                            stack.Push(leftOperand + rightOperand);
                            break;
                        case "-":
                        case "u-":
                            stack.Push(leftOperand - rightOperand);
                            break;
                        case "x":
                            stack.Push(leftOperand * rightOperand);
                            break;
                        case "÷":
                            if (rightOperand != 0)
                            {
                                stack.Push(leftOperand / rightOperand);
                            }
                            else
                            {
                                throw new InvalidOperationException("Division by zero is not allowed");
                            }
                            break;
                        case "%":
                            stack.Push(leftOperand * rightOperand / 100);
                            break;
                        default:
                            throw new ArgumentException($"Unknown operator: {token}");
                    }
                }
            }

            return stack.Pop();
        }
    }
}
